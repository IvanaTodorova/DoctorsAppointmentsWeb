using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using PraksaWebApp.Models;
namespace PraksaWebApp.Controllers
{
    public class Tipovi_na_specijalizacijaController : Controller
    {
    private readonly HttpClient _httpClient;
    
    public Tipovi_na_specijalizacijaController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IActionResult> Index()
    {
        List<Tipovi_na_specijalizacija>? speciijalizacija = await _httpClient.GetFromJsonAsync<List<Tipovi_na_specijalizacija>>(
            "https://localhost:7081/api/Tipovi_Na_Specijalizacija");

        return View(speciijalizacija);
    }

    }
}


