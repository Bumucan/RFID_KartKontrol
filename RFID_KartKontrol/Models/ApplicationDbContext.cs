using Microsoft.EntityFrameworkCore;
using RFID_KartKontrol.Models;
using System.Collections.Generic;

namespace RFID_KartKontrol.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Kart> Kartlar { get; set; }
        public DbSet<Personel> Personel { get; set; }
        public DbSet<GirisKaydi> GirisKayitlari { get; set; }
    }
}
