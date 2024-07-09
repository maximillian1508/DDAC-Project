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
        
        public IActionResult ManageUser()
        {
            return View();
        }
    }
}
