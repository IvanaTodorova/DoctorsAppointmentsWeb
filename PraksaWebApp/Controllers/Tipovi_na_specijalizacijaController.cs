using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using PraksaWebApp.Models;

namespace PraksaWebApp.Controllers
{
    public class Tipovi_na_specijalizacijaController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;

        public Tipovi_na_specijalizacijaController(
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

            List<Tipovi_na_specijalizacija>? specijalizacija =
                await _httpClient.GetFromJsonAsync<List<Tipovi_na_specijalizacija>>(
                    _apiSettings.BaseUrl + "Tipovi_Na_Specijalizacija"
                );

            return View(specijalizacija);
        }


        [HttpPost]
        public async Task<IActionResult> Save(Tipovi_na_specijalizacija tns)
        {
            if (tns.id == 0)
            {
                await _httpClient.PostAsJsonAsync(
                    _apiSettings.BaseUrl + "Tipovi_Na_Specijalizacija",
                    tns
                );
            }
            else
            {
                await _httpClient.PutAsJsonAsync(
                    $"{_apiSettings.BaseUrl}Tipovi_Na_Specijalizacija?id={tns.id}",
                    tns
                );
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _httpClient.DeleteAsync(
                $"{_apiSettings.BaseUrl}Tipovi_Na_Specijalizacija?id={id}"
            );

            return RedirectToAction("Index");
        }
    }
}