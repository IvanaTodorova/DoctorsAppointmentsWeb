namespace PraksaWebApp.Models
{
    public class Appointments
    {
        public int? id { get; set; }

        public int? doctor_id { get; set; }
        public string? doctor_first_name { get; set; }
        public string? doctor_last_name { get; set; }

        public string? specijalnost { get; set; }
        public int? patientid { get; set; }
        public string? patient_first_name { get; set; }
        public string? patient_last_name { get; set; }

        public DateTime appointmentdate { get; set; }
        public string? appointmenttime { get; set; }
        public int? status_id { get; set; }
        public string? status { get; set; }

        public string? notes { get; set; }
    }
}
