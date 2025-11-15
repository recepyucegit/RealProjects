using Domain.Enums;
using static System.Formats.Asn1.AsnWriter;

namespace Domain.Entities
{
    /// <summary>
    /// Çalışan Entity
    /// TEKNOROMA'da toplam 258 çalışan var
    /// 20 merkez personeli + 238 şube personeli
    /// 
    /// ÖNEMLİ: Bu entity ASP.NET Identity User tablosuyla ilişkili olacak
    /// Bir çalışan = Bir kullanıcı hesabı
    /// </summary>
    public class Employee : BaseEntity
    {
        /// <summary>
        /// ASP.NET Identity User ID
        /// Identity tablosundaki kullanıcı ile eşleştirme için
        /// NEDEN? Authentication ve Authorization Identity üzerinden
        /// </summary>
        public string IdentityUserId { get; set; }

        /// <summary>
        /// TC Kimlik Numarası
        /// UNIQUE constraint olacak
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
        /// Email adresi
        /// Identity tablosundaki email ile senkron olacak
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Telefon numarası
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Doğum Tarihi
        /// </summary>
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// İşe başlama tarihi
        /// NEDEN? Kıdem hesabı, izin hakları için
        /// </summary>
        public DateTime HireDate { get; set; }

        /// <summary>
        /// Maaş
        /// NEDEN? Muhasebe departmanı gider raporu için
        /// Decimal: Para birimi için hassas hesaplama
        /// </summary>
        public decimal Salary { get; set; }

        /// <summary>
        /// Hangi mağazada çalışıyor? (Foreign Key)
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Hangi departmanda çalışıyor? (Foreign Key)
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// Çalışanın rolü
        /// NEDEN? Yetkilendirme için
        /// Haluk Bey'in istediği: "Kullanıcı sadece yetkisi dahilindeki bölümlere erişebilmeli"
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// Satış kotası (sadece satış ekibi için)
        /// Gül Satar'ın istediği: "Satış kotası 10.000 TL, üzerindeki satıştan %10 prim"
        /// Null olabilir çünkü her çalışan satış yapmaz
        /// </summary>
        public decimal? SalesQuota { get; set; }

        /// <summary>
        /// Çalışan aktif mi?
        /// İşten ayrılan çalışanlar için false
        /// </summary>
        public bool IsActive { get; set; } = true;


        // ====== CALCULATED PROPERTY ======

        /// <summary>
        /// Çalışanın tam adı
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";


        // ====== NAVIGATION PROPERTIES ======

        /// <summary>
        /// Çalışanın bağlı olduğu mağaza
        /// Many-to-One ilişki
        /// </summary>
        public virtual Store Store { get; set; }

        /// <summary>
        /// Çalışanın bağlı olduğu departman
        /// Many-to-One ilişki
        /// </summary>
        public virtual Department Department { get; set; }

        /// <summary>
        /// Çalışanın yaptığı satışlar
        /// One-to-Many ilişki
        /// Haluk Bey'in raporu: "Hangi çalışanım ne kadar satmış"
        /// </summary>
        public virtual ICollection<Sale> Sales { get; set; }

        /// <summary>
        /// Çalışanın aldığı maaş ödemeleri
        /// Expense tablosundan gelecek
        /// </summary>
        public virtual ICollection<Expense> Expenses { get; set; }

        /// <summary>
        /// Çalışanın çözdüğü teknik servis kayıtları
        /// </summary>
        public virtual ICollection<TechnicalService> AssignedTechnicalServices { get; set; }
    }
}