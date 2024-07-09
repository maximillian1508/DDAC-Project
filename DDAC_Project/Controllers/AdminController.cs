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

namespace DDAC_Project.Controllers
{
    [Authorize(Roles ="Admin")]
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

        public async Task<IActionResult> Index()
        {
            if (!_signInManager.IsSignedIn(User)) 
            {
                return Challenge();
            }
            var totalClient = await _context.Clients.CountAsync();

            var totalManagedAssets = await _context.Transactions.SumAsync(t => t.Amount);

            DateTime currentDate = DateTime.Today;
            DateTime startDateOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            DateTime endDateOfMonth = startDateOfMonth.AddMonths(1).AddDays(-1);
            var totalTransactions = await _context.Transactions
                .Where(t => t.Date >= startDateOfMonth && t.Date <= endDateOfMonth)
                .CountAsync();


            //var totalComment = await _context.Comments.Where(comment => comment.AdvisorId == advisorId).CountAsync();

            var viewModel = new IndexViewModel
            {
                ClientCount = totalClient,
                ManagedAssets = totalManagedAssets,
                TransactionCount = totalTransactions
            };

            return View("Index", viewModel);
        }    
        
        public async Task<IActionResult> ManageUser()
        {
            var user = await _userManager.GetUserAsync(User);

            if (!_signInManager.IsSignedIn(User))
            {
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

        public async Task<IActionResult> DeleteData(string UserId)
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

        public IActionResult AddAdmin()
        {

            return View();
        }

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

        public IActionResult AddAdvisor()
        {

            return View();
        }

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
