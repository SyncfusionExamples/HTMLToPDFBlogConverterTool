using System.ComponentModel.DataAnnotations;

namespace HTMLToPDF_WebApplication.Models
{
    public class URL
    {
        [Required]
        public string AdURL { get; set; }
        [Required]
        public string ImageURL { get; set; }
        [Required]
        public string HeaderText { get; set; }
        [Required]
        public string BlogLink { get; set; }
    }
}
