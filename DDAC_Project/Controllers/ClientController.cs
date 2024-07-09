using DDAC_Project.Data;
using Microsoft.AspNetCore.Mvc;
using DDAC_Project.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DDAC_Project.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DDAC_Project.Controllers
{
    public class ClientController : Controller
    {
        private readonly DDAC_ProjectContext _context;

        // Keep only this constructor
        public ClientController(DDAC_ProjectContext context)
        {
            _context = context;
        }

        public class IndexViewModel
        {
            public List<TransactionModel> RecentTransactions { get; set; }
            public int ? totalTransaction { get; set; }

            public decimal monthlyBudget { get; set; }

            public decimal totalMonthlyExpense { get; set; }

            public int monthlyBudgetCount { get; set; }
        }

        public class TransactionModel
        {
            public string? Description { get; set; }
            public decimal? Amount { get; set; }
            public string? Date{ get; set; }
            public string? GoalName { get; set; }
            public string? CategoryName { get; set; }
            public string? Type { get; set; }
        }

        public class GoalsViewModel
        {
            public int ? GoalId { get; set; }

            public string ? Name { get; set; }

            public decimal  TargetAmount { get; set; }

            public decimal Progress { get; set; }
        }

        public async Task<IActionResult> Index()
        {
            var clientId = Convert.ToInt32(HttpContext.Session.GetInt32("ClientId"));

            if (clientId == 0)
            {
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

                return Challenge();
            }

            var monthlyBudget = await _context.Budgets
                .Where(b => b.ClientId == clientId && b.Month.Month == DateTime.Now.Month && b.Month.Year == DateTime.Now.Year)
                .Select(b => b.Amount)
                .SingleOrDefaultAsync();

            ViewBag.NewBudget = new Budget
            {
                Amount = monthlyBudget == 0 ? 0 : monthlyBudget,
                ClientId = clientId,
                Client = await _context.Clients.FindAsync(clientId),
                Month = DateTime.Now,
            };

            var viewModel = new IndexViewModel
            {
                RecentTransactions = await _context.Transactions
                .Where(t => t.ClientId == clientId)
                .OrderByDescending(t => t.TransactionId)
                .Take(4)
                 .Select(t => new TransactionModel
                 {
                     Description = t.Description,
                     Amount = t.Amount,
                     Date = t.Date.ToLongDateString(),
                     GoalName = t.Goal != null ? t.Goal.Name : null,
                     CategoryName = t.Category.Name,
                     Type = t.Category.Type
                 })
                .ToListAsync(),
                totalTransaction = await _context.Transactions
                .Where(t => t.ClientId == clientId && t.Date.Month == DateTime.Now.Month && t.Date.Year == DateTime.Now.Year)
                .CountAsync(),
                monthlyBudget = monthlyBudget,
                totalMonthlyExpense = await _context.Transactions
                .Where(t => t.ClientId == clientId && t.Date.Month == DateTime.Now.Month && t.Date.Year == DateTime.Now.Year && t.Category.Type == "Expense")
                .SumAsync(t => t.Amount),
                monthlyBudgetCount = await _context.Budgets
                .Where(b => b.ClientId == clientId && b.Month.Month == DateTime.Now.Month && b.Month.Year == DateTime.Now.Year)
                .CountAsync()
            };
            return View(viewModel);
        }

        public IActionResult Analysis()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AddIncome()
        {
            var clientId = Convert.ToInt32(HttpContext.Session.GetInt32("ClientId"));

            // Fetch categories
            var categories = await _context.Categories
                .Where(c =>
                    (c.IsDefault == true && c.Type == "Income") ||
                    (c.ClientId == clientId && c.Type == "Income")
                )
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.Name
                })
                .ToListAsync();

            // Fetch goals
            var goals = await _context.Goals
                .Where(g => g.ClientId == clientId)
                .Select(g => new SelectListItem
                {
                    Value = g.GoalId.ToString(),
                    Text = g.Name
                })
                .ToListAsync();

            // Add a default "No Goal" option
            goals.Insert(0, new SelectListItem { Value = "", Text = "" });

            ViewBag.Categories = categories;
            ViewBag.Goals = goals;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> AddIncome(Transaction transaction)
        {

            var clientId = Convert.ToInt32(HttpContext.Session.GetInt32("ClientId"));
            transaction.ClientId = clientId;

            if (transaction.GoalId == 0)
            {
                transaction.GoalId = null;
            }

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> AddExpense()
        {
            var clientId = Convert.ToInt32(HttpContext.Session.GetInt32("ClientId"));

            // Fetch categories
            var categories = await _context.Categories
                .Where(c =>
                    (c.IsDefault == true && c.Type == "Expense") ||
                    (c.ClientId == clientId && c.Type == "Expense")
                )
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.Name
                })
                .ToListAsync();

            ViewBag.Categories = categories;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> AddExpense(Transaction transaction)
        {

            var clientId = Convert.ToInt32(HttpContext.Session.GetInt32("ClientId"));
            transaction.ClientId = clientId;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Goals()
        {
            var clientId = Convert.ToInt32(HttpContext.Session.GetInt32("ClientId"));

            if (clientId == 0)
            {
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

                return Challenge();
            }
            List<GoalsViewModel> goals = await _context.Goals
                .Where(goal => goal.ClientId == clientId)
                .Select(goal => new GoalsViewModel {
                    GoalId = goal.GoalId,
                    Name = goal.Name,
                    TargetAmount = goal.TargetAmount,
                    Progress =  _context.Transactions
                    .Where(t => t.ClientId == clientId && t.GoalId == goal.GoalId).Sum(t => t.Amount)
                })
                .ToListAsync();

            ViewBag.NewGoal = new Goal
            {
                Name = "",
                TargetAmount = 0,
                ClientId = clientId,
                Client = await _context.Clients.FindAsync(clientId)
            };

            return View(goals);
        }

        [HttpPost]
        public async Task<IActionResult> AddGoal(Goal newGoal)
        {
            var clientId = Convert.ToInt32(HttpContext.Session.GetInt32("ClientId"));
            newGoal.ClientId = clientId;
            Console.WriteLine(clientId);
            _context.Goals.Add(newGoal);
            await _context.SaveChangesAsync();

            return RedirectToAction("Goals");
        }

        public async Task<IActionResult> EditGoal(int? GoalId)
        {
            if (GoalId == null)
            {
                return NotFound();
            }
            var goal = await _context.Goals.FindAsync(GoalId);

            if (goal == null)
            {
                return BadRequest(GoalId + " is not found in the table!");
            }
            return View(goal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGoal(Goal goal)
        {
            try
            {
                var clientId = Convert.ToInt32(HttpContext.Session.GetInt32("ClientId"));
                goal.ClientId = clientId;
                _context.Goals.Update(goal);
                await _context.SaveChangesAsync();
                return RedirectToAction("Goals", "Client");
                //return View("EditGoal", goal);
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
        }

        public async Task<IActionResult> DeleteGoal(int? GoalId)
        {
            if (GoalId == null)
            {
                return NotFound();
            }
            var goal = await _context.Goals.FindAsync(GoalId);
            if (goal == null)
            {
                return BadRequest(GoalId + " is not found in the list!");
            }
            _context.Goals.Remove(goal);
            await _context.SaveChangesAsync();
            return RedirectToAction("Goals", "Client");
        }

        public async Task<IActionResult> AddBudget(Budget newBudget)
        {
            var clientId = Convert.ToInt32(HttpContext.Session.GetInt32("ClientId"));
            newBudget.ClientId = clientId;
            newBudget.Month = DateTime.Now;
            _context.Budgets.Add(newBudget);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> UpdateBudget(Budget newBudget)
        {
            var clientId = Convert.ToInt32(HttpContext.Session.GetInt32("ClientId"));

            var budgetId = await _context.Budgets
                .Where(b => b.ClientId == clientId && b.Month.Month == DateTime.Now.Month && b.Month.Year == DateTime.Now.Year)
                .Select(b => b.BudgetId)
                .SingleOrDefaultAsync();

            newBudget.BudgetId = budgetId;
            newBudget.Month = DateTime.Now;
            newBudget.ClientId = clientId;

            _context.Budgets.Update(newBudget);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}