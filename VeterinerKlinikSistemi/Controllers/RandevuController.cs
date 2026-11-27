using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinikSistemi.Models;
using VeterinerKlinikSistemi.Veri;

namespace VeterinerKlinikSistemi.Controllers
{
    [Authorize]
    public class RandevuController : Controller
    {
        private readonly VeritabaniBaglami _baglam;
        private readonly UserManager<Kullanici> _kullaniciYoneticisi;

        public RandevuController(VeritabaniBaglami baglam, UserManager<Kullanici> kullaniciYoneticisi)
        {
            _baglam = baglam;
            _kullaniciYoneticisi = kullaniciYoneticisi;
        }

        public async Task<IActionResult> Index()
        {
            var kullaniciId = _kullaniciYoneticisi.GetUserId(User);
            var randevular = await _baglam.Randevular
                .Include(r => r.EvcilHayvan)
                .Include(r => r.Hizmet)
                .Include(r => r.Veteriner)
                    .ThenInclude(v => v!.Klinik)
                .Where(r => r.EvcilHayvan!.SahipId == kullaniciId)
                .OrderByDescending(r => r.RandevuTarihi)
                .ToListAsync();

            return View(randevular);
        }

        [HttpGet]
        public async Task<IActionResult> Al()
        {
            var kullaniciId = _kullaniciYoneticisi.GetUserId(User);
            var hayvanlar = await _baglam.EvcilHayvanlar
                .Where(e => e.SahipId == kullaniciId)
                .ToListAsync();

            if (!hayvanlar.Any())
            {
                TempData["Hata"] = "Randevu alabilmek için önce en az bir evcil hayvan eklemelisiniz.";
                return RedirectToAction("Ekle", "EvcilHayvan");
            }

            await DropdownlariDoldur(kullaniciId!);
            return View();
        }

        // Seçilen hizmete göre o hizmeti veren veterinerleri JSON döner
        [HttpGet]
        public async Task<IActionResult> VeterinerleriGetir(int hizmetId)
        {
            var veterinerler = await _baglam.Veterinerler
                .Include(v => v.Klinik)
                .Where(v => v.Hizmetler.Any(h => h.Id == hizmetId))
                .Select(v => new
                {
                    id = v.Id,
                    ad = $"{v.AdSoyad} ({v.UzmanlikAlani} - {v.Klinik!.Ad})"
                })
                .ToListAsync();

            return Json(veterinerler);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Al(Randevu randevu)
        {
            var kullaniciId = _kullaniciYoneticisi.GetUserId(User);

            // 1. Geçmiş tarih kontrolü
            if (randevu.RandevuTarihi <= DateTime.Now)
            {
                ModelState.AddModelError(string.Empty, "Geçmiş bir tarih veya saat için randevu alamazsınız.");
            }

            // 2. Evcil hayvan sahiplik kontrolü
            var hayvan = await _baglam.EvcilHayvanlar
                .FirstOrDefaultAsync(e => e.Id == randevu.EvcilHayvanId && e.SahipId == kullaniciId);
            if (hayvan == null)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz evcil hayvan seçimi.");
            }

            // 3. Hizmet bilgilerini al (Süre ve Ücret)
            var hizmet = await _baglam.Hizmetler.FindAsync(randevu.HizmetId);
            if (hizmet == null)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz hizmet seçimi.");
            }
            else
            {
                randevu.SureDakika = hizmet.SureDakika;
                randevu.Ucret = hizmet.Ucret;
            }

