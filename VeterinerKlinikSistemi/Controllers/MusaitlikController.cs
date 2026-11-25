using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinikSistemi.Models;
using VeterinerKlinikSistemi.Veri;

namespace VeterinerKlinikSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MusaitlikController : Controller
    {
        private readonly VeritabaniBaglami _baglam;

        public MusaitlikController(VeritabaniBaglami baglam)
        {
            _baglam = baglam;
        }

        private List<SelectListItem> GunListesiGetir() => new()
        {
            new SelectListItem("Pazartesi", ((int)DayOfWeek.Monday).ToString()),
            new SelectListItem("Salı", ((int)DayOfWeek.Tuesday).ToString()),
            new SelectListItem("Çarşamba", ((int)DayOfWeek.Wednesday).ToString()),
            new SelectListItem("Perşembe", ((int)DayOfWeek.Thursday).ToString()),
            new SelectListItem("Cuma", ((int)DayOfWeek.Friday).ToString()),
            new SelectListItem("Cumartesi", ((int)DayOfWeek.Saturday).ToString()),
            new SelectListItem("Pazar", ((int)DayOfWeek.Sunday).ToString())
        };

        public async Task<IActionResult> Index()
        {
            var musaitlikler = await _baglam.VeterinerMusaitlikleri
                .Include(m => m.Veteriner)
                .ThenInclude(v => v!.Klinik)
                .OrderBy(m => m.Veteriner!.AdSoyad)
                .ThenBy(m => m.Gun)
                .ThenBy(m => m.BaslangicSaati)
                .ToListAsync();

            return View(musaitlikler);
        }

        [HttpGet]
        public IActionResult Ekle()
        {
            ViewBag.Veterinerler = new SelectList(_baglam.Veterinerler.ToList(), "Id", "AdSoyad");
            ViewBag.Gunler = GunListesiGetir();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(VeterinerMusaitlik musaitlik)
        {
            if (musaitlik.BitisSaati <= musaitlik.BaslangicSaati)
            {
                ModelState.AddModelError(string.Empty, "Bitiş saati, başlangıç saatinden sonra olmalıdır.");
            }

            bool cakismaVar = await _baglam.VeterinerMusaitlikleri.AnyAsync(m =>
                m.VeterinerId == musaitlik.VeterinerId &&
                m.Gun == musaitlik.Gun &&
                m.BaslangicSaati < musaitlik.BitisSaati &&
                musaitlik.BaslangicSaati < m.BitisSaati);

            if (cakismaVar)
            {
                ModelState.AddModelError(string.Empty, "Bu veteriner için seçilen gün ve saat aralığında zaten bir müsaitlik kaydı var.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Veterinerler = new SelectList(_baglam.Veterinerler.ToList(), "Id", "AdSoyad", musaitlik.VeterinerId);
                ViewBag.Gunler = GunListesiGetir();
                return View(musaitlik);
            }

            _baglam.VeterinerMusaitlikleri.Add(musaitlik);
            await _baglam.SaveChangesAsync();
            TempData["Basari"] = "Müsaitlik saati başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Duzenle(int id)
        {
            var musaitlik = await _baglam.VeterinerMusaitlikleri.FindAsync(id);
            if (musaitlik == null) return NotFound();

            ViewBag.Veterinerler = new SelectList(_baglam.Veterinerler.ToList(), "Id", "AdSoyad", musaitlik.VeterinerId);
            ViewBag.Gunler = GunListesiGetir();
            return View(musaitlik);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(VeterinerMusaitlik musaitlik)
        {
            if (musaitlik.BitisSaati <= musaitlik.BaslangicSaati)
            {
                ModelState.AddModelError(string.Empty, "Bitiş saati, başlangıç saatinden sonra olmalıdır.");
            }

            bool cakismaVar = await _baglam.VeterinerMusaitlikleri.AnyAsync(m =>
                m.Id != musaitlik.Id &&
                m.VeterinerId == musaitlik.VeterinerId &&
                m.Gun == musaitlik.Gun &&
                m.BaslangicSaati < musaitlik.BitisSaati &&
                musaitlik.BaslangicSaati < m.BitisSaati);

            if (cakismaVar)
            {
                ModelState.AddModelError(string.Empty, "Bu veteriner için seçilen gün ve saat aralığında başka bir müsaitlik kaydıyla çakışma var.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Veterinerler = new SelectList(_baglam.Veterinerler.ToList(), "Id", "AdSoyad", musaitlik.VeterinerId);
                ViewBag.Gunler = GunListesiGetir();
                return View(musaitlik);
            }

            _baglam.VeterinerMusaitlikleri.Update(musaitlik);
            await _baglam.SaveChangesAsync();
            TempData["Basari"] = "Müsaitlik saati güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var musaitlik = await _baglam.VeterinerMusaitlikleri.FindAsync(id);
            if (musaitlik == null) return NotFound();

            _baglam.VeterinerMusaitlikleri.Remove(musaitlik);
            await _baglam.SaveChangesAsync();
            TempData["Basari"] = "Müsaitlik saati silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}