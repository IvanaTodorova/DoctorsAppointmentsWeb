using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using PraksaWebApp.Models;

namespace PraksaWebApp.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly HttpClient _httpClient;

        public AppointmentsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            List<Appointments>? termini = await _httpClient.GetFromJsonAsync<List<Appointments>>(
                "https://localhost:7081/api/Appointments");

            return View(termini);
        }
    }
}