using Microsoft.AspNetCore.Mvc;

namespace DDAC_Project.Controllers
{
    public class AdvisorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
