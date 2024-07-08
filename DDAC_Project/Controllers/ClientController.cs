using DDAC_Project.Data;
using Microsoft.AspNetCore.Mvc;
using DDAC_Project.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DDAC_Project.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;

namespace DDAC_Project.Controllers
{
    public class ClientController : Controller
    {
        private readonly DDAC_ProjectContext _context;

        // Keep only this constructor
        public ClientController(DDAC_ProjectContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Analysis()
        {
            return View();
        }

        public async Task<IActionResult> Goals()
        {
            var clientId = Convert.ToInt32(HttpContext.Session.GetInt32("ClientId"));

            if (clientId == 0)
            {
                return Challenge();
            }
            List<Goal> goals = await _context.Goals.Where(goal => goal.ClientId == clientId).ToListAsync();
            
            ViewBag.NewGoal = new Goal
            {
                Name = "",
                TargetAmount = 0,
                ClientId = clientId,
                Client = await _context.Clients.FindAsync(clientId)
            };

            return View(goals);
        }

        [HttpPost]
        public async Task<IActionResult> AddGoal(Goal newGoal)
        {
            var clientId = Convert.ToInt32(HttpContext.Session.GetInt32("ClientId"));
            newGoal.ClientId = clientId;
            Console.WriteLine(clientId);
                _context.Goals.Add(newGoal);
                await _context.SaveChangesAsync();

            return RedirectToAction("Goals");
        }

        public async Task<IActionResult> EditGoal(int ? GoalId)
        {
            if (GoalId == null)
            {
                return NotFound();
            }
            var goal = await _context.Goals.FindAsync(GoalId);

            if (goal == null)
            {
                return BadRequest(GoalId + " is not found in the table!");
            }
            return View(goal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGoal(Goal goal)
        {
            try
            {
                    var clientId = Convert.ToInt32(HttpContext.Session.GetInt32("ClientId"));
                    goal.ClientId = clientId;
                    _context.Goals.Update(goal);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Goals", "Client");
                //return View("EditGoal", goal);
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
        }

        public async Task<IActionResult> DeleteGoal(int? GoalId)
        {
            if (GoalId == null)
            {
                return NotFound();
            }
            var goal = await _context.Goals.FindAsync(GoalId);
            if (goal == null)
            {
                return BadRequest(GoalId + " is not found in the list!");
            }
            _context.Goals.Remove(goal);
            await _context.SaveChangesAsync();
            return RedirectToAction("Goals", "Client");
        }
    }
}