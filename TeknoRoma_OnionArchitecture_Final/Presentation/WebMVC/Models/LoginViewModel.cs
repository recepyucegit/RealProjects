// ===================================================================================
// TEKNOROMA - LOGIN VIEW MODEL
// ===================================================================================
//
// VIEW MODEL NEDİR?
// View (sayfa) ile Controller arasında veri taşıyan modeldir
// Database entity'lerinden farklıdır, sadece UI için gerekli alanları içerir
//
// NEDEN AYRI MODEL?
// 1. GÜVENLIK: Database entity'lerini direkt View'da kullanmak güvenlik riski
// 2. ESNEKLIK: UI ihtiyaçlarına göre özel alanlar eklenebilir
// 3. VALIDASYON: Sadece bu form için gereken kurallar tanımlanır
//
// ===================================================================================

using System.ComponentModel.DataAnnotations;

namespace Presentation.WebMVC.Models
{
    /// <summary>
    /// Login formu için View Model
    ///
    /// KULLANIM:
    /// - Login sayfasında form binding için kullanılır
    /// - ModelState validasyonu için Data Annotations içerir
    /// - View'da @model LoginViewModel şeklinde kullanılır
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Kullanıcı Email Adresi
        ///
        /// DATA ANNOTATIONS:
        /// - [Required]: Boş bırakılamaz (ModelState.IsValid kontrolü)
        /// - [EmailAddress]: Geçerli email formatı kontrolü
        /// - [Display]: Label metni (View'da otomatik kullanılır)
        ///
        /// ÖRNEK KULLANIM (View):
        /// <label asp-for="Email"></label> → "Email Adresi" yazdırır
        /// </summary>
        [Required(ErrorMessage = "Email adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [Display(Name = "Email Adresi")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Kullanıcı Şifresi
        ///
        /// DATA ANNOTATIONS:
        /// - [Required]: Boş bırakılamaz
        /// - [DataType(DataType.Password)]: Password input olarak render edilir
        ///   (Yazılan karakterler gizlenir: ****)
        /// </summary>
        [Required(ErrorMessage = "Şifre zorunludur")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Beni Hatırla Checkbox'ı
        ///
        /// KULLANIM:
        /// true ise → Cookie'nin ömrü uzun olur (2 hafta)
        /// false ise → Browser kapanınca cookie silinir
        ///
        /// GÜVENLİK NOTU:
        /// Paylaşımlı bilgisayarlarda "Beni Hatırla" önerilmez!
        /// </summary>
        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }
}
