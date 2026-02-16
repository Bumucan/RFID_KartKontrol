using Microsoft.AspNetCore.Mvc;
using RFID_KartKontrol.Models;
using System.Linq;

namespace RFID_KartKontrol.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            // 1. Formdan gelen veriler kurallara uyuyor mu? (Boş mu dolu mu?)
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 2. Senin belirlediğin Admin ve Şifre kontrolü
            // Modeldeki property isimleri LoginViewModel.cs dosyandakilerle aynı olmalı (KullaniciAdi, Sifre)
            if (model.KullaniciAdi == "admin" && model.Sifre == "1234")
            {
                // Giriş Başarılı -> Session'a kaydet
                HttpContext.Session.SetString("AdminLoggedIn", "true");
                return RedirectToAction("Index");
            }

            // 3. Hatalı giriş ise
            ViewBag.Hata = "Kullanıcı adı veya şifre yanlış!";

            // Modeli sayfaya geri gönderiyoruz ki kutular boşalmasın, kullanıcı hatasını görsün
            return View(model);
        }

        // Admin ana sayfa
        public IActionResult Index()
        {
            // Giriş Kayıtlarını Personel Tablosuyla Birleştirip Çekiyoruz
            var liste = (from k in _context.GirisKayitlari
                         join p in _context.Personel
                         on k.KartUID equals p.KartUID into ps
                         from personel in ps.DefaultIfEmpty() // Personel yoksa boş gelsin (Left Join)
                         orderby k.KayitID descending // En yeni kayıt en üstte
                         select new GirisKaydi
                         {
                             KayitID = k.KayitID,
                             KartUID = k.KartUID,
                             Tarih = k.Tarih,
                             Islem = k.Islem ?? 0,
                             // Personel varsa ismini al, yoksa uyarı yaz
                             PersonelAd = (personel != null) ? personel.AdSoyad : "YETKİSİZ / TANIMSIZ"
                         }).ToList();

            return View(liste);
        }

        // Admin çıkış
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminLoggedIn");
            return RedirectToAction("Login");
        }

        // Tüm kartlar
        public IActionResult Kartlar()
        {
            var liste = _context.Kartlar.OrderBy(k => k.KartUID).ToList();
            return View(liste);
        }

        // Yetkisiz kartlar (KayitliMi = false)
        public IActionResult YetkisizKartlar()
        {
            var liste = _context.Kartlar
                .Where(k => !k.KayitliMi)
                .OrderBy(k => k.KartUID)
                .ToList();
            return View(liste);
        }

        // Kartı aktif/pasif yap
        public IActionResult ToggleKart(int id)
        {
            var kart = _context.Kartlar.FirstOrDefault(k => k.KartID == id);
            if (kart != null)
            {
                kart.KayitliMi = !kart.KayitliMi;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // Kartı yetkisiz yap
        public IActionResult MakeYetkisiz(int id)
        {
            var kart = _context.Kartlar.FirstOrDefault(k => k.KartID == id);
            if (kart != null)
            {
                kart.KayitliMi = false;
                _context.SaveChanges();
            }
            return RedirectToAction("YetkisizKartlar");
        }

        // Kartı Sil (View dosyasında kullanılan ama eksik olan metot)
        public IActionResult DeleteKart(int id)
        {
            var kart = _context.Kartlar.Find(id);
            if (kart != null)
            {
                _context.Kartlar.Remove(kart);
                _context.SaveChanges();
            }
            // Silme işleminden sonra geldiği yere dönsün
            return RedirectToAction("YetkisizKartlar");
        }

        // Personel listesi
        public IActionResult Personeller()
        {
            // _context.Personel yerine Personeller kullanıldı
            var liste = _context.Personel.OrderBy(p => p.AdSoyad).ToList();
            return View(liste);
        }

        // Personel düzenleme formunu göster
        public IActionResult PersonelDuzenle(int id)
        {
            var personel = _context.Personel.Find(id);
            if (personel == null) return NotFound();
            return View(personel);
        }

        // Düzenlenen personeli kaydet
        [HttpPost]
        public IActionResult PersonelDuzenle(Personel model)
        {
            if (!ModelState.IsValid) return View(model);

            var personel = _context.Personel.Find(model.PersonelID);
            if (personel != null)
            {
                personel.AdSoyad = model.AdSoyad;
                personel.KartUID = model.KartUID;
                _context.SaveChanges();
            }
            return RedirectToAction("Personeller");
        }

        // Personel sil
        public IActionResult DeletePersonel(int id)
        {
            var personel = _context.Personel.Find(id);
            if (personel != null)
            {
                _context.Personel.Remove(personel);

                // Personel silinince kartı da boşa çıkaralım (Yetkisiz yapalım)
                var kart = _context.Kartlar.FirstOrDefault(k => k.KartUID == personel.KartUID);
                if (kart != null)
                    kart.KayitliMi = false;

                _context.SaveChanges();
            }
            return RedirectToAction("Personeller");
        }

        // --- YENİ EKLENEN KISIMLAR ---

        // 1. Personel Ekleme Sayfası (GET)
        [HttpGet]
        public IActionResult PersonelEkle(string uid)
        {
            // Eğer UID gelmediyse yetkisiz kartlara geri yolla
            if (string.IsNullOrEmpty(uid))
            {
                return RedirectToAction("YetkisizKartlar");
            }
            // Gelen UID'yi View'a taşıyoruz ki input içine otomatik dolsun
            // Bunu View içinde Context.Request.Query["uid"] ile de alabilirsin 
            // ama view model ile göndermek daha temizdir, şimdilik senin view yapına dokunmadım.
            return View();
        }

        // 2. Personel Kaydetme İşlemi (POST)
        [HttpPost]
        public IActionResult PersonelEkle(Personel p)
        {
            // 1. Önce Kartı Bul ve "KayitliMi" yap (Aktifleştir)
            var kart = _context.Kartlar.FirstOrDefault(x => x.KartUID == p.KartUID);
            if (kart != null)
            {
                kart.KayitliMi = true;
            }

            // 2. Personeli Ekle
            _context.Personel.Add(p);
            _context.SaveChanges();

            // İşlem bitince personel listesine git
            return RedirectToAction("Personeller");
        }
        public IActionResult Ozet()
        {
            // --- 1. KUTU İÇİN: TÜM PERSONEL LİSTESİ ---
            ViewBag.ToplamPersonel = _context.Personel.Count();
            ViewBag.TumPersonelListesi = _context.Personel.OrderBy(x => x.AdSoyad).ToList();


            // --- 2. KUTU İÇİN: ŞU AN İÇERİDE OLANLAR ---
            // Önce kimlerin son hareketi "GİRİŞ(1)" onu buluyoruz
            var sonHareketler = _context.GirisKayitlari
                .AsEnumerable()
                .GroupBy(x => x.KartUID)
                .Select(g => g.OrderByDescending(x => x.KayitID).FirstOrDefault())
                .Where(x => x != null && x.Islem == 1) // Null olmayanları ve Giriş yapanları aldık
                .ToList();

            ViewBag.IceridekilerSayisi = sonHareketler.Count();

            // DÜZELTİLEN SATIR BURASI (x! eklendi)
            // Listemizdeki elemanların 'null' olmadığından eminiz, ünlemle bunu belirtiyoruz.
            var icerdekiUIDler = sonHareketler.Select(x => x!.KartUID).ToList();

            // İçerideki UID'lere sahip personelleri bul
            ViewBag.IceridekilerListesi = _context.Personel
                .Where(p => icerdekiUIDler.Contains(p.KartUID))
                .ToList();


            // --- 3. KUTU İÇİN: BUGÜN GİRENLER ---
            var bugunGirenler = (from g in _context.GirisKayitlari
                                 join p in _context.Personel on g.KartUID equals p.KartUID
                                 where g.Tarih.Date == DateTime.Today && g.Islem == 1
                                 orderby g.Tarih descending
                                 select new
                                 {
                                     AdSoyad = p.AdSoyad,
                                     Saat = g.Tarih,
                                     KartUID = p.KartUID
                                 }).ToList();

            ViewBag.BugunGirisSayisi = bugunGirenler.Count();
            ViewBag.BugunGirenlerListesi = bugunGirenler;


            // --- 4. KUTU İÇİN: YETKİSİZ (Sadece Sayı) ---
            ViewBag.YetkisizSayisi = _context.GirisKayitlari
                .Where(x => x.Islem == 2)
                .Count();

            return View();
        }
    }
}