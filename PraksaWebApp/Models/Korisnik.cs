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
        public string? Doctor_name { get; set; }
        public string? Doctor_surname { get; set; }
        public string? Patient_name { get; set; }
        public string? Patient_surname { get; set; }
    }
}