using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeterinerKlinikSistemi.Models
{
    public class Hizmet
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [StringLength(100)]
        public string Ad { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Aciklama { get; set; }

        [Required(ErrorMessage = "Süre (dakika) zorunludur.")]
        [Range(5, 480, ErrorMessage = "Süre 5 ile 480 dakika arasında olmalıdır.")]
        public int SureDakika { get; set; }

        [Required(ErrorMessage = "Ücret zorunludur.")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000)]
        public decimal Ucret { get; set; }

        public int KlinikId { get; set; }
        public Klinik? Klinik { get; set; }

        public ICollection<Veteriner> Veterinerler { get; set; } = new List<Veteriner>();
    }
}