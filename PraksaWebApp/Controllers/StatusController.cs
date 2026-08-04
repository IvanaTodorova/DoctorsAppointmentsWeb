using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using PraksaWebApp.Models;

namespace PraksaWebApp.Controllers
{
    public class StatusController : Controller
    {
        private readonly HttpClient _httpClient;

        public StatusController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            List<Status>? status = await _httpClient.GetFromJsonAsync<List<Status>>(
                "https://localhost:7081/api/Status");

            return View(status);
        }
        [HttpPost]
        public async Task<IActionResult> Save(Status status)
        {
            Console.WriteLine("ID: " + status.id);
            Console.WriteLine("NAME: " + status.status_name);

            if (status.id == 0)
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "https://localhost:7081/api/Status",
                    status
                );

                Console.WriteLine("POST STATUS: " + response.StatusCode);
            }
            else
            {
                var response = await _httpClient.PutAsJsonAsync(
                    $"https://localhost:7081/api/Status?id={status.id}",
                    status
                );

                Console.WriteLine("PUT STATUS: " + response.StatusCode);
            }

            return RedirectToAction("Index");
        }
    }
}
