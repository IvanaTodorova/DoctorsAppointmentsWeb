using System.ComponentModel.DataAnnotations;

namespace PraksaWebApp.Models
{
    public class Patient
    {
        public int? id { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public string? embg { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
    }
}
