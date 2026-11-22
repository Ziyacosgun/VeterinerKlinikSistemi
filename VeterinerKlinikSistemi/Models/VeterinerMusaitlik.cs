using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinikSistemi.Models
{
    public class VeterinerMusaitlik
    {
        public int Id { get; set; }

        public int VeterinerId { get; set; }
        public Veteriner? Veteriner { get; set; }

        [Required]
        public DayOfWeek Gun { get; set; }

        [Required]
        public TimeSpan BaslangicSaati { get; set; }

        [Required]
        public TimeSpan BitisSaati { get; set; }
    }
}