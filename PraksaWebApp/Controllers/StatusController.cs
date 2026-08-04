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
            if (status.id == 0)
            { // POST - додавање нов статус
                await _httpClient.PostAsJsonAsync(
                    "https://localhost:7081/api/Status",
                    status
                    );
            } else
            { // PUT - изменување постоечки статус
                await _httpClient.PutAsJsonAsync(
                    $"https://localhost:7081/api/Status?id={status.id}",
                    status
                    );
            }
            return RedirectToAction("Index"); 
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        { 
            await _httpClient.DeleteAsync(
                $"https://localhost:7081/api/Status?id={id}"
                );
            return RedirectToAction("Index"); 
        }
    }
}
