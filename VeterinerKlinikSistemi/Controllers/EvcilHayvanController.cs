using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinikSistemi.Models;
using VeterinerKlinikSistemi.Veri;

namespace VeterinerKlinikSistemi.Controllers
{
    [Authorize]
    public class EvcilHayvanController : Controller
    {
        private readonly VeritabaniBaglami _baglam;
        private readonly UserManager<Kullanici> _kullaniciYoneticisi;

        public EvcilHayvanController(VeritabaniBaglami baglam, UserManager<Kullanici> kullaniciYoneticisi)
        {
            _baglam = baglam;
            _kullaniciYoneticisi = kullaniciYoneticisi;
        }

        public async Task<IActionResult> Index()
        {
            var kullaniciId = _kullaniciYoneticisi.GetUserId(User);
            var hayvanlar = await _baglam.EvcilHayvanlar
                .Include(e => e.AsiKayitlari)
                .Where(e => e.SahipId == kullaniciId)
                .ToListAsync();

            return View(hayvanlar);
        }

        [HttpGet]
        public IActionResult Ekle()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(EvcilHayvan evcilHayvan)
        {
            var kullaniciId = _kullaniciYoneticisi.GetUserId(User);
            evcilHayvan.SahipId = kullaniciId!;

            // SahipId formdan değil oturumdan atandığı için doğrulamadan çıkarıyoruz
            ModelState.Remove(nameof(EvcilHayvan.SahipId));

            if (!ModelState.IsValid) return View(evcilHayvan);

            _baglam.EvcilHayvanlar.Add(evcilHayvan);
            await _baglam.SaveChangesAsync();
            TempData["Basari"] = "Evcil hayvanınız başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Duzenle(int id)
        {
            var kullaniciId = _kullaniciYoneticisi.GetUserId(User);
            var hayvan = await _baglam.EvcilHayvanlar
                .FirstOrDefaultAsync(e => e.Id == id && e.SahipId == kullaniciId);

            if (hayvan == null) return NotFound();

            return View(hayvan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(EvcilHayvan evcilHayvan)
        {
            var kullaniciId = _kullaniciYoneticisi.GetUserId(User);
            ModelState.Remove(nameof(EvcilHayvan.SahipId));

            if (!ModelState.IsValid) return View(evcilHayvan);

            var mevcut = await _baglam.EvcilHayvanlar
                .FirstOrDefaultAsync(e => e.Id == evcilHayvan.Id && e.SahipId == kullaniciId);

            if (mevcut == null) return NotFound();

            mevcut.Ad = evcilHayvan.Ad;
            mevcut.Tur = evcilHayvan.Tur;
            mevcut.Irk = evcilHayvan.Irk;
            mevcut.DogumTarihi = evcilHayvan.DogumTarihi;
            mevcut.Kilo = evcilHayvan.Kilo;
            mevcut.FotografUrl = evcilHayvan.FotografUrl;

            await _baglam.SaveChangesAsync();
            TempData["Basari"] = "Evcil hayvan bilgileri güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var kullaniciId = _kullaniciYoneticisi.GetUserId(User);
            var hayvan = await _baglam.EvcilHayvanlar
                .FirstOrDefaultAsync(e => e.Id == id && e.SahipId == kullaniciId);

            if (hayvan == null) return NotFound();

            _baglam.EvcilHayvanlar.Remove(hayvan);
            await _baglam.SaveChangesAsync();
            TempData["Basari"] = "Evcil hayvan kaydı silindi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AsiGecmisi(int id)
        {
            var kullaniciId = _kullaniciYoneticisi.GetUserId(User);
            var hayvan = await _baglam.EvcilHayvanlar
                .Include(e => e.AsiKayitlari.OrderByDescending(a => a.YapildigiTarih))
                .FirstOrDefaultAsync(e => e.Id == id && e.SahipId == kullaniciId);

            if (hayvan == null) return NotFound();

            return View(hayvan);
        }
    }
}