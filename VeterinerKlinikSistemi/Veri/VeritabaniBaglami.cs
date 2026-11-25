using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using VeterinerKlinikSistemi.Models;

namespace VeterinerKlinikSistemi.Veri
{
    public class VeritabaniBaglami : IdentityDbContext<Kullanici>
    {
        public VeritabaniBaglami(DbContextOptions<VeritabaniBaglami> options)
            : base(options)
        {
        }

        public DbSet<Klinik> Klinikler { get; set; }
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<Veteriner> Veterinerler { get; set; }
        public DbSet<VeterinerMusaitlik> VeterinerMusaitlikleri { get; set; }
        public DbSet<EvcilHayvan> EvcilHayvanlar { get; set; }
        public DbSet<AsiKaydi> AsiKayitlari { get; set; }
        public DbSet<Randevu> Randevular { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // SQL Server'da çoklu silme (cascade path) hatasını önlemek için ilişkileri sınırlandırıyoruz
            builder.Entity<Randevu>()
                .HasOne(r => r.EvcilHayvan)
                .WithMany(e => e.Randevular)
                .HasForeignKey(r => r.EvcilHayvanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Randevu>()
                .HasOne(r => r.Hizmet)
                .WithMany()
                .HasForeignKey(r => r.HizmetId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Randevu>()
                .HasOne(r => r.Veteriner)
                .WithMany(v => v.Randevular)
                .HasForeignKey(r => r.VeterinerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<EvcilHayvan>()
                .HasOne(e => e.Sahip)
                .WithMany(k => k.EvcilHayvanlar)
                .HasForeignKey(e => e.SahipId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Veteriner>()
                .HasOne(v => v.Klinik)
                .WithMany(k => k.Veterinerler)
                .HasForeignKey(v => v.KlinikId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}