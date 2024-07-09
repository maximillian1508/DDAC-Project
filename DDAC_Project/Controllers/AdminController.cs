using DDAC_Project.Areas.Identity.Data;
using DDAC_Project.Data;
using DDAC_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static DDAC_Project.Controllers.AdvisorController;

namespace DDAC_Project.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
        private readonly DDAC_ProjectContext _context;
        private readonly UserManager<DDAC_ProjectUser> _userManager;
        private readonly SignInManager<DDAC_ProjectUser> _signInManager;
        public AdminController(DDAC_ProjectContext context, UserManager<DDAC_ProjectUser> userManager, SignInManager<DDAC_ProjectUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
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
            if (!_signInManager.IsSignedIn(User))
            {
                return Challenge();
            }

            List<ManageUserViewModel> userData = await _context.Users
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
    }
}
