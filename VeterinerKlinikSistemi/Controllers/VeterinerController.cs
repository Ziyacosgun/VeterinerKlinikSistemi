using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinikSistemi.Models;
using VeterinerKlinikSistemi.Veri;

namespace VeterinerKlinikSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VeterinerController : Controller
    {
        private readonly VeritabaniBaglami _baglam;
        private readonly UserManager<Kullanici> _kullaniciYoneticisi;

        public VeterinerController(VeritabaniBaglami baglam, UserManager<Kullanici> kullaniciYoneticisi)
        {
            _baglam = baglam;
            _kullaniciYoneticisi = kullaniciYoneticisi;
        }

        public async Task<IActionResult> Index()
        {
            var veterinerler = await _baglam.Veterinerler
                .Include(v => v.Klinik)
                .Include(v => v.Hizmetler)
                .Include(v => v.Kullanici)
                .ToListAsync();
            return View(veterinerler);
        }

        [HttpGet]
        public IActionResult Ekle()
        {
            ViewBag.Klinikler = new SelectList(_baglam.Klinikler.ToList(), "Id", "Ad");
            ViewBag.Hizmetler = _baglam.Hizmetler.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(Veteriner veteriner, int[] secilenHizmetler, string? email, string? sifre)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Klinikler = new SelectList(_baglam.Klinikler.ToList(), "Id", "Ad", veteriner.KlinikId);
                ViewBag.Hizmetler = _baglam.Hizmetler.ToList();
                return View(veteriner);
            }

            // E-posta ve şifre girildiyse Veteriner rolüyle kullanıcı hesabı oluştur
            if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(sifre))
            {
                var yeniKullanici = new Kullanici
                {
                    UserName = email,
                    Email = email,
                    AdSoyad = veteriner.AdSoyad,
                    EmailConfirmed = true
                };

                var sonuc = await _kullaniciYoneticisi.CreateAsync(yeniKullanici, sifre);
                if (!sonuc.Succeeded)
                {
                    foreach (var hata in sonuc.Errors)
                    {
                        ModelState.AddModelError(string.Empty, hata.Description);
                    }
                    ViewBag.Klinikler = new SelectList(_baglam.Klinikler.ToList(), "Id", "Ad", veteriner.KlinikId);
                    ViewBag.Hizmetler = _baglam.Hizmetler.ToList();
                    return View(veteriner);
                }

                await _kullaniciYoneticisi.AddToRoleAsync(yeniKullanici, "Veteriner");
                veteriner.KullaniciId = yeniKullanici.Id;
            }

            // Seçilen hizmetleri ekle
            if (secilenHizmetler != null && secilenHizmetler.Length > 0)
            {
                var hizmetler = await _baglam.Hizmetler
                    .Where(h => secilenHizmetler.Contains(h.Id))
                    .ToListAsync();
                veteriner.Hizmetler = hizmetler;
            }

            _baglam.Veterinerler.Add(veteriner);
            await _baglam.SaveChangesAsync();
            TempData["Basari"] = "Veteriner başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Duzenle(int id)
        {
            var veteriner = await _baglam.Veterinerler
                .Include(v => v.Hizmetler)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (veteriner == null) return NotFound();

            ViewBag.Klinikler = new SelectList(_baglam.Klinikler.ToList(), "Id", "Ad", veteriner.KlinikId);
            ViewBag.Hizmetler = _baglam.Hizmetler.ToList();
            ViewBag.SecilenHizmetIds = veteriner.Hizmetler.Select(h => h.Id).ToList();
            return View(veteriner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(Veteriner veteriner, int[] secilenHizmetler)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Klinikler = new SelectList(_baglam.Klinikler.ToList(), "Id", "Ad", veteriner.KlinikId);
                ViewBag.Hizmetler = _baglam.Hizmetler.ToList();
                ViewBag.SecilenHizmetIds = secilenHizmetler?.ToList() ?? new List<int>();
                return View(veteriner);
            }

            var mevcutVeteriner = await _baglam.Veterinerler
                .Include(v => v.Hizmetler)
                .FirstOrDefaultAsync(v => v.Id == veteriner.Id);

            if (mevcutVeteriner == null) return NotFound();

            mevcutVeteriner.AdSoyad = veteriner.AdSoyad;
            mevcutVeteriner.UzmanlikAlani = veteriner.UzmanlikAlani;
            mevcutVeteriner.KlinikId = veteriner.KlinikId;

            mevcutVeteriner.Hizmetler.Clear();
            if (secilenHizmetler != null && secilenHizmetler.Length > 0)
            {
                var hizmetler = await _baglam.Hizmetler
                    .Where(h => secilenHizmetler.Contains(h.Id))
                    .ToListAsync();
                foreach (var hizmet in hizmetler)
                {
                    mevcutVeteriner.Hizmetler.Add(hizmet);
                }
            }

            await _baglam.SaveChangesAsync();
            TempData["Basari"] = "Veteriner bilgileri güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var veteriner = await _baglam.Veterinerler.FindAsync(id);
            if (veteriner == null) return NotFound();

            _baglam.Veterinerler.Remove(veteriner);
            await _baglam.SaveChangesAsync();
            TempData["Basari"] = "Veteriner silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}