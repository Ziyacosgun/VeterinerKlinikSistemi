using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeterinerKlinikSistemi.Models
{
    public enum RandevuDurumu
    {
        Bekliyor = 0,
        Onaylandi = 1,
        Reddedildi = 2,
        Tamamlandi = 3
    }

    public class Randevu
    {
        public int Id { get; set; }

        public int EvcilHayvanId { get; set; }
        public EvcilHayvan? EvcilHayvan { get; set; }

        public int HizmetId { get; set; }
        public Hizmet? Hizmet { get; set; }

        public int VeterinerId { get; set; }
        public Veteriner? Veteriner { get; set; }

        [Required(ErrorMessage = "Randevu tarih ve saati zorunludur.")]
        public DateTime RandevuTarihi { get; set; }

        public int SureDakika { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Ucret { get; set; }

        public RandevuDurumu Durum { get; set; } = RandevuDurumu.Bekliyor;

        [StringLength(1000)]
        public string? MuayeneNotu { get; set; }
    }
}