using System.ComponentModel.DataAnnotations;

namespace RFID_KartKontrol.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Lütfen kullanıcı adını girin.")]
        // Sonuna = null!; ekledik
        public string KullaniciAdi { get; set; } = null!;

        [Required(ErrorMessage = "Lütfen şifrenizi girin.")]
        [DataType(DataType.Password)]
        // Sonuna = null!; ekledik
        public string Sifre { get; set; } = null!;
    }
}