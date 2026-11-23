using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VeterinerKlinikSistemi.Models;

namespace VeterinerKlinikSistemi.Controllers
{
    public class HesapController : Controller
    {
        private readonly UserManager<Kullanici> _kullaniciYoneticisi;
        private readonly SignInManager<Kullanici> _oturumYoneticisi;

        public HesapController(UserManager<Kullanici> kullaniciYoneticisi, SignInManager<Kullanici> oturumYoneticisi)
        {
            _kullaniciYoneticisi = kullaniciYoneticisi;
            _oturumYoneticisi = oturumYoneticisi;
        }

        [HttpGet]
        public IActionResult Giris() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Giris(GirisViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var sonuc = await _oturumYoneticisi.PasswordSignInAsync(
                model.Email, model.Sifre, model.BeniHatirla, lockoutOnFailure: false);

            if (sonuc.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Kayit() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Kayit(KayitViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var kullanici = new Kullanici
            {
                UserName = model.Email,
                Email = model.Email,
                AdSoyad = model.AdSoyad
            };

            var sonuc = await _kullaniciYoneticisi.CreateAsync(kullanici, model.Sifre);
            if (sonuc.Succeeded)
            {
                await _kullaniciYoneticisi.AddToRoleAsync(kullanici, "Uye");
                await _oturumYoneticisi.SignInAsync(kullanici, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var hata in sonuc.Errors)
            {
                ModelState.AddModelError(string.Empty, hata.Description);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cikis()
        {
            await _oturumYoneticisi.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ErisimReddedildi() => View();
    }
}