using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinikSistemi.Models
{
    public class EvcilHayvan
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Evcil hayvan adı zorunludur.")]
        [StringLength(80)]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tür zorunludur (örn: Kedi, Köpek).")]
        [StringLength(50)]
        public string Tur { get; set; } = string.Empty;

        [Required(ErrorMessage = "Irk zorunludur.")]
        [StringLength(80)]
        public string Irk { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
        [DataType(DataType.Date)]
        public DateTime DogumTarihi { get; set; }

        [Required(ErrorMessage = "Kilo bilgisi zorunludur.")]
        [Range(0.1, 200, ErrorMessage = "Geçerli bir kilo giriniz.")]
        public double Kilo { get; set; }

        public string? FotografUrl { get; set; }

        [Required]
        public string SahipId { get; set; } = string.Empty;
        public Kullanici? Sahip { get; set; }

        public ICollection<AsiKaydi> AsiKayitlari { get; set; } = new List<AsiKaydi>();
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}