using DDAC_Project.Areas.Identity.Data;
using DDAC_Project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DDAC_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<DDAC_ProjectUser> _userManager;


        public HomeController(ILogger<HomeController> logger, UserManager<DDAC_ProjectUser> userManager)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Advisor"))
                    {
                        return RedirectToAction("Index", "Advisor");
                    }                  
                    else if (await _userManager.IsInRoleAsync(user, "Client"))
                    {
                        return RedirectToAction("Index", "Client");
                    }
                }
                return View();
            }
            return View();
        }

        [Route("about-us")]
        public async Task<IActionResult> AboutUs()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Advisor"))
                    {
                        return RedirectToAction("Index", "Advisor");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Client"))
                    {
                        return RedirectToAction("Index", "Client");
                    }
                }
            }
            return View();
        }        

        public IActionResult RedirectToContact()
        {
            return new RedirectResult(Url.Action("AboutUs") + "#contact");
        }

        [Route("/features")]
        public async Task<IActionResult> Features()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Advisor"))
                    {
                        return RedirectToAction("Index", "Advisor");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Client"))
                    {
                        return RedirectToAction("Index", "Client");
                    }
                }
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
