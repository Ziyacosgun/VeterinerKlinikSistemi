using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinikSistemi.Models;
using VeterinerKlinikSistemi.Veri;

namespace VeterinerKlinikSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HizmetController : Controller
    {
        private readonly VeritabaniBaglami _baglam;

        public HizmetController(VeritabaniBaglami baglam)
        {
            _baglam = baglam;
        }

        public async Task<IActionResult> Index()
        {
            var hizmetler = await _baglam.Hizmetler
                .Include(h => h.Klinik)
                .ToListAsync();
            return View(hizmetler);
        }

        [HttpGet]
        public IActionResult Ekle()
        {
            ViewBag.Klinikler = new SelectList(_baglam.Klinikler.ToList(), "Id", "Ad");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(Hizmet hizmet)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Klinikler = new SelectList(_baglam.Klinikler.ToList(), "Id", "Ad", hizmet.KlinikId);
                return View(hizmet);
            }

            _baglam.Hizmetler.Add(hizmet);
            await _baglam.SaveChangesAsync();
            TempData["Basari"] = "Hizmet başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Duzenle(int id)
        {
            var hizmet = await _baglam.Hizmetler.FindAsync(id);
            if (hizmet == null) return NotFound();

            ViewBag.Klinikler = new SelectList(_baglam.Klinikler.ToList(), "Id", "Ad", hizmet.KlinikId);
            return View(hizmet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(Hizmet hizmet)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Klinikler = new SelectList(_baglam.Klinikler.ToList(), "Id", "Ad", hizmet.KlinikId);
                return View(hizmet);
            }

            _baglam.Hizmetler.Update(hizmet);
            await _baglam.SaveChangesAsync();
            TempData["Basari"] = "Hizmet bilgileri güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var hizmet = await _baglam.Hizmetler.FindAsync(id);
            if (hizmet == null) return NotFound();

            _baglam.Hizmetler.Remove(hizmet);
            await _baglam.SaveChangesAsync();
            TempData["Basari"] = "Hizmet silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}