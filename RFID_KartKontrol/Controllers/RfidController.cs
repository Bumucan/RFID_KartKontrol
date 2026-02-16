using Microsoft.AspNetCore.Mvc;
using RFID_KartKontrol.Models;
using System;
using System.Linq;

namespace RFID_KartKontrol.Controllers
{
    public class RfidController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RfidController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Okut(string uid)
        {
            if (string.IsNullOrEmpty(uid))
                return Content("UID_YOK");

            // 1. Kart DB'de var mı kontrol et
            var kart = _context.Kartlar.FirstOrDefault(k => k.KartUID == uid);

      
            if (kart == null)
            {
                // Kartı veritabanına ekle (Pasif olarak)
                kart = new Kart
                {
                    KartUID = uid,
                    KayitliMi = false
                };
                _context.Kartlar.Add(kart);

               
                var yetkisizKayit = new GirisKaydi
                {
                    KartUID = uid,
                    Tarih = DateTime.Now,
                    Islem = 2 
                };
                _context.GirisKayitlari.Add(yetkisizKayit);
                _context.SaveChanges();

                return Content("UID_YOK");
            }

            // Kart Var ama Henüz Personel Atanmamış (Pasif) -> YETKİSİZ (2)
            if (!kart.KayitliMi)
            {
                var yetkisizKayit = new GirisKaydi
                {
                    KartUID = uid,
                    Tarih = DateTime.Now,
                    Islem = 2 
                };
                _context.GirisKayitlari.Add(yetkisizKayit);
                _context.SaveChanges();

                return Content("UID_YOK");
            }

            // SENARYO 3: Kart Yetkili, Personel Kontrolü
            var personel = _context.Personel.FirstOrDefault(p => p.KartUID == uid);

            if (personel == null)
            {
                // Yetkili kart ama personel tablosunda karşılığı silinmiş -> YETKİSİZ (2)
                var eksikPersonelKayit = new GirisKaydi
                {
                    KartUID = uid,
                    Tarih = DateTime.Now,
                    Islem = 2 // <-- ARTIK TURUNCU GÖRÜNECEK
                };
                _context.GirisKayitlari.Add(eksikPersonelKayit);
                _context.SaveChanges();

                return Content("PERSONEL_YOK");
            }

            // SENARYO 4: Giriş / Çıkış İşlemi (Normal Çalışma)

            // Son kaydı bul
            var sonKayit = _context.GirisKayitlari
                .Where(x => x.KartUID == uid)
                .OrderByDescending(x => x.KayitID)
                .FirstOrDefault();

           
            // Sadece son işlem "GİRİŞ (1)" ise "ÇIKIŞ (0)" yap.
            // Diğer tüm durumlarda (Yoksa, Çıkışsa veya Yetkisizse) "GİRİŞ (1)" yap.
            int yeniIslem = (sonKayit != null && sonKayit.Islem == 1) ? 0 : 1;

            var yeniKayit = new GirisKaydi
            {
                KartUID = uid,
                Tarih = DateTime.Now,
                Islem = yeniIslem
            };
            _context.GirisKayitlari.Add(yeniKayit);
            _context.SaveChanges();

            // Arduino'ya Cevap: OK_GIRIS_Ahmet veya OK_CIKIS_Ahmet
            string islemTuru = (yeniIslem == 1 ? "GIRIS" : "CIKIS");
            return Content($"OK_{islemTuru}_{personel.AdSoyad}");
        }

        // Listeleme Sayfası
        public IActionResult Index()
        {
            // LINQ ile "Left Join" yapıyoruz (Kayıt var ama personel yoksa bile getirir)
            var liste = (from k in _context.GirisKayitlari
                         join p in _context.Personel
                         on k.KartUID equals p.KartUID into ps
                         from personel in ps.DefaultIfEmpty() // Eşleşen personel yoksa boş geç
                         orderby k.KayitID descending
                         select new GirisKaydi
                         {
                             KayitID = k.KayitID,
                             KartUID = k.KartUID,
                             Tarih = k.Tarih,
                             Islem = k.Islem ?? 0,
                             // Eğer personel bulunduysa adını yaz, bulunamadıysa "YETKİSİZ" yaz
                             PersonelAd = (personel != null) ? personel.AdSoyad : "YETKİSİZ / TANIMSIZ"
                         }).ToList();

            return View(liste);
        }
       
    }
}