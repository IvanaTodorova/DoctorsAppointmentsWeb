using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using PraksaWebApp.Models;

namespace PraksaWebApp.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;

        public AppointmentsController(
            HttpClient httpClient,
            IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClient;
            _apiSettings = apiSettings.Value;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                List<Appointments> termini =
                    await _httpClient.GetFromJsonAsync<List<Appointments>>(
                        _apiSettings.BaseUrl + "Appointments"
                    ) ?? new List<Appointments>();


                List<Doctor> doktori =
                    await _httpClient.GetFromJsonAsync<List<Doctor>>(
                        _apiSettings.BaseUrl + "Doctors"
                    ) ?? new List<Doctor>();


                List<Patient> pacienti =
                    await _httpClient.GetFromJsonAsync<List<Patient>>(
                        _apiSettings.BaseUrl + "Patients"
                    ) ?? new List<Patient>();


                List<Status> statusi =
                    await _httpClient.GetFromJsonAsync<List<Status>>(
                        _apiSettings.BaseUrl + "Status"
                    ) ?? new List<Status>();


                int pageSize = 10;


                int totalPages = (int)Math.Ceiling(
                    termini.Count / (double)pageSize
                );


                var terminiNaStrana = termini
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();


                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;


                AppointmentViewModel model = new AppointmentViewModel
                {
                    Appointments = terminiNaStrana,
                    Doctors = doktori,
                    Patients = pacienti,
                    Status = statusi
                };


                return View(model);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Save(Appointments appointment)
        {
            TimeSpan newAppointmentTime = TimeSpan.Parse(appointment.appointmenttime);
            TimeSpan appointmentEnd = TimeSpan.Zero;
            var doctorAppointments = await _httpClient
            .GetFromJsonAsync<List<Appointments>>(
            $"{_apiSettings.BaseUrl}Appointments/GetDoctorAppointmentsForDate?doctor_id={appointment.doctor_id}&datum={appointment.appointmentdate:yyyy-MM-dd}"
            ) ?? new List<Appointments>();

            bool exists = doctorAppointments.Any(a =>
            {
                TimeSpan oldAppointmentTime = TimeSpan.Parse(a.appointmenttime);

                 appointmentEnd = oldAppointmentTime.Add(
                    TimeSpan.FromMinutes(30)
                );

                return newAppointmentTime >= oldAppointmentTime &&
                       newAppointmentTime < appointmentEnd &&
                       a.id != appointment.id;
            });

            if (exists)
            {
                TempData["Error"] = "Докторот веќе има закажано термин во тоа време! Следно слободно време е " + appointmentEnd.ToString();
                return RedirectToAction("Index");
            }


            if (appointment.id == 0)
            {
                await _httpClient.PostAsJsonAsync(
                    _apiSettings.BaseUrl + "Appointments",
                    appointment
                );
            }
            else
            {
                await _httpClient.PutAsJsonAsync(
                    $"{_apiSettings.BaseUrl}Appointments?id={appointment.id}",
                    appointment
                );
            }


            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _httpClient.DeleteAsync(
                $"{_apiSettings.BaseUrl}Appointments?id={id}"
            );

            return RedirectToAction("Index");
        }
    }
}