using Microsoft.AspNetCore.Mvc;
using PraksaWebApp.Models;
using System.Net.Http.Json;

namespace PraksaWebApp.Controllers
{
    public class PatientsController : Controller
    {
        private readonly HttpClient _httpClient;

        public PatientsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        // Ги прикажува сите пациенти
        public async Task<IActionResult> Index()
        {
            List<Patient>? pacienti =
                await _httpClient.GetFromJsonAsync<List<Patient>>(
                    "https://localhost:7081/api/Patients"
                );

            return View(pacienti);
        }


        // Додава нов пациент или изменува постоечки
        [HttpPost]
        public async Task<IActionResult> Save(Patient patient)
        {
            if (patient.id == 0)
            {
                // Додавање нов пациент
                await _httpClient.PostAsJsonAsync(
                    "https://localhost:7081/api/Patients",
                    patient
                );
            }
            else
            {
                // Изменување на постоечки пациент
                await _httpClient.PutAsJsonAsync(
                    $"https://localhost:7081/api/Patients?id={patient.id}",
                    patient
                );
            }

            return RedirectToAction("Index");
        }

        // Брише пациент
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _httpClient.DeleteAsync(
                $"https://localhost:7081/api/Patients?id={id}"
            );

            return RedirectToAction("Index");
        }
    }
}
