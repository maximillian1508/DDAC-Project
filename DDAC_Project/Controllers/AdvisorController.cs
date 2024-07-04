﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDAC_Project.Controllers
{
    [Authorize(Roles ="Advisor")]
    public class AdvisorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult FinancialAnalysis()
        {
            return View();
        }

        public IActionResult SelectUser()
        {
            return View();
        }
    }
}