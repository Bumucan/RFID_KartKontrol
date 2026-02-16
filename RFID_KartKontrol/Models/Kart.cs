using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RFID_KartKontrol.Models
{
    [Table("Kartlar")] // SQL tablosu adı
    public class Kart
    {
        [Key] // EF Core için PK
        public int KartID { get; set; }

        [Required] // Null atanamaz
        public string KartUID { get; set; } = null!; // ← düzeltme

        public bool KayitliMi { get; set; }

        // Parameterless constructor EF Core için gerekli
        public Kart() { }

        // Kullanıcı veya kod ile oluşturulacak constructor
        public Kart(string kartUID, bool kayitliMi)
        {
            KartUID = kartUID ?? throw new ArgumentNullException(nameof(kartUID));
            KayitliMi = kayitliMi;
        }
    }
}
