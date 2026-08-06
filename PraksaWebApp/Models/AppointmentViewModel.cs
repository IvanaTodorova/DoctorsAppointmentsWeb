namespace PraksaWebApp.Models
{
    public class AppointmentViewModel
    {
        public List<Appointments> Appointments { get; set; }

        public List<Doctor> Doctors { get; set; }

        public List<Patient> Patients { get; set; }

        public List<Status> Status { get; set; }
    }
}