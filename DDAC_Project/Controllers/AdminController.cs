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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DDAC_Project.Validators;
using Microsoft.AspNetCore.Mvc.Rendering;

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

            public string AdvisorEmail { get; set; }
        }

        public class CreateAdminModel
        {
            [Required(ErrorMessage = "First name is required")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Phone number is required")]
            [RegularExpression(@"^[0-9]+$", ErrorMessage = "Phone number must contain only digits")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            [UniqueEmail(ErrorMessage = "This email is already in use")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required(ErrorMessage = "Confirm password is required")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public class EditAdminModel
        {
            [Required(ErrorMessage = "First name is required")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Phone number is required")]
            [RegularExpression(@"^[0-9]+$", ErrorMessage = "Phone number must contain only digits")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            public string Email { get; set; }

            [DataType(DataType.Password)]
            public string? Password { get; set; }

            public string? UserId { get; set; }
        }

        public class CreateAdvisorModel
        {
            [Required(ErrorMessage = "First name is required")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Phone number is required")]
            [RegularExpression(@"^[0-9]+$", ErrorMessage = "Phone number must contain only digits")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            [UniqueEmail(ErrorMessage = "This email is already in use")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required(ErrorMessage = "Confirm password is required")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string? Specialization { get; set; }

            [RegularExpression(@"^[0-9]+$", ErrorMessage = "Year of experience must contain only digits")]
            public string? YearOfExperience { get; set; }
        }

        public class EditAdvisorModel
        {
            [Required(ErrorMessage = "First name is required")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Phone number is required")]
            [RegularExpression(@"^[0-9]+$", ErrorMessage = "Phone number must contain only digits")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            public string Email { get; set; }

            [DataType(DataType.Password)]
            public string? Password { get; set; }

            public string? Specialization { get; set; }

            [RegularExpression(@"^[0-9]+$", ErrorMessage = "Year of experience must contain only digits")]
            public string? YearOfExperience { get; set; }

            public string? UserId { get; set; }

            public int? AdvisorId { get; set; }
        }

        public class EditClientModel
        {
            [Required(ErrorMessage = "First name is required")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Phone number is required")]
            [RegularExpression(@"^[0-9]+$", ErrorMessage = "Phone number must contain only digits")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            public string Email { get; set; }

            [DataType(DataType.Password)]
            public string? Password { get; set; }

            public string? UserId { get; set; }

            public int? AdvisorId { get; set; }
            public int? ClientId { get; set; }
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
                {
                    ClientUserId = t.Client.UserId,
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
                    UserType = u.UserType,
                    AdvisorEmail = u.UserType == "Client"
                                    ? _context.Clients
                                        .Where(c => c.UserId == u.Id)
                                        .Select(c => c.Advisor.User.Email)
                                        .FirstOrDefault()
                                    : null
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
            if (ModelState.IsValid)
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

            return View(admin);
        }

        [Route("/edit-admin")]
        [HttpGet]
        public async Task<IActionResult> EditAdmin(string? UserId)
        {
            if (string.IsNullOrEmpty(UserId))
            {
                return NotFound();
            }
            var admin = await _userManager.FindByIdAsync(UserId);

            if (admin == null)
            {
                return NotFound();
            }

            var model = new EditAdminModel
            {
                UserId = admin.Id,
                Email = admin.Email,
                PhoneNumber = admin.PhoneNumber,
                FirstName = admin.FirstName,
                LastName = admin.LastName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAdmin(EditAdminModel admin)
        {
            if (!ModelState.IsValid)
            {
                return View("EditAdmin", admin);
            }

            var user = await _userManager.FindByIdAsync(admin.UserId);
            if (user == null)
            {
                return NotFound();
            }

            user.PhoneNumber = admin.PhoneNumber;
            user.FirstName = admin.FirstName;
            user.LastName = admin.LastName;

            // Check if email has changed
            if (user.Email != admin.Email)
            {
                // Check if the new email is already in use
                var existingUser = await _userManager.FindByEmailAsync(admin.Email);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    ModelState.AddModelError("Email", "This email is already in use");
                    return View("EditAdmin", admin);
                }

                // Update email
                var setEmailResult = await _userManager.SetEmailAsync(user, admin.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("EditAdmin", admin);
                }

                // Update username
                await _userManager.SetUserNameAsync(user, admin.Email);

                var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _userManager.ConfirmEmailAsync(user, confirmToken);
            }

            if (!String.IsNullOrEmpty(admin.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordChangeResult = await _userManager.ResetPasswordAsync(user, token, admin.Password);

                if (!passwordChangeResult.Succeeded)
                {
                    foreach (var error in passwordChangeResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("EditAdmin", admin);
                }
            }
            // Update other properties as needed

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("ManageUser", "Admin");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("EditAdmin", admin);
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
            if (ModelState.IsValid)
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
            return View(advisor);
        }

        [Route("/edit-advisor")]
        public async Task<IActionResult> EditAdvisor(string? UserId)
        {
            if (string.IsNullOrEmpty(UserId))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(UserId);

            if (user == null)
            {
                return NotFound();
            }

            var advisorData = await _context.Advisors.Where(a => a.UserId == UserId).FirstOrDefaultAsync();

            var advisor = new EditAdvisorModel
            {
                AdvisorId = advisorData.AdvisorId,
                UserId = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                YearOfExperience = advisorData.YearsOfExperience,
                Specialization = advisorData.Specialization
            };

            return View(advisor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAdvisor(EditAdvisorModel advisor)
        {
            if (!ModelState.IsValid)
            {
                return View("EditAdvisor", advisor);
            }

            var user = await _userManager.FindByIdAsync(advisor.UserId);
            if (user == null)
            {
                return NotFound();
            }

            user.PhoneNumber = advisor.PhoneNumber;
            user.FirstName = advisor.FirstName;
            user.LastName = advisor.LastName;

            // Check if email has changed
            if (user.Email != advisor.Email)
            {
                // Check if the new email is already in use
                var existingUser = await _userManager.FindByEmailAsync(advisor.Email);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    ModelState.AddModelError("Email", "This email is already in use");
                    return View("EditAdvisor", advisor);
                }

                // Update email
                var setEmailResult = await _userManager.SetEmailAsync(user, advisor.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("EditAdvisor", advisor);
                }

                // Update username
                await _userManager.SetUserNameAsync(user, advisor.Email);

                var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _userManager.ConfirmEmailAsync(user, confirmToken);
            }

            if (!String.IsNullOrEmpty(advisor.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordChangeResult = await _userManager.ResetPasswordAsync(user, token, advisor.Password);

                if (!passwordChangeResult.Succeeded)
                {
                    foreach (var error in passwordChangeResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("EditAdvisor", advisor);
                }
            }

            var adv = await _context.Advisors.FindAsync(advisor.AdvisorId);
            adv.Specialization = advisor.Specialization;
            adv.YearsOfExperience = advisor.YearOfExperience;
            _context.Advisors.Update(adv);
            await _context.SaveChangesAsync();


            // Update other properties as needed
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("ManageUser", "Admin");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("EditAdvisor", advisor);
        }

        [Route("/edit-client")]
        public async Task<IActionResult> EditClient(string? UserId)
        {
            if (string.IsNullOrEmpty(UserId))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(UserId);

            if (user == null)
            {
                return NotFound();
            }

            var clientData = await _context.Clients.Where(a => a.UserId == UserId).FirstOrDefaultAsync();
            var currentAdvisorId = clientData?.AdvisorId;

            var advisorList = await _context.Advisors
                .GroupJoin(_context.Clients,
                    advisor => advisor.AdvisorId,
                    client => client.AdvisorId,
                    (advisor, clients) => new { Advisor = advisor, ClientCount = clients.Count() })
                .Where(x => x.ClientCount < 3 || x.Advisor.AdvisorId == currentAdvisorId)
                .Select(x => new SelectListItem
                {
                    Value = x.Advisor.AdvisorId.ToString(),
                    Text = $"{x.Advisor.User.FirstName} {x.Advisor.User.LastName}"
                })
                .ToListAsync();

            advisorList.Insert(0, new SelectListItem { Value = "", Text = "" });

            ViewBag.AdvisorList = advisorList;

            var client = new EditClientModel
            {
                ClientId = clientData.ClientId,
                AdvisorId = clientData.AdvisorId,
                UserId = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
            };

            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateClient(EditClientModel client)
        {
            var user = await _userManager.FindByIdAsync(client.UserId);
            if (user == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var cli = await _context.Clients.FindAsync(client.ClientId);
                cli.AdvisorId = client.AdvisorId;
                _context.Clients.Update(cli);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManageUser", "Admin");
            }

            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
            return View("EditClient", client);
        }

    }
}
