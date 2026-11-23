using Microsoft.AspNetCore.Identity;
using VeterinerKlinikSistemi.Models;

namespace VeterinerKlinikSistemi.Veri
{
    public static class BaslangicVerisi
    {
        public static async Task RolleriVeAdminiOlusturAsync(IServiceProvider serviceProvider)
        {
            var rolYoneticisi = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var kullaniciYoneticisi = serviceProvider.GetRequiredService<UserManager<Kullanici>>();

            // 1. Rolleri oluştur
            string[] roller = { "Admin", "Veteriner", "Uye" };
            foreach (var rol in roller)
            {
                if (!await rolYoneticisi.RoleExistsAsync(rol))
                {
                    await rolYoneticisi.CreateAsync(new IdentityRole(rol));
                }
            }

            // 2. Başlangıç Admin kullanıcısını oluştur
            string adminEmail = "ziya.cosgun@sakarya.edu.tr"; 
            var adminKullanici = await kullaniciYoneticisi.FindByEmailAsync(adminEmail);

            if (adminKullanici == null)
            {
                adminKullanici = new Kullanici
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    AdSoyad = "Sistem Yöneticisi",
                    EmailConfirmed = true
                };

                var sonuc = await kullaniciYoneticisi.CreateAsync(adminKullanici, "sau");
                if (sonuc.Succeeded)
                {
                    await kullaniciYoneticisi.AddToRoleAsync(adminKullanici, "Admin");
                }
            }
        }
    }
}