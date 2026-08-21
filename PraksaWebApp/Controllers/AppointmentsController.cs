using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PraksaWebApp.Models;
using System.Net.Http.Json;
using ClosedXML.Excel;
using System.IO;
namespace PraksaWebApp.Controllers
{
    [Authorize]
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

            var tipClaim = User.FindFirst("tip_korisnik")?.Value;
            var tip = tipClaim != null ? int.Parse(tipClaim) : -1;

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

                termini = termini
                    .OrderBy(x => x.appointmentdate)
                    .ToList();

                if (sort == "date")
                {
                    termini = termini
                        .OrderBy(x => x.appointmentdate)
                        .ToList();
                }
                else if (sort == "time")
                {
                    termini = termini
                        .OrderBy(x => TimeSpan.Parse(x.appointmenttime))
                        .ToList();
                }
                else if (sort == "patient")
                {
                    termini = termini
                        .OrderBy(x => x.patient_first_name)
                        .ToList();
                }
                else if (sort == "doctor")
                {
                    termini = termini
                        .OrderBy(x => x.doctor_first_name)
                        .ToList();
                }
                else if (sort == "status")
                {
                    termini = termini
                        .OrderBy(x => x.status)
                        .ToList();
                }

                if (vreme_od.HasValue)
                {
                    termini = termini
                        .Where(x =>
                            TimeSpan.TryParse(
                                x.appointmenttime,
                                out TimeSpan vreme
                            )
                            && vreme >= vreme_od.Value)
                        .ToList();
                }

                if (vreme_do.HasValue)
                {
                    termini = termini
                        .Where(x =>
                            TimeSpan.TryParse(
                                x.appointmenttime,
                                out TimeSpan vreme
                            )
                            && vreme <= vreme_do.Value)
                        .ToList();
                }

                if (!string.IsNullOrWhiteSpace(patient))
                {
                    termini = termini
                        .Where(x =>
                            (x.patient_first_name ?? "")
                            .StartsWith(
                                patient,
                                StringComparison.OrdinalIgnoreCase
                            ))
                        .ToList();
                }

                if (!string.IsNullOrWhiteSpace(doctor))
                {
                    termini = termini
                        .Where(x =>
                            (x.doctor_first_name ?? "")
                            .StartsWith(
                                doctor,
                                StringComparison.OrdinalIgnoreCase
                            ))
                        .ToList();
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    termini = termini
                        .Where(x =>
                            (x.status ?? "")
                            .Equals(
                                status,
                                StringComparison.OrdinalIgnoreCase
                            ))
                        .ToList();
                }

                if (vreme_od.HasValue || vreme_do.HasValue)
                {
                    if (datum_od.HasValue || datum_do.HasValue)
                    {
                        termini = termini
                            .OrderBy(x => x.appointmentdate)
                            .ThenBy(x =>
                                TimeSpan.Parse(x.appointmenttime))
                            .ToList();
                    }
                    else
                    {
                        termini = termini
                            .OrderBy(x =>
                                TimeSpan.Parse(x.appointmenttime))
                            .ToList();
                    }
                }

                List<Doctor> doktori =
                    await _httpClient.GetFromJsonAsync<List<Doctor>>(
                        _apiSettings.BaseUrl + "Doctors"
                    )
                    ?? new List<Doctor>();

                List<Patient> pacienti =
                    await _httpClient.GetFromJsonAsync<List<Patient>>(
                        _apiSettings.BaseUrl + "Patients"
                    )
                    ?? new List<Patient>();

                if (tip == 1 && doctorId.HasValue)
                {
                    doktori = doktori
                        .Where(x => x.Id == doctorId.Value)
                        .ToList();
                }

                if (tip == 2 && patientId.HasValue)
                {
                    pacienti = pacienti
                        .Where(x => x.id == patientId.Value)
                        .ToList();
                }

