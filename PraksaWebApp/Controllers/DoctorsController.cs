using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using PraksaWebApp.Models;

namespace PraksaWebApp.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly HttpClient _httpClient;

        public DoctorsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            List<Doctor>? doktori = await _httpClient.GetFromJsonAsync<List<Doctor>>(
                "https://localhost:7081/api/Doctors"
            );

            List<Tipovi_na_specijalizacija>? specijalizacii = await _httpClient.GetFromJsonAsync<List<Tipovi_na_specijalizacija>>(
                "https://localhost:7081/api/Tipovi_Na_Specijalizacija"
            );


            DoctorViewModel model = new DoctorViewModel
            {
                Doctors = doktori,
                Specijalizacija = specijalizacii
            };


            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Save(Doctor doctor)
        {
            if (doctor.Id == null || doctor.Id == 0)
            {
                await _httpClient.PostAsJsonAsync(
                    "https://localhost:7081/api/Doctors",
                    doctor
                );
            }
            else
            {
                await _httpClient.PutAsJsonAsync(
                    $"https://localhost:7081/api/Doctors?id={doctor.Id}",
                    doctor
                );
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _httpClient.DeleteAsync(
                $"https://localhost:7081/api/Doctors?id={id}"
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

