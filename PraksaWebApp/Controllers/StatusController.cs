using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using PraksaWebApp.Models;

namespace PraksaWebApp.Controllers
{
    public class StatusController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;

        public StatusController(
            HttpClient httpClient,
            IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClient;
            _apiSettings = apiSettings.Value;
        }


        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("username") == null)
            {
                return RedirectToAction("Index", "Korisnik");
            }

            List<Status>? status = await _httpClient.GetFromJsonAsync<List<Status>>(
                _apiSettings.BaseUrl + "Status"
            );

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
                    _apiSettings.BaseUrl + "Status",
                    status
                );

                Console.WriteLine("POST STATUS: " + response.StatusCode);
            }
            else
            {
                var response = await _httpClient.PutAsJsonAsync(
                    $"{_apiSettings.BaseUrl}Status?id={status.id}",
                    status
                );

                Console.WriteLine("PUT STATUS: " + response.StatusCode);
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _httpClient.DeleteAsync(
                $"{_apiSettings.BaseUrl}Status?id={id}"
            );

            return RedirectToAction("Index");
        }
    }
}