                List<Status> statusi =
                    await _httpClient.GetFromJsonAsync<List<Status>>(
                        _apiSettings.BaseUrl + "Status"
                    )
                    ?? new List<Status>();

                ViewBag.TotalAppointments = termini.Count;

                ViewBag.ScheduledAppointments = termini.Count(x =>
                    (x.status ?? "")
                    .Equals(
                        "ЗАКАЖАН",
                        StringComparison.OrdinalIgnoreCase
                    ));

                ViewBag.CompletedAppointments = termini.Count(x =>
                    (x.status ?? "")
                    .Equals(
                        "ЗАВРШЕН",
                        StringComparison.OrdinalIgnoreCase
                    ));

                ViewBag.CancelledAppointments = termini.Count(x =>
                    (x.status ?? "")
                    .Equals(
                        "ОТКАЖАН",
                        StringComparison.OrdinalIgnoreCase
                    ));

                ViewBag.InProgressAppointments = termini.Count(x =>
                    (x.status ?? "")
                    .Equals(
                        "ВО ТЕК",
                        StringComparison.OrdinalIgnoreCase
                    ));

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

                ViewBag.DatumOd =
                    datum_od?.ToString("yyyy-MM-dd");

                ViewBag.DatumDo =
                    datum_do?.ToString("yyyy-MM-dd");

                ViewBag.Sort = sort;

                ViewBag.VremeOd =
                    vreme_od?.ToString(@"hh\:mm");

                ViewBag.VremeDo =
                    vreme_do?.ToString(@"hh\:mm");

                ViewBag.Patient = patient;
                ViewBag.Doctor = doctor;
                ViewBag.StatusFilter = status;


                AppointmentViewModel model =
                    new AppointmentViewModel
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
            var username =
                HttpContext.Session.GetString("username");

            Appointments? oldAppointment = null;

            if (appointment.id != 0)
            {
                oldAppointment =
                    await _httpClient.GetFromJsonAsync<Appointments>(
                        $"{_apiSettings.BaseUrl}Appointments/GetById?id={appointment.id}"
                    );
            }

            bool dateChanged =
                oldAppointment != null &&
                oldAppointment.appointmentdate.Date !=
                appointment.appointmentdate.Date;


            TimeSpan newAppointmentTime =
                TimeSpan.Parse(appointment.appointmenttime);

            TimeSpan appointmentEnd =
                TimeSpan.Zero;

