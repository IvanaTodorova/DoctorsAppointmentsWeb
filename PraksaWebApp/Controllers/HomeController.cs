using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PraksaWebApp.Models;
using System.Net.Http.Json;
using System.Diagnostics;

namespace PraksaWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;
        public HomeController(
        ILogger<HomeController> logger,
        HttpClient httpClient,
        IOptions<ApiSettings> apiSettings)
        {
            _logger = logger;
            _httpClient = httpClient;
            _apiSettings = apiSettings.Value;
        }

        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("username");
            var tip = HttpContext.Session.GetInt32("tip_na_korisnik");

            if (HttpContext.Session.GetString("username") == null)
            {
                return RedirectToAction("Index", "Korisnik");
            }
            ViewBag.Username = username;
            ViewBag.Tip = tip;

            var termini = new List<Appointments>();

            try
            {
                string url = _apiSettings.BaseUrl +
                             $"Appointments?datum_od={DateTime.Today:yyyy-MM-dd}" +
                             $"&datum_do={DateTime.Today:yyyy-MM-dd}";

                termini = await _httpClient.GetFromJsonAsync<List<Appointments>>(url)
                           ?? new List<Appointments>();

                if (tip == 1)
                {
                    var doctorId = HttpContext.Session.GetInt32("doctor_id");

                    if (doctorId.HasValue)
                    {
                        termini = termini
                            .Where(x => x.doctor_id == doctorId.Value)
                            .OrderBy(x => TimeSpan.Parse(x.appointmenttime))
                            .ToList();
                    }
                }
                else if (tip == 2)
                {
                    var patientId = HttpContext.Session.GetInt32("patient_id");

                    if (patientId.HasValue)
                    {
                        termini = termini
                            .Where(x => x.patientid == patientId.Value)
                            .OrderBy(x => TimeSpan.Parse(x.appointmenttime))
                            .ToList();
                    }
                }

                ViewBag.DnevniTermini = termini;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при вчитување на дневните термини.");
                ViewBag.DnevniTermini = new List<Appointments>();
            }

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
