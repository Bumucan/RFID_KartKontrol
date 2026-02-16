using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // <-- BUNU EKLEMEYİ UNUTMA

namespace RFID_KartKontrol.Models
{
    public class GirisKaydi
    {
        [Key]
        public int KayitID { get; set; }

        public string KartUID { get; set; } = null!;

        public DateTime Tarih { get; set; }

        public int? Islem { get; set; }

        // --- YENİ EKLENEN KISIM ---
        [NotMapped] // Veritabanına kaydedilmez, sadece ekranda görünür
        public string? PersonelAd { get; set; }
        // ---------------------------

        public GirisKaydi() { }

        public GirisKaydi(string kartUID, DateTime tarih, int islem)
        {
            KartUID = kartUID;
            Tarih = tarih;
            Islem = islem;
        }
    }
}