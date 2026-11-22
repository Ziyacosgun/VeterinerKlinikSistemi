using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinikSistemi.Models
{
    public class AsiKaydi
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Aşı adı zorunludur.")]
        [StringLength(100)]
        public string AsiAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yapıldığı tarih zorunludur.")]
        [DataType(DataType.Date)]
        public DateTime YapildigiTarih { get; set; }

        [DataType(DataType.Date)]
        public DateTime? SonrakiDozTarihi { get; set; }

        public int EvcilHayvanId { get; set; }
        public EvcilHayvan? EvcilHayvan { get; set; }
    }
}