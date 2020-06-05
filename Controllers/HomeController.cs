﻿using ASPNETCore30Dashboard.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ASPNETCore30Dashboard.Controllers {
    public class HomeController : Controller {
        public IActionResult Login() {
            return View();
        }

        public IActionResult Dashboard() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
