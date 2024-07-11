using DDAC_Project.Areas.Identity.Data;
using DDAC_Project.Data;
using DDAC_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text;
using DDAC_Project.Areas.Identity.Pages.Account;
using static DDAC_Project.Controllers.AdvisorController;
using Microsoft.AspNetCore.Authentication;
using DDAC_Project.Constants;

namespace DDAC_Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly SignInManager<DDAC_ProjectUser> _signInManager;
        private readonly UserManager<DDAC_ProjectUser> _userManager;
        private readonly IUserEmailStore<DDAC_ProjectUser> _emailStore;
        private readonly IUserStore<DDAC_ProjectUser> _userStore;
        private readonly DDAC_ProjectContext _context;

        public AdminController(
            UserManager<DDAC_ProjectUser> userManager,
            IUserStore<DDAC_ProjectUser> userStore,
            SignInManager<DDAC_ProjectUser> signInManager,
            DDAC_ProjectContext context
            )
        {
            _userManager = userManager;
            _userStore = userStore;
            _signInManager = signInManager;
            _context = context;
        }

        public class IndexViewModel
        {
            public int? ClientCount { get; set; }
            public decimal ManagedAssets { get; set; }
            public decimal TransactionCount { get; set; }

            public List<TotalAssetModel> TotalAssetsPerUser { get; set; }

        }

        public class ManageUserViewModel
        {
            public string UserId { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string UserType { get; set; }
        }

        public class CreateAdminModel
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string PhoneNumber { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
        }

        public class CreateAdvisorModel
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string PhoneNumber { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
            public string Specialization { get; set; }
            public string YearOfExperience { get; set; }
        }

        public class TotalAssetModel
        {
            public string AdvisorEmail { get; set; }
            public string ClientEmail { get; set; }

            public string ClientName { get; set; }
            public decimal TotalAsset { get; set; }

            public string ClientUserId { get; set; }
        }

        [Route("/admin-dashboard")]
        public async Task<IActionResult> Index()
        {
            if (!User.IsInRole(UserRoles.Admin))
            {
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

                return Challenge();
            }
            var totalClient = await _context.Clients.CountAsync();

            var totalIncome = await _context.Transactions.Where(t => t.Category.Type == "Income").SumAsync(t => t.Amount);
            var totalExpense = await _context.Transactions.Where(t => t.Category.Type == "Expense").SumAsync(t => t.Amount);

            var totalManagedAssets = totalIncome - totalExpense;

            var totalTransactions = await _context.Transactions
                .Where(t => t.Date.Month == DateTime.Now.Month && t.Date.Year <= DateTime.Now.Year)
                .CountAsync();

            List<TotalAssetModel> totalAssetsPerUser = await _context.Transactions
                .GroupBy(t => new 
                { ClientUserId = t.Client.UserId, 
                  ClientEmail = t.Client.User.Email, 
                  AdvisorEmail = t.Client.Advisor.User.Email,
                  ClientFirstName = t.Client.User.FirstName,
                  ClientLastName = t.Client.User.LastName
                })
                .Select(g => new TotalAssetModel
                {
                    ClientEmail = g.Key.ClientEmail,
                    ClientUserId = g.Key.ClientUserId,
                    AdvisorEmail = g.Key.AdvisorEmail,
                    TotalAsset = g.Where(t => t.Category.Type == "Income").Sum(t => t.Amount) -
                                 g.Where(t => t.Category.Type == "Expense").Sum(t => t.Amount),
                    ClientName = g.Key.ClientFirstName + " " + g.Key.ClientLastName
                })
                .OrderByDescending(t => t.TotalAsset)
                .Take(4)
                .ToListAsync();


            var viewModel = new IndexViewModel
            {
                ClientCount = totalClient,
                ManagedAssets = totalManagedAssets,
                TransactionCount = totalTransactions,
                TotalAssetsPerUser = totalAssetsPerUser,
            };

            return View("Index", viewModel);
        }

        [Route("/manage-user")]
        public async Task<IActionResult> ManageUser()
        {
            var user = await _userManager.GetUserAsync(User);

            if (!User.IsInRole(UserRoles.Admin))
            {
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

                return Challenge();
            }

            List<ManageUserViewModel> userData = await _context.Users
                .Where(u => u.Id != user.Id)
                .Select(u => new ManageUserViewModel
                {
                    UserId = u.Id,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    UserType = u.UserType

                })
                .ToListAsync();



            return View(userData);
        }

        public async Task<IActionResult> DeleteUser(string UserId)
        {
            if (UserId == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(UserId);
            if (user == null)
            {
                return BadRequest(UserId + " is not found in the list!");
            }
            if (user.UserType == "Client")
            {
                var Client = await _context.Clients.Where(c => c.UserId == UserId).FirstOrDefaultAsync();

                if (Client != null)
                {
                    // Delete related transactions
                    var transactions = await _context.Transactions.Where(t => t.ClientId == Client.ClientId).ToListAsync();
                    _context.Transactions.RemoveRange(transactions);

                    // Delete related goals
                    var goals = await _context.Goals.Where(g => g.ClientId == Client.ClientId).ToListAsync();
                    _context.Goals.RemoveRange(goals);

                    // Delete related categories
                    var categories = await _context.Categories.Where(c => c.ClientId == Client.ClientId).ToListAsync();
                    _context.Categories.RemoveRange(categories);
                    _context.Clients.Remove(Client);
                }
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageUser", "Admin");
        }

        [Route("/add-admin")]
        public IActionResult AddAdmin()
        {
            return View();
        }

        [Route("/add-admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAdmin(CreateAdminModel admin)
        {

            var user = RegisterModel.CreateUser();

            user.FirstName = admin.FirstName;
            user.LastName = admin.LastName;
            user.PhoneNumber = admin.PhoneNumber;
            user.Email = admin.Email;
            user.EmailConfirmed = true;
            user.UserType = "Admin";

            await _userStore.SetUserNameAsync(user, admin.Email, CancellationToken.None);
            await _userManager.CreateAsync(user, admin.Password);
            await _userManager.AddToRoleAsync(user, Constants.UserRoles.Admin);

            return RedirectToAction("ManageUser");
        }

        [Route("/add-advisor")]
        public IActionResult AddAdvisor()
        {

            return View();
        }

        [Route("/add-advisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAdvisor(CreateAdvisorModel advisor)
        {

            var user = RegisterModel.CreateUser();

            user.FirstName = advisor.FirstName;
            user.LastName = advisor.LastName;
            user.PhoneNumber = advisor.PhoneNumber;
            user.Email = advisor.Email;
            user.EmailConfirmed = true;
            user.UserType = "Advisor";

            await _userStore.SetUserNameAsync(user, advisor.Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, advisor.Password);
            await _userManager.AddToRoleAsync(user, Constants.UserRoles.Advisor);

            if (result.Succeeded)
            {
                var userId = await _userManager.GetUserIdAsync(user);

                var newAdvisor = new Advisor
                {
                    User = user,
                    UserId = userId,
                    Specialization = advisor.Specialization,
                    YearsOfExperience = advisor.YearOfExperience
                    //add specialization, year of experience
                };

                await _context.AddAsync(newAdvisor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageUser");
        }

    }
}
