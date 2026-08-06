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
            List<Appointments> termini = await _httpClient.GetFromJsonAsync<List<Appointments>>(
                "https://localhost:7081/api/Appointments") ?? new List<Appointments>();

            List<Doctor>? doktori = await _httpClient.GetFromJsonAsync<List<Doctor>>(
                "https://localhost:7081/api/Doctors");

            List<Patient>? pacienti = await _httpClient.GetFromJsonAsync<List<Patient>>(
                "https://localhost:7081/api/Patients");

            List<Status>? statusi = await _httpClient.GetFromJsonAsync<List<Status>>(
                "https://localhost:7081/api/Status");


            AppointmentViewModel model = new AppointmentViewModel
            {
                Appointments = termini,
                Doctors = doktori,
                Patients = pacienti,
                Status = statusi
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Save(Appointments appointment)
        {
            var appointments = await _httpClient
                .GetFromJsonAsync<List<Appointments>>(
                    "https://localhost:7081/api/Appointments"
                ) ?? new List<Appointments>();

            bool exists = appointments.Any(a =>
                a.doctor_id == appointment.doctor_id &&
                a.appointmentdate.Date == appointment.appointmentdate.Date &&
                a.appointmenttime == appointment.appointmenttime &&
                a.id != appointment.id
            );

            if (exists)
            {
                TempData["Error"] = "Докторот веќе има закажано термин во тоа време!";
                return RedirectToAction("Index");
            }

            if (appointment.id == 0)
            {
                await _httpClient.PostAsJsonAsync(
                    "https://localhost:7081/api/Appointments",
                    appointment
                );
            }
            else
            {
                await _httpClient.PutAsJsonAsync(
                    $"https://localhost:7081/api/Appointments?id={appointment.id}",
                    appointment
                );
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _httpClient.DeleteAsync(
                $"https://localhost:7081/api/Appointments?id={id}"
            );

            return RedirectToAction("Index");
        }
    }
}