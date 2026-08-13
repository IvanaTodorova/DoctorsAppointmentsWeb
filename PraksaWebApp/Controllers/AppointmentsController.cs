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

        public async Task<IActionResult> Index(
            int page = 1,
            DateTime? datum_od = null,
            DateTime? datum_do = null,
            TimeSpan? vreme_od = null,
            TimeSpan? vreme_do = null,
            string? patient = null,
            string? doctor = null,
            string? status = null,
            string? sort = null)
        {
            if (HttpContext.Session.GetString("username") == null)
            {
                return RedirectToAction("Index", "Korisnik");
            }

            var tip = HttpContext.Session.GetInt32("tip_na_korisnik");
            var patientId = HttpContext.Session.GetInt32("patient_id");
            var doctorId = HttpContext.Session.GetInt32("doctor_id");

            try
            {
                string url = _apiSettings.BaseUrl + "Appointments";

                if (datum_od.HasValue)
                {
                    url += $"?datum_od={datum_od.Value:yyyy-MM-dd}";
                }

                if (datum_do.HasValue)
                {
                    url += datum_od.HasValue
                        ? $"&datum_do={datum_do.Value:yyyy-MM-dd}"
                        : $"?datum_do={datum_do.Value:yyyy-MM-dd}";
                }

                List<Appointments> termini =
                    await _httpClient.GetFromJsonAsync<List<Appointments>>(url)
                    ?? new List<Appointments>();


                if (tip == 2 && patientId.HasValue)
                {
                    termini = termini
                        .Where(x => x.patientid == patientId.Value)
                        .ToList();
                }

                if (tip == 1 && doctorId.HasValue)
                {
                    termini = termini
                        .Where(x => x.doctor_id == doctorId.Value)
                        .ToList();
                }

                termini = termini.OrderBy(x => x.appointmentdate).ToList();

                if (sort == "date")
                    termini = termini.OrderBy(x => x.appointmentdate).ToList();

                else if (sort == "time")
                    termini = termini
                        .OrderBy(x => TimeSpan.Parse(x.appointmenttime))
                        .ToList();

                else if (sort == "patient")
                    termini = termini.OrderBy(x => x.patient_first_name).ToList();

                else if (sort == "doctor")
                    termini = termini.OrderBy(x => x.doctor_first_name).ToList();

                else if (sort == "status")
                    termini = termini.OrderBy(x => x.status).ToList();

                if (vreme_od.HasValue)
                {
                    termini = termini
                        .Where(x => TimeSpan.TryParse(x.appointmenttime, out TimeSpan vreme)
                                    && vreme >= vreme_od.Value)
                        .ToList();
                }

                if (vreme_do.HasValue)
                {
                    termini = termini
                        .Where(x => TimeSpan.TryParse(x.appointmenttime, out TimeSpan vreme)
                                    && vreme <= vreme_do.Value)
                        .ToList();
                }

                if (!string.IsNullOrWhiteSpace(patient))
                {
                    termini = termini
                        .Where(x => (x.patient_first_name ?? "")
                            .StartsWith(patient, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (!string.IsNullOrWhiteSpace(doctor))
                {
                    termini = termini
                        .Where(x => (x.doctor_first_name ?? "")
                            .StartsWith(doctor, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    termini = termini
                        .Where(x => (x.status ?? "")
                            .Equals(status, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (vreme_od.HasValue || vreme_do.HasValue)
                {
                    if (datum_od.HasValue || datum_do.HasValue)
                    {
                        termini = termini
                            .OrderBy(x => x.appointmentdate)
                            .ThenBy(x => TimeSpan.Parse(x.appointmenttime))
                            .ToList();
                    }
                    else
                    {
                        termini = termini
                            .OrderBy(x => TimeSpan.Parse(x.appointmenttime))
                            .ToList();
                    }
                }

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

                // Статистика според филтрираните термини
                ViewBag.TotalAppointments = termini.Count;

                ViewBag.ScheduledAppointments = termini.Count(x =>
                    (x.status ?? "").Equals("ЗАКАЖАН", StringComparison.OrdinalIgnoreCase));

                ViewBag.CompletedAppointments = termini.Count(x =>
                    (x.status ?? "").Equals("ЗАВРШЕН", StringComparison.OrdinalIgnoreCase));

                ViewBag.CancelledAppointments = termini.Count(x =>
                    (x.status ?? "").Equals("ОТКАЖАН", StringComparison.OrdinalIgnoreCase));


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

                // Ги чуваме датумите за да останат во filter-от
                ViewBag.DatumOd = datum_od?.ToString("yyyy-MM-dd");
                ViewBag.DatumDo = datum_do?.ToString("yyyy-MM-dd");
                ViewBag.Sort = sort;

                ViewBag.VremeOd = vreme_od?.ToString(@"hh\:mm");
                ViewBag.VremeDo = vreme_do?.ToString(@"hh\:mm");
                ViewBag.Patient = patient;
                ViewBag.Doctor = doctor;
                ViewBag.StatusFilter = status;

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