            // 4. Veteriner müsaitlik ve randevu çakışma kontrolü
            if (ModelState.IsValid && hizmet != null)
            {
                var baslangic = randevu.RandevuTarihi;
                var bitis = baslangic.AddMinutes(hizmet.SureDakika);
                var gun = baslangic.DayOfWeek;
                var baslangicSaati = baslangic.TimeOfDay;
                var bitisSaati = bitis.TimeOfDay;

                // Veteriner o gün ve saat aralığında çalışıyor mu?
                bool musaitMi = await _baglam.VeterinerMusaitlikleri.AnyAsync(m =>
                    m.VeterinerId == randevu.VeterinerId &&
                    m.Gun == gun &&
                    m.BaslangicSaati <= baslangicSaati &&
                    m.BitisSaati >= bitisSaati);

                if (!musaitMi)
                {
                    ModelState.AddModelError(string.Empty, "Seçilen veteriner bu gün ve saat aralığında çalışmamaktadır.");
                }
                else
                {
                    // Aynı gün o veterinerin reddedilmemiş başka randevusuyla çakışma var mı?
                    var gununRandevulari = await _baglam.Randevular
                        .Where(r => r.VeterinerId == randevu.VeterinerId &&
                                    r.RandevuTarihi.Date == baslangic.Date &&
                                    r.Durum != RandevuDurumu.Reddedildi)
                        .ToListAsync();

                    bool cakismaVar = gununRandevulari.Any(r =>
                        r.RandevuTarihi < bitis &&
                        baslangic < r.RandevuTarihi.AddMinutes(r.SureDakika));

                    if (cakismaVar)
                    {
                        ModelState.AddModelError(string.Empty, "Seçilen saatte veterinerin başka bir randevusu bulunmaktadır. Lütfen farklı bir saat seçiniz.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                await DropdownlariDoldur(kullaniciId!, randevu.EvcilHayvanId, randevu.HizmetId, randevu.VeterinerId);
                return View(randevu);
            }

            randevu.Durum = RandevuDurumu.Bekliyor;
            _baglam.Randevular.Add(randevu);
            await _baglam.SaveChangesAsync();

            TempData["Basari"] = "Randevunuz başarıyla oluşturuldu. Veteriner onayı bekleniyor.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IptalEt(int id)
        {
            var kullaniciId = _kullaniciYoneticisi.GetUserId(User);
            var randevu = await _baglam.Randevular
                .Include(r => r.EvcilHayvan)
                .FirstOrDefaultAsync(r => r.Id == id && r.EvcilHayvan!.SahipId == kullaniciId);

            if (randevu == null) return NotFound();

            if (randevu.Durum != RandevuDurumu.Bekliyor)
            {
                TempData["Hata"] = "Sadece 'Bekliyor' durumundaki randevular iptal edilebilir.";
                return RedirectToAction(nameof(Index));
            }

            _baglam.Randevular.Remove(randevu);
            await _baglam.SaveChangesAsync();
            TempData["Basari"] = "Randevunuz iptal edildi.";
            return RedirectToAction(nameof(Index));
        }

        private async Task DropdownlariDoldur(string kullaniciId, int? secilenHayvan = null, int? secilenHizmet = null, int? secilenVet = null)
        {
            var hayvanlar = await _baglam.EvcilHayvanlar
                .Where(e => e.SahipId == kullaniciId)
                .ToListAsync();

            var hizmetler = await _baglam.Hizmetler
                .Include(h => h.Klinik)
                .Select(h => new
                {
                    h.Id,
                    Ad = $"{h.Ad} - {h.SureDakika} dk / {h.Ucret:N2} ₺ ({h.Klinik!.Ad})"
                })
                .ToListAsync();

            ViewBag.EvcilHayvanlar = new SelectList(hayvanlar, "Id", "Ad", secilenHayvan);
            ViewBag.Hizmetler = new SelectList(hizmetler, "Id", "Ad", secilenHizmet);

            if (secilenHizmet.HasValue)
            {
                var veterinerler = await _baglam.Veterinerler
                    .Where(v => v.Hizmetler.Any(h => h.Id == secilenHizmet.Value))
                    .ToListAsync();
                ViewBag.Veterinerler = new SelectList(veterinerler, "Id", "AdSoyad", secilenVet);
            }
            else
            {
                ViewBag.Veterinerler = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }
    }
}