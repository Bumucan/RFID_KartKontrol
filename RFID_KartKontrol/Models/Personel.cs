using System;
using System.ComponentModel.DataAnnotations;

namespace RFID_KartKontrol.Models
{
    public class Personel
    {
        public int PersonelID { get; set; }

        [Required] // null olamaz, form binding için
        public string AdSoyad { get; set; } = null!;

        [Required] // null olamaz
        public string KartUID { get; set; } = null!;

        public Personel() { } // parametresiz constructor EF için

        public Personel(string adSoyad, string kartUID)
        {
            AdSoyad = adSoyad ?? throw new ArgumentNullException(nameof(adSoyad));
            KartUID = kartUID ?? throw new ArgumentNullException(nameof(kartUID));
        }
    }
}
