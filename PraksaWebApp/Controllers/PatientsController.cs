using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PraksaWebApp.Models;
using System.Net.Http.Json;

using Microsoft.Extensions.Options;

namespace PraksaWebApp.Controllers
{
    [Authorize]
    public class PatientsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;

        public PatientsController(
        HttpClient httpClient,
        IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClient;
            _apiSettings = apiSettings.Value;
        }


        // Ги прикажува сите пациенти
        public async Task<IActionResult> Index(int page = 1)
        {
            var tipClaim = User.FindFirst("tip_korisnik")?.Value;

            if (tipClaim != "0")
            {
                return RedirectToAction("Index", "Home");
            }

            if (HttpContext.Session.GetString("username") == null)
            {
                return RedirectToAction("Index", "Korisnik");
            }

            List<Patient>? pacienti =
                await _httpClient.GetFromJsonAsync<List<Patient>>(
                    _apiSettings.BaseUrl + "Patients"
                );

            int pageSize = 10;

            int totalPages = (int)Math.Ceiling(
                pacienti.Count / (double)pageSize
            );

            var pacientiNaStrana = pacienti
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(pacientiNaStrana);
        }


        // Додава нов пациент или изменува постоечки
        [HttpPost]
        public async Task<IActionResult> Save(Patient patient)
        {
            var tipClaim = User.FindFirst("tip_korisnik")?.Value;

            if (tipClaim != "0")
            {
                return RedirectToAction("Index", "Home");
            }

            HttpResponseMessage response;

            if (patient.id == 0)
            {
                // Нов пациент
                response = await _httpClient.PostAsJsonAsync(
                    _apiSettings.BaseUrl + "Patients",
                    patient
                );
            }
            else
            {
                // Измена на постоечки пациент
                response = await _httpClient.PutAsJsonAsync(
                    $"{_apiSettings.BaseUrl}Patients?id={patient.id}",
                    patient
                );
            }

            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return Content(result);
            }

            return RedirectToAction("Index");
        }

        // Брише пациент
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var tipClaim = User.FindFirst("tip_korisnik")?.Value;

            if (tipClaim != "0")
            {
                return RedirectToAction("Index", "Home");
            }

            await _httpClient.DeleteAsync(
                $"{_apiSettings.BaseUrl}Patients?id={id}"
            );

            return RedirectToAction("Index");
        }
    }
}
