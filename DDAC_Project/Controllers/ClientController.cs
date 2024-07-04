using Microsoft.AspNetCore.Mvc;

namespace DDAC_Project.Controllers
{
    public class ClientController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }       
        public IActionResult Analysis()
        {
            return View();
        }      
        
        public IActionResult Goals()
        {
            return View();
        }
    }
}
