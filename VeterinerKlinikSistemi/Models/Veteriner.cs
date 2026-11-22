using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinikSistemi.Models
{
    public class Veteriner
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hekim adı zorunludur.")]
        [StringLength(100)]
        public string AdSoyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
        [StringLength(150)]
        public string UzmanlikAlani { get; set; } = string.Empty;

        // Veterinerin sisteme giriş yapabilmesi için Identity bağlantısı
        public string? KullaniciId { get; set; }
        public Kullanici? Kullanici { get; set; }

        public int KlinikId { get; set; }
        public Klinik? Klinik { get; set; }

        public ICollection<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();
        public ICollection<VeterinerMusaitlik> Musaitlikler { get; set; } = new List<VeterinerMusaitlik>();
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}