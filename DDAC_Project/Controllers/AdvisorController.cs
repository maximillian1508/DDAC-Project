using DDAC_Project.Areas.Identity.Data;
using DDAC_Project.Areas.Identity.Pages.Account.Manage;
using DDAC_Project.Data;
using DDAC_Project.Views.Advisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DDAC_Project.Controllers
{
    [Authorize(Roles = "Advisor")]
    public class AdvisorController : Controller
    {
        private readonly DDAC_ProjectContext _context;
        private readonly UserManager<DDAC_ProjectUser> _userManager;
        public AdvisorController(DDAC_ProjectContext context, UserManager<DDAC_ProjectUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public class IndexViewModel
        {
            public int? ClientCount { get; set; }
            public int? CommentCount { get; set; }
            public decimal ManagedAssets { get; set; }
            public List<UserModel> AssignedClients { get; set; }
            public List<ClientAssetInfo> ClientAssets { get; set; }

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
            var totalManagedAssets = await _context.Transactions
                .Where(t => clientIds.Contains(t.ClientId))
                .SumAsync(t => t.Amount);

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
                    TotalAssets = _context.Transactions
                        .Where(t => t.ClientId == c.ClientId)
                        .Sum(t => t.Amount)
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

        public async Task<IActionResult> FinancialAnalysis(int clientId)
        {
            ViewBag.ClientId = clientId;
            return View();
        }

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


    }
}
