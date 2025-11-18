using Domain.Enums;
using System.Reflection;

namespace Domain.Entities
{
    /// <summary>
    /// Müşteri Entity
    /// Gül Satar'ın istediği: "Tc Kimlik Numarasını girdiğim anda bilgiler otomatik gelmeli"
    /// Haluk Bey'in istediği rapor: "Müşteri kitlesi yaş, cinsiyet analizi"
    /// </summary>
    public class Customer : BaseEntity
    {
        /// <summary>
        /// TC Kimlik Numarası
        /// UNIQUE constraint olacak (aynı TC ile birden fazla müşteri olamaz)
        /// 11 haneli string
        /// </summary>
        public string IdentityNumber { get; set; }

        /// <summary>
        /// Ad
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Soyad
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Doğum Tarihi
        /// NEDEN? Yaş analizi için (Haluk Bey'in raporu)
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Cinsiyet
        /// NEDEN? Demografik analiz için (Haluk Bey'in raporu)
        /// </summary>
        public Gender? Gender { get; set; }

        /// <summary>
        /// Email adresi
        /// Kampanya ve bilgilendirme için
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Telefon numarası
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Adres
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Şehir
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Müşteri aktif mi?
        /// Müşteri veri tabanından silmek yerine pasif ediyoruz (GDPR)
        /// </summary>
        public bool IsActive { get; set; } = true;


        // ====== CALCULATED PROPERTY ======
        // Database'e kaydedilmez, runtime'da hesaplanır

        /// <summary>
        /// Müşterinin tam adı (Ad + Soyad)
        /// NEDEN NotMapped? Database'de sütun oluşturulmasın, runtime'da hesaplansın
        /// Configuration'da [NotMapped] attribute ile işaretlenecek
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Müşterinin yaşı
        /// BirthDate'den hesaplanır
        /// </summary>
        public int? Age => BirthDate.HasValue
            ? DateTime.Now.Year - BirthDate.Value.Year
            : null;


        // ====== NAVIGATION PROPERTIES ======

        /// <summary>
        /// Müşterinin yaptığı satışlar
        /// One-to-Many ilişki (Bir müşteri birden fazla alışveriş yapabilir)
        /// </summary>
        public virtual ICollection<Sale> Sales { get; set; }
    }
}