using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using PraksaWebApp.Models;

namespace PraksaWebApp.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;

        public DoctorsController(
            HttpClient httpClient,
            IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClient;
            _apiSettings = apiSettings.Value;
        }


        public async Task<IActionResult> Index(int page = 1)
        {
            List<Doctor> doktori =
                await _httpClient.GetFromJsonAsync<List<Doctor>>(
                    _apiSettings.BaseUrl + "Doctors"
                ) ?? new List<Doctor>();


            List<Tipovi_na_specijalizacija> specijalizacii =
                await _httpClient.GetFromJsonAsync<List<Tipovi_na_specijalizacija>>(
                    _apiSettings.BaseUrl + "Tipovi_Na_Specijalizacija"
                ) ?? new List<Tipovi_na_specijalizacija>();


            int pageSize = 10;


            int totalPages = (int)Math.Ceiling(
                doktori.Count / (double)pageSize
            );


            var doktoriNaStrana = doktori
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();


            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;


            DoctorViewModel model = new DoctorViewModel
            {
                Doctors = doktoriNaStrana,
                Specijalizacija = specijalizacii
            };


            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Save(Doctor doctor)
        {
            if (doctor.Specijalnost_id == null || doctor.Specijalnost_id == 0)
            {
                return RedirectToAction("Index");
            }


            if (doctor.Id == null || doctor.Id == 0)
            {
                await _httpClient.PostAsJsonAsync(
                    _apiSettings.BaseUrl + "Doctors",
                    doctor
                );
            }
            else
            {
                await _httpClient.PutAsJsonAsync(
                    $"{_apiSettings.BaseUrl}Doctors?id={doctor.Id}",
                    doctor
                );
            }


            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _httpClient.DeleteAsync(
                $"{_apiSettings.BaseUrl}Doctors?id={id}"
            );

            return RedirectToAction("Index");
        }
    }
}


//using Microsoft.AspNetCore.Mvc;
//using System.Net.Http.Json;
//using PraksaWebApp.Models;

//namespace PraksaWebApp.Controllers
//{
//    public class DoctorsController : Controller
//    {
//        public IActionResult Index()
//        {
//            List<Doctor> doktori = new List<Doctor>()
//            {
//                new Doctor { Id = 1, First_name = "Ивана", Last_name = "Тодорова", IsActive = true, Phone = "075123123", Specijalnost_id = 1, Specijalnost = "Педијатар" },
//                new Doctor { Id = 2, First_name = "Тимче", Last_name = "Поп-ицовски", IsActive = true, Phone = "075111222", Specijalnost_id = 3, Specijalnost = "Општ лекар" },
//                new Doctor { Id = 3, First_name = "Стефанија", Last_name = "Тодорова", IsActive = true, Phone = "0751223333", Specijalnost_id = 5, Specijalnost = "Хирург" },
//                new Doctor { Id = 4, First_name = "Ристе", Last_name = "Лалков", IsActive = true, Phone = "071231231", Specijalnost_id = 6, Specijalnost = "Ортопед" }
//            };
//            return View(doktori);
//        }
//    }
//}


