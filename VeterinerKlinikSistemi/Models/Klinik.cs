using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinikSistemi.Models
{
    public class Klinik
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Klinik adı zorunludur.")]
        [StringLength(150)]
        public string Ad { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Adres { get; set; }

        [StringLength(20)]
        public string? Telefon { get; set; }

        [Required]
        public TimeSpan AcilisSaati { get; set; } = new TimeSpan(9, 0, 0);

        [Required]
        public TimeSpan KapanisSaati { get; set; } = new TimeSpan(18, 0, 0);

        public ICollection<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();
        public ICollection<Veteriner> Veterinerler { get; set; } = new List<Veteriner>();
    }
}