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
    }
}