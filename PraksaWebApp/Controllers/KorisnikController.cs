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

            string korisnikUrl = _apiSettings.BaseUrl + "Korisnik";
            string doctorUrl = _apiSettings.BaseUrl + "Doctors";
            string patientUrl = _apiSettings.BaseUrl + "Patients";

            var korisnici = await _httpClient
                .GetFromJsonAsync<List<Korisnik>>(korisnikUrl)
                ?? new List<Korisnik>();

            var doctors = await _httpClient
                .GetFromJsonAsync<List<Doctor>>(doctorUrl)
                ?? new List<Doctor>();

            var patients = await _httpClient
                .GetFromJsonAsync<List<Patient>>(patientUrl)
                ?? new List<Patient>();

            var model = new KorisnikViewModel
            {
                Korisnik = new Korisnik(),
                Doctors = doctors,
                Patients = patients
            };

            ViewBag.Korisnici = korisnici;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(
            string username,
            string pass,
            int tip_na_korisnik,
            int? doctor_id,
            int? patient_id)
        {
            if (tip_na_korisnik == 0)
            {
                doctor_id = null;
                patient_id = null;
            }

            if (tip_na_korisnik == 1)
            {
                if (doctor_id == null)
                {
                    TempData["Error"] =
                        "За корисник од тип Доктор мора да изберете доктор.";

                    return RedirectToAction("Lista");
                }

                if (patient_id != null)
                {
                    TempData["Error"] =
                        "За корисник од тип Доктор не може да изберете пациент.";

                    return RedirectToAction("Lista");
                }
            }

            if (tip_na_korisnik == 2)
            {
                if (patient_id == null)
                {
                    TempData["Error"] =
                        "За корисник од тип Пациент мора да изберете пациент.";

                    return RedirectToAction("Lista");
                }

                if (doctor_id != null)
                {
                    TempData["Error"] =
                        "За корисник од тип Пациент не може да изберете доктор.";

                    return RedirectToAction("Lista");
                }
            }

            Korisnik korisnik = new Korisnik
            {
                username = username,
                pass = pass,
                tip_na_korisnik = tip_na_korisnik,
                doctor_id = doctor_id,
                patient_id = patient_id
            };

            var response = await _httpClient.PostAsJsonAsync(
                _apiSettings.BaseUrl + "Korisnik",
                korisnik
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                TempData["Error"] =
                    "Грешка при зачувување: " + error;

                return RedirectToAction("Lista");
            }

            return RedirectToAction("Lista");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _httpClient.DeleteAsync(
                $"{_apiSettings.BaseUrl}Korisnik?id={id}"
            );

            return RedirectToAction("Lista");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Index(
            string username,
            string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                ViewBag.Error = "Внесете username.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Внесете лозинка.";
                return View();
            }

            string url = _apiSettings.BaseUrl +
                         $"Korisnik/GetUser?username={username}&password={password}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error =
                    "Корисникот не постои или username/password се погрешни.";

                return View();
            }

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                ViewBag.Error =
                    "Корисникот не постои или username/password се погрешни.";

                return View();
            }

            var korisnik =
                System.Text.Json.JsonSerializer.Deserialize<Korisnik>(content);

            if (korisnik == null)
            {
                ViewBag.Error =
                    "Корисникот не постои или username/password се погрешни.";

                return View();
            }

            HttpContext.Session.SetString(
                "username",
                korisnik.username);

            HttpContext.Session.SetInt32(
                "tip_na_korisnik",
                korisnik.tip_na_korisnik);

            HttpContext.Session.SetInt32(
                "korisnik_id",
                korisnik.id);

            if (korisnik.doctor_id.HasValue)
            {
                HttpContext.Session.SetInt32(
                    "doctor_id",
                    korisnik.doctor_id.Value);
            }

            if (korisnik.patient_id.HasValue)
            {
                HttpContext.Session.SetInt32(
                    "patient_id",
                    korisnik.patient_id.Value);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Update(
            int id,
            string username_edit,
            string pass_edit,
            int tip_na_korisnik_edit,
            int? doctor_id_edit,
            int? patient_id_edit)
        {
            if (tip_na_korisnik_edit == 0)
            {
                doctor_id_edit = null;
                patient_id_edit = null;
            }

            if (tip_na_korisnik_edit == 1)
            {
                if (doctor_id_edit == null)
                {
                    TempData["Error"] =
                        "За корисник од тип Доктор мора да изберете доктор.";

                    return RedirectToAction("Lista");
                }

                if (patient_id_edit != null)
                {
                    TempData["Error"] =
                        "За корисник од тип Доктор не може да изберете пациент.";

                    return RedirectToAction("Lista");
                }
            }

            if (tip_na_korisnik_edit == 2)
            {
                if (patient_id_edit == null)
                {
                    TempData["Error"] =
                        "За корисник од тип Пациент мора да изберете пациент.";

                    return RedirectToAction("Lista");
                }

                if (doctor_id_edit != null)
                {
                    TempData["Error"] =
                        "За корисник од тип Пациент не може да изберете доктор.";

                    return RedirectToAction("Lista");
                }
            }

            Korisnik korisnik = new Korisnik
            {
                id = id,
                username = username_edit,
                pass = pass_edit,
                tip_na_korisnik = tip_na_korisnik_edit,
                doctor_id = doctor_id_edit,
                patient_id = patient_id_edit
            };

            var response = await _httpClient.PutAsJsonAsync(
                $"{_apiSettings.BaseUrl}Korisnik?id={id}",
                korisnik
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                TempData["Error"] =
                    "Грешка при изменување: " + error;

                return RedirectToAction("Lista");
            }

            return RedirectToAction("Lista");
        }
    }
}