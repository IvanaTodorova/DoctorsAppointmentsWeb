using Microsoft.AspNetCore.Mvc;
using PraksaWebApp.Models;
using System.Diagnostics;

namespace PraksaWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var username = HttpContext.Session.GetString("username");
            var tip = HttpContext.Session.GetInt32("tip_na_korisnik");

            if (HttpContext.Session.GetString("username") == null)
            {
                return RedirectToAction("Index", "Korisnik");
            }
            ViewBag.Username = username;
            ViewBag.Tip = tip;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