            if (appointment.id == 0 || dateChanged)
            {

                if (appointment.appointmentdate.Date <
                    DateTime.Today)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Не може да се избере датум кој е во минатото."
                    });
                }

                if (appointment.appointmentdate.Date ==
                    DateTime.Today)
                {
                    if (newAppointmentTime <
                        DateTime.Now.TimeOfDay)
                    {
                        return Json(new
                        {
                            success = false,
                            message =
                                "Времето што го избравте веќе е поминато."
                        });
                    }
                }
            }

            var doctorAppointments =
                await _httpClient
                    .GetFromJsonAsync<List<Appointments>>(
                        $"{_apiSettings.BaseUrl}Appointments/GetDoctorAppointmentsForDate" +
                        $"?doctor_id={appointment.doctor_id}" +
                        $"&datum={appointment.appointmentdate:yyyy-MM-dd}"
                    )
                ?? new List<Appointments>();

            bool exists = doctorAppointments.Any(a =>
            {
                TimeSpan oldAppointmentTime =
                    TimeSpan.Parse(a.appointmenttime);

                appointmentEnd =
                    oldAppointmentTime.Add(
                        TimeSpan.FromMinutes(30)
                    );

                return newAppointmentTime >=
                           oldAppointmentTime
                       &&
                       newAppointmentTime <
                           appointmentEnd
                       &&
                       a.id != appointment.id;
            });

            if (exists)
            {
                return Json(new
                {
                    success = false,

                    message =
                        "Докторот веќе има закажано термин во тоа време! " +
                        "Следно слободно време е " +
                        appointmentEnd.ToString(@"hh\:mm")
                });
            }

            if (appointment.id == 0)
            {
                appointment.created_by =
                    username;
                await _httpClient.PostAsJsonAsync(
                    _apiSettings.BaseUrl + "Appointments",
                    appointment);

                return Json(new
                {
                    success = true,
                    message = "Успешно креиран термин!"
                });

            }
            else
            {
                appointment.modified_by =
                    username;
                await _httpClient.PutAsJsonAsync(
                    $"{_apiSettings.BaseUrl}Appointments?id={appointment.id}",
                    appointment);

                return Json(new
                {
                    success = true,
                    message = "Успешно ажуриран термин!"
                });
            }
        }

        [HttpPut]
        public async Task<IActionResult> PromeniStatusVoTek(int id, string modified_by)
        {
            var response = await _httpClient.PutAsync(
                $"{_apiSettings.BaseUrl}Appointments/PromeniStatusVoTek" +
                $"?id={id}&modified_by={Uri.EscapeDataString(modified_by)}",
                null
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                return BadRequest(new
                {
                    success = false,
                    message = error
                });
            }

            return Ok(new
            {
                success = true
            });
        }

        [HttpPut]
        public async Task<IActionResult> PromeniStatusVoZavrsen(int id, string modified_by, string? notes)
        {
            var url =
                $"{_apiSettings.BaseUrl}Appointments/PromeniStatusVoZavrsen" +
                $"?id={id}" +
                $"&modified_by={Uri.EscapeDataString(modified_by)}" +
                $"&notes={Uri.EscapeDataString(notes ?? "")}";

            var response = await _httpClient.PutAsync(url, null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                return BadRequest(new
                {
                    success = false,
                    message = error
                });
            }

            return Ok(new
            {
                success = true
            });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _httpClient.DeleteAsync(
                $"{_apiSettings.BaseUrl}Appointments?id={id}"
            );

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientHistory(int patientId)
        {
            var termini = await _httpClient.GetFromJsonAsync<List<Appointments>>(
                $"{_apiSettings.BaseUrl}Appointments"
            ) ?? new List<Appointments>();

            termini = termini
                .Where(x => x.patientid == patientId)
                .OrderByDescending(x => x.appointmentdate)
                .ThenByDescending(x => TimeSpan.Parse(x.appointmenttime))
                .ToList();

            return Json(termini);
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel()
        {
            var termini = await _httpClient.GetFromJsonAsync<List<Appointments>>(
                _apiSettings.BaseUrl + "Appointments"
            ) ?? new List<Appointments>();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Термини");

                worksheet.Cell(1, 1).Value = "Ид";
                worksheet.Cell(1, 2).Value = "Доктор";
                worksheet.Cell(1, 3).Value = "Пациент";
                worksheet.Cell(1, 4).Value = "Датум";
                worksheet.Cell(1, 5).Value = "Време";
                worksheet.Cell(1, 6).Value = "Статус";
                worksheet.Cell(1, 7).Value = "Забелешка";

                int row = 2;

                foreach (var termin in termini)
                {
                    worksheet.Cell(row, 1).Value = termin.id;
                    worksheet.Cell(row, 2).Value =
                        $"{termin.doctor_first_name} {termin.doctor_last_name}";
                    worksheet.Cell(row, 3).Value =
                        $"{termin.patient_first_name} {termin.patient_last_name}";
                    worksheet.Cell(row, 4).Value =
                        termin.appointmentdate.ToString("dd.MM.yyyy");
                    worksheet.Cell(row, 5).Value =
                    TimeSpan.Parse(termin.appointmenttime).ToString(@"hh\:mm");
                    worksheet.Cell(row, 6).Value = termin.status;
                    worksheet.Cell(row, 7).Value = termin.notes;

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Termini.xlsx"
                    );
                }
            }
        }
    }
}