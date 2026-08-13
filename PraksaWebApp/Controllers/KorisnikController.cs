using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using PraksaWebApp.Models;

namespace PraksaWebApp.Controllers
{
    public class KorisnikController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;

        public KorisnikController(
            HttpClient httpClient,
            IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClient;
            _apiSettings = apiSettings.Value;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            var tip = HttpContext.Session.GetInt32("tip_na_korisnik");

            if (tip != 0)
            {
                return RedirectToAction("Index", "Home");
            }

            string url = _apiSettings.BaseUrl + "Korisnik";

            var korisnici = await _httpClient
                .GetFromJsonAsync<List<Korisnik>>(url)
                ?? new List<Korisnik>();

            return View(korisnici);
        }
     
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Index(string username, string password)
        {
            string url = _apiSettings.BaseUrl +
                         $"Korisnik/GetUser?username={username}&password={password}";

            var korisnik = await _httpClient
                .GetFromJsonAsync<Korisnik>(url);

            if (korisnik == null)
            {
                ViewBag.Error = "Погрешно username или password.";
                return View();
            }

            HttpContext.Session.SetString("username", korisnik.username);
            HttpContext.Session.SetInt32("tip_na_korisnik", korisnik.tip_na_korisnik);
            HttpContext.Session.SetInt32("korisnik_id", korisnik.id);

            if (korisnik.doctor_id.HasValue)
            {
                HttpContext.Session.SetInt32("doctor_id", korisnik.doctor_id.Value);
            }

            if (korisnik.patient_id.HasValue)
            {
                HttpContext.Session.SetInt32("patient_id", korisnik.patient_id.Value);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}