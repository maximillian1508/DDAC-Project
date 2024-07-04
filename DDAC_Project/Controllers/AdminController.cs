using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDAC_Project.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }    
        
        public IActionResult ManageUser()
        {
            return View();
        }
    }
}
