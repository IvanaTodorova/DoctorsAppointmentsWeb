namespace PraksaWebApp.Models
{
    public class KorisnikViewModel
    {
        public Korisnik Korisnik { get; set; } = new Korisnik();

        public List<Doctor> Doctors { get; set; } = new List<Doctor>();

        public List<Patient> Patients { get; set; } = new List<Patient>();
    }
}