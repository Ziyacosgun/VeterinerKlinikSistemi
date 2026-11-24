using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinikSistemi.Models;
using VeterinerKlinikSistemi.Veri;

namespace VeterinerKlinikSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class KlinikController : Controller
    {
        private readonly VeritabaniBaglami _baglam;

        public KlinikController(VeritabaniBaglami baglam)
        {
            _baglam = baglam;
        }

        public async Task<IActionResult> Index()
        {
            var klinikler = await _baglam.Klinikler.ToListAsync();
            return View(klinikler);
        }

        [HttpGet]
        public IActionResult Ekle()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(Klinik klinik)
        {
            if (!ModelState.IsValid) return View(klinik);

            _baglam.Klinikler.Add(klinik);
            await _baglam.SaveChangesAsync();
            TempData["Basari"] = "Klinik başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Duzenle(int id)
        {
            var klinik = await _baglam.Klinikler.FindAsync(id);
            if (klinik == null) return NotFound();

            return View(klinik);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(Klinik klinik)
        {
            if (!ModelState.IsValid) return View(klinik);

            _baglam.Klinikler.Update(klinik);
            await _baglam.SaveChangesAsync();
            TempData["Basari"] = "Klinik bilgileri güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var klinik = await _baglam.Klinikler.FindAsync(id);
            if (klinik == null) return NotFound();

            _baglam.Klinikler.Remove(klinik);
            await _baglam.SaveChangesAsync();
            TempData["Basari"] = "Klinik silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}