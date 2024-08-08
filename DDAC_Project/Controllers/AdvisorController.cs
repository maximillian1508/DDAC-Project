using DDAC_Project.Areas.Identity.Data;
using DDAC_Project.Areas.Identity.Pages.Account.Manage;
using DDAC_Project.Data;
using DDAC_Project.Models;
using DDAC_Project.Views.Advisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DDAC_Project.Controllers
{
    public class AdvisorController : Controller
    {
        private readonly DDAC_ProjectContext _context;
        private readonly UserManager<DDAC_ProjectUser> _userManager;
        private readonly SignInManager<DDAC_ProjectUser> _signInManager;
        public AdvisorController(DDAC_ProjectContext context, UserManager<DDAC_ProjectUser> userManager, SignInManager<DDAC_ProjectUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public class IndexViewModel
        {
            public int? ClientCount { get; set; }
            public int? CommentCount { get; set; }
            public decimal ManagedAssets { get; set; }
            public List<UserModel> AssignedClients { get; set; }
            public List<ClientAssetInfo> ClientAssets { get; set; }

        }

        public class FinancialAnalysisModel
        {
            public string FullName { get; set; }
            public decimal TotalIncome { get; set; }
            public decimal TotalExpense { get; set; }
            public decimal NetBalance { get; set; }
            public List<IncomeCategoryData> CategoryIncomeData { get; set; }
            public List<ExpenseCategoryData> CategoryExpenseData { get; set; }
            public List<TransactionHistoryItem> TransactionHistory { get; set; }
            public List<CommentModel> CommentHistory { get; set; }
            public int ClientId { get; set; }
            public DateTime CurrentDate { get; set; }
            public List<DateTime> AvailableMonths { get; set; }
        }

        public class UserModel
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string ClientID { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
        }

        public class SelectUserModel
        {
            public List<UserModel> Users { get; set; }
        }

        private List<int> GetClientIdsForAdvisor(int advisorId)
        {
            return _context.Clients
                .Where(c => c.AdvisorId == advisorId)
                .Select(c => c.ClientId)
                .ToList();
        }

        public class ClientAssetInfo
        {
            public string Name { get; set; }
            public decimal Assets { get; set; }
        }

        public class IncomeCategoryData
        {
            public string CategoryName { get; set; }
            public decimal TotalIncome { get; set; }
        }

        public class ExpenseCategoryData
        {
            public string CategoryName { get; set; }
            public decimal TotalExpense { get; set; }
        }

        public class TransactionHistoryItem
        {
            public int TransactionId { get; set; }
            public string GoalName { get; set; }
            public string CategoryName { get; set; }
            public decimal Amount { get; set; }
            public DateTime Date { get; set; }
            public string Description { get; set; }

            public string Type { get; set; }
        }

        public class CommentModel
        {
            public string CommentText { get; set; }
            public DateTime Date { get; set; }

            public string AdvisorName { get; set; }
        }

        [Route("/advisor-dashboard")]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> Index()
        {
            //var user = await _userManager.GetUserAsync(User);
            var advisorId = Convert.ToInt32(HttpContext.Session.GetInt32("AdvisorId"));

            if (advisorId == 0) 
            {
                return Challenge();
            }

            var totalClient = await _context.Clients.Where(client => client.AdvisorId == advisorId).CountAsync();

            var totalComment = await _context.Comments.Where(comment => comment.AdvisorId == advisorId).CountAsync();

            var clientIds = GetClientIdsForAdvisor(advisorId);
            var totalIncome = await _context.Transactions
                .Where(t => clientIds.Contains(t.ClientId) && t.Category.Type == "Income")
                .SumAsync(t => t.Amount); 
            
            var totalExpense = await _context.Transactions
                .Where(t => clientIds.Contains(t.ClientId) && t.Category.Type == "Expense")
                .SumAsync(t => t.Amount);

            var totalManagedAssets = totalIncome - totalExpense;

            var assignedClients = await _context.Clients
                .Where(c => c.AdvisorId == advisorId)
                .Select(c => new
                {
                    userModel = new UserModel
                    {
                        FirstName = c.User.FirstName,
                        LastName = c.User.LastName,
                        Email = c.User.Email,
                        PhoneNumber = c.User.PhoneNumber,
                        ClientID = c.ClientId.ToString()
                    },
                    TotalAssets = _context.Transactions.Where(t => t.ClientId == c.ClientId && t.Category.Type == "Income").Sum(t => t.Amount) - 
                                     _context.Transactions.Where(t => t.ClientId == c.ClientId && t.Category.Type == "Expense").Sum(t => t.Amount) 
                })
                .ToListAsync();

            var viewModel = new IndexViewModel
            {
                ClientCount = totalClient,
                CommentCount = totalComment,
                ManagedAssets = totalManagedAssets,
                AssignedClients = assignedClients.Select(ac => ac.userModel).ToList(),
                ClientAssets = assignedClients.Select(ac => new ClientAssetInfo
                {
                    Name = $"{ac.userModel.FirstName} {ac.userModel.LastName}",
                    Assets = ac.TotalAssets
                }).ToList()
            };

            return View("Index", viewModel);
        }

        [Authorize(Roles="Advisor")]
        [Route("/financial-analysis")]
        public async Task<IActionResult> FinancialAnalysis(int clientId,  DateTime? date = null)
        {
            var currentDate = date ?? DateTime.Now;

            var advisorId = Convert.ToInt32(HttpContext.Session.GetInt32("AdvisorId"));
            var user = await _userManager.GetUserAsync(User);

            if (advisorId == 0 && user.UserType == "Advisor")
            {
                return Challenge();
            } 

            var client = await _context.Clients.Include(c => c.User).FirstOrDefaultAsync(c => c.ClientId == clientId);
            if (client == null)
            {
                return NotFound();
            }

            var fullName = $"{client.User.FirstName} {client.User.LastName}";

            var transactions = await _context.Transactions.Where(t => t.ClientId == clientId && t.Date.Month == currentDate.Month && t.Date.Year == currentDate.Year).ToListAsync();
            var totalIncome = await _context.Transactions.Where(t => t.ClientId == clientId && t.Category.Type == "Income" && t.Date.Month == currentDate.Month && t.Date.Year == currentDate.Year).SumAsync(t => t.Amount);
            var totalExpense = await _context.Transactions.Where(t => t.ClientId == clientId && t.Category.Type == "Expense" && t.Date.Month == currentDate.Month && t.Date.Year == currentDate.Year).SumAsync(t => t.Amount);
            var netBalance = totalIncome - totalExpense;
            var categoryIncomeData = await _context.Categories
                .GroupJoin(
                    _context.Transactions.Where(t => t.ClientId == clientId && t.Category.Type == "Income" && t.Date.Month == currentDate.Month && t.Date.Year == currentDate.Year),
                    c => c.CategoryId,
                    t => t.CategoryId,
                    (c, ts) => new IncomeCategoryData
                    {
                        CategoryName = c.Name,
                        TotalIncome = ts.Sum(t => t.Amount),
                    })
                .Where(ic => ic.TotalIncome > 0)
                .ToListAsync();
            var categoryExpenseData = await _context.Categories
                .GroupJoin(
                    _context.Transactions.Where(t => t.ClientId == clientId && t.Category.Type == "Expense" && t.Date.Month == currentDate.Month && t.Date.Year == currentDate.Year),
                    c => c.CategoryId,
                    t => t.CategoryId,
                    (c, ts) => new ExpenseCategoryData
                    {
                        CategoryName = c.Name,
                        TotalExpense = ts.Sum(t => t.Amount)
                    })
                .Where(ic => ic.TotalExpense > 0)
                .ToListAsync();
            var transactionHistory = await _context.Transactions
                .Where(t => t.ClientId == clientId && t.Date.Month == currentDate.Month && t.Date.Year == currentDate.Year)
                .OrderByDescending(t => t.TransactionId)
                .Select(t => new TransactionHistoryItem
                {
                    TransactionId = t.TransactionId,
                    GoalName = t.Goal.Name,
                    CategoryName = t.Category.Name,
                    Amount = t.Amount,
                    Date = t.Date,
                    Description = t.Description,
                    Type = t.Category.Type,
                })
                .ToListAsync();
            var commentHistory = await _context.Comments
                .Where(c => c.ClientId == clientId && c.Date.Month == currentDate.Month && c.Date.Year == currentDate.Year)
                .OrderByDescending(c => c.Date)
                .Select(c => new CommentModel
                {
                    CommentText = c.CommentText,
                    Date = c.Date,
                    AdvisorName = c.Advisor.User.FirstName + " " + c.Advisor.User.LastName
                })
                .ToListAsync();

            var model = new FinancialAnalysisModel
            {
                FullName = fullName,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                NetBalance = netBalance,
                CategoryIncomeData = categoryIncomeData,
                CategoryExpenseData = categoryExpenseData,
                TransactionHistory = transactionHistory,
                CommentHistory = commentHistory,
                ClientId = clientId,
                CurrentDate = currentDate,
                AvailableMonths = GetAvailableMonths(currentDate)
            };

            return View(model);
        }

        private List<DateTime> GetAvailableMonths(DateTime currentDate)
        {
            var months = new List<DateTime>();
            for (int i = 0; i < 6; i++)
            {
                months.Add(currentDate.AddMonths(-i).Date);
            }
            return months;
        }

        [Authorize(Roles = "Client")]
        [Route("/client-financial-analysis")]
        public async Task<IActionResult> ClientFinancialAnalysis(DateTime?date=null)
        {
            var client = Convert.ToInt32(HttpContext.Session.GetInt32("ClientId"));

            if (client == 0)
            {
                return Challenge();
            }


            var result = await FinancialAnalysis(client, date) as ViewResult;
            if (result != null)
            {
                return View("FinancialAnalysis", result.Model);
            }
            return NotFound();
        }

        [Authorize(Roles = "Advisor")]
        [Route("/select-user")]
        public async Task<IActionResult> SelectUser()
        {
            var advisorId = Convert.ToInt32(HttpContext.Session.GetInt32("AdvisorId"));

            if (advisorId == 0)
            {
                return Challenge();
            }

            List<UserModel> assignedClients = await _context.Clients
                .Where(c => c.AdvisorId == advisorId)
                .Select(c => new UserModel
                {
                    FirstName = c.User.FirstName,
                    LastName = c.User.LastName,
                    ClientID = c.ClientId.ToString(),
                    Email = c.User.Email,
                    PhoneNumber = c.User.PhoneNumber,
                })
                .ToListAsync();

            return View(assignedClients);
        }

        [Authorize(Roles = "Advisor")]
        [HttpPost]
        public async Task<IActionResult> AddComment(int clientId, string commentText)
        {
            if (string.IsNullOrWhiteSpace(commentText))
            {
                return Json(new { success = false, message = "Comment text cannot be empty." });
            }

            var advisorId = Convert.ToInt32(HttpContext.Session.GetInt32("AdvisorId"));

            if (advisorId == 0)
            {
                return Json(new { success = false, message = "Unauthorized." });
            }

            var client = await _context.Clients.FindAsync(clientId);
            var advisor = await _context.Advisors.FindAsync(advisorId);

            if (client == null || advisor == null)
            {
                return Json(new { success = false, message = "Client or Advisor not found." });
            }

            var comment = new Comment
            {
                ClientId = clientId,
                AdvisorId = advisorId,
                CommentText = commentText,
                Date = DateTime.Now,
                Client = client,
                Advisor = advisor
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                commentText = comment.CommentText,
                date = comment.Date.ToString("MMM dd, yyyy HH:mm")
            });
        }
    }
}
