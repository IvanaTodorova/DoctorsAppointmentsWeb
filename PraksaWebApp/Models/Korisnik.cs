namespace PraksaWebApp.Models
{
    public class Korisnik
    {
        public int id { get; set; }
        public string username { get; set; }
        public string pass { get; set; }
        public int tip_na_korisnik { get; set; }
        public int? doctor_id { get; set; }
        public int? patient_id { get; set; }
    }
}