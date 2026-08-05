namespace PraksaWebApp.Models
{
    public class Doctor
    {
        public int? Id { get; set; }
        public string? First_name { get; set; }
        public string? Last_name { get; set; }
        public string? Specijalnost { get; set; }
        public string? Phone { get; set; }
        public bool? IsActive { get; set; }
        public string? IsActiveString
        {
            get
            {
                return IsActive == true ? "Да" : "Не";
            }
        }
        public int? Specijalnost_id { get; set; }
    }
}

