using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinikSistemi.Models;
using VeterinerKlinikSistemi.Veri;

namespace VeterinerKlinikSistemi.Controllers
{
    [Authorize(Roles = "Veteriner")]
    public class VeterinerPaneliController : Controller
    {
        private readonly VeritabaniBaglami _baglam;
        private readonly UserManager<Kullanici> _kullaniciYoneticisi;

        public VeterinerPaneliController(VeritabaniBaglami baglam, UserManager<Kullanici> kullaniciYoneticisi)
        {
            _baglam = baglam;
            _kullaniciYoneticisi = kullaniciYoneticisi;
        }

        private async Task<Veteriner?> GirisYapanVeterineriGetirAsync()
        {
            var kullaniciId = _kullaniciYoneticisi.GetUserId(User);
            return await _baglam.Veterinerler
                .Include(v => v.Klinik)
                .FirstOrDefaultAsync(v => v.KullaniciId == kullaniciId);
        }

        public async Task<IActionResult> Index()
        {
            var veteriner = await GirisYapanVeterineriGetirAsync();
            if (veteriner == null)
            {
                ViewBag.ProfilEksik = true;
                return View(new List<Randevu>());
            }

            ViewBag.VeterinerBilgi = $"{veteriner.AdSoyad} ({veteriner.Klinik?.Ad})";

            var randevular = await _baglam.Randevular
                .Include(r => r.EvcilHayvan)
                    .ThenInclude(e => e!.Sahip)
                .Include(r => r.Hizmet)
                .Where(r => r.VeterinerId == veteriner.Id)
                .OrderBy(r => r.Durum)
                .ThenBy(r => r.RandevuTarihi)
                .ToListAsync();

            return View(randevular);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onayla(int id)
        {
            var veteriner = await GirisYapanVeterineriGetirAsync();
            if (veteriner == null) return Forbid();

            var randevu = await _baglam.Randevular
                .FirstOrDefaultAsync(r => r.Id == id && r.VeterinerId == veteriner.Id);

            if (randevu == null) return NotFound();

            randevu.Durum = RandevuDurumu.Onaylandi;
            await _baglam.SaveChangesAsync();

            TempData["Basari"] = "Randevu onaylandı.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reddet(int id)
        {
            var veteriner = await GirisYapanVeterineriGetirAsync();
            if (veteriner == null) return Forbid();

            var randevu = await _baglam.Randevular
                .FirstOrDefaultAsync(r => r.Id == id && r.VeterinerId == veteriner.Id);

            if (randevu == null) return NotFound();

            randevu.Durum = RandevuDurumu.Reddedildi;
            await _baglam.SaveChangesAsync();

            TempData["Basari"] = "Randevu reddedildi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Muayene(int id)
        {
            var veteriner = await GirisYapanVeterineriGetirAsync();
            if (veteriner == null) return Forbid();

            var randevu = await _baglam.Randevular
                .Include(r => r.Hizmet)
                .Include(r => r.EvcilHayvan)
                    .ThenInclude(e => e!.Sahip)
                .Include(r => r.EvcilHayvan)
                    .ThenInclude(e => e!.AsiKayitlari.OrderByDescending(a => a.YapildigiTarih))
                .FirstOrDefaultAsync(r => r.Id == id && r.VeterinerId == veteriner.Id);

            if (randevu == null) return NotFound();

            return View(randevu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MuayeneTamamla(int id, string? muayeneNotu)
        {
            var veteriner = await GirisYapanVeterineriGetirAsync();
            if (veteriner == null) return Forbid();

            var randevu = await _baglam.Randevular
                .FirstOrDefaultAsync(r => r.Id == id && r.VeterinerId == veteriner.Id);

            if (randevu == null) return NotFound();

            randevu.MuayeneNotu = muayeneNotu;
            randevu.Durum = RandevuDurumu.Tamamlandi;
            await _baglam.SaveChangesAsync();

            TempData["Basari"] = "Muayene notu kaydedildi ve randevu tamamlandı.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsiEkle(int randevuId, string asiAdi, DateTime yapildigiTarih, DateTime? sonrakiDozTarihi)
        {
            var veteriner = await GirisYapanVeterineriGetirAsync();
            if (veteriner == null) return Forbid();

            var randevu = await _baglam.Randevular
                .Include(r => r.EvcilHayvan)
                .FirstOrDefaultAsync(r => r.Id == randevuId && r.VeterinerId == veteriner.Id);

            if (randevu == null || randevu.EvcilHayvan == null) return NotFound();

            if (string.IsNullOrWhiteSpace(asiAdi))
            {
                TempData["Hata"] = "Aşı adı boş bırakılamaz.";
                return RedirectToAction(nameof(Muayene), new { id = randevuId });
            }

            var yeniAsi = new AsiKaydi
            {
                AsiAdi = asiAdi.Trim(),
                YapildigiTarih = yapildigiTarih,
                SonrakiDozTarihi = sonrakiDozTarihi,
                EvcilHayvanId = randevu.EvcilHayvanId
            };

            randevu.EvcilHayvan.AsiKayitlari.Add(yeniAsi);
            await _baglam.SaveChangesAsync();

            TempData["Basari"] = $"{asiAdi} aşısı hastanın kaydına eklendi.";
            return RedirectToAction(nameof(Muayene), new { id = randevuId });
        }
    }
}