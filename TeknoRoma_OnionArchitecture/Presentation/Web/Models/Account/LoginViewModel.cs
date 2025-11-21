using System.ComponentModel.DataAnnotations;

namespace Web.Models.Account
{
    /// <summary>
    /// Login ViewModel
    ///
    /// AMAÇ:
    /// - Kullanıcı girişi için form modeli
    /// - Email ve şifre ile giriş
    /// - "Beni Hatırla" özelliği
    ///
    /// KULLANICILAR:
    /// - halukbey@teknoroma.com (Şube Müdürü)
    /// - gulsatar@teknoroma.com (Kasa Satış)
    /// - fahricepci@teknoroma.com (Mobil Satış)
    /// - kerimzulaci@teknoroma.com (Depo)
    /// - feyzaparagoz@teknoroma.com (Muhasebe)
    /// - ozgunkablocu@teknoroma.com (Teknik Servis)
    ///
    /// Şifre: TeknoRoma123!
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
