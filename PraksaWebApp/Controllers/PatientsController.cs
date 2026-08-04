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
        public async Task<IActionResult> Index()
        {
            List<Patient>? pacienti = await _httpClient.GetFromJsonAsync<List<Patient>>(
                "https://localhost:7081/api/Patients"
            );
            return View(pacienti);
        }
    }
}


