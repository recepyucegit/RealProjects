using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Entities;

/// <summary>
/// Çalışan Entity
/// TEKNOROMA'da toplam 258 çalışan var
/// 20 merkez personeli + 238 şube personeli
///
/// ÖNEMLİ: Bu entity ASP.NET Identity User tablosuyla ilişkili olacak
/// Bir çalışan = Bir kullanıcı hesabı
/// Authentication ve Authorization Identity üzerinden
///
/// NEDEN Bu Kadar Detaylı?
/// - Haluk Bey: "Hangi çalışanım ne kadar satmış, performansını görmek istiyorum"
/// - Feyza Paragöz: "Maaş ödemeleri için çalışan bilgileri şart"
/// - Özgün Kablocu: "Teknik servis kayıtları çalışan bazında tutulmalı"
/// </summary>
public class Employee : BaseEntity
{
    /// <summary>
    /// ASP.NET Identity User ID
    /// Identity tablosundaki kullanıcı ile eşleştirme için
    /// NEDEN? Authentication ve Authorization Identity üzerinden
    /// Giriş yapan kullanıcının Employee kaydına ulaşmak için
    /// </summary>
    public string? IdentityUserId { get; set; }

    /// <summary>
    /// TC Kimlik Numarası
    /// UNIQUE constraint olacak
    /// 11 haneli sayı
    /// NEDEN? Yasal zorunluluk, çalışan takibi
    /// </summary>
    public string IdentityNumber { get; set; } = string.Empty;

    /// <summary>
    /// Ad
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Soyad
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Email adresi
    /// Identity tablosundaki email ile senkron olacak
    /// Raporlar ve bildirimler için kullanılır
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Telefon numarası
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Cinsiyet
    /// İnsan Kaynakları raporları için
    /// </summary>
    public Gender Gender { get; set; }

    /// <summary>
    /// Doğum Tarihi
    /// Yaş hesabı için kullanılır
    /// </summary>
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// İşe başlama tarihi
    /// NEDEN? Kıdem hesabı, izin hakları, prim hesaplamaları için
    /// </summary>
    public DateTime HireDate { get; set; }

    /// <summary>
    /// Maaş
    /// NEDEN? Feyza Paragöz (Muhasebe) gider raporu için
    /// Decimal: Para birimi için hassas hesaplama
    /// UYARI: Hassas veri! DTO'larda dikkatli kullanılmalı
    /// </summary>
    public decimal Salary { get; set; }

    /// <summary>
    /// Hangi mağazada çalışıyor? (Foreign Key)
    /// Her çalışan bir şubeye atanır
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// Hangi departmanda çalışıyor? (Foreign Key)
    /// Örn: Satış, Depo, Muhasebe, Teknik Servis
    /// </summary>
    public int DepartmentId { get; set; }

    /// <summary>
    /// Çalışanın rolü
    /// NEDEN? Yetkilendirme için
    /// Haluk Bey'in istediği: "Kullanıcı sadece yetkisi dahilindeki bölümlere erişebilmeli"
    /// SubeYoneticisi, KasaSatis, Depo, Muhasebe, TeknikServis
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Satış kotası (sadece satış ekibi için)
    /// Gül Satar'ın istediği: "Satış kotası 10.000 TL, üzerindeki satıştan %10 prim"
    /// Null olabilir çünkü her çalışan satış yapmaz (depo, muhasebe vb.)
    /// </summary>
    public decimal? SalesQuota { get; set; }

    /// <summary>
    /// Adres
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Şehir
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Çalışan aktif mi?
    /// İşten ayrılan çalışanlar için false
    /// NEDEN? Soft Delete - İşten ayrılan çalışanların geçmiş kayıtları silinmez
    /// </summary>
    public bool IsActive { get; set; } = true;


    // ====== CALCULATED PROPERTIES ======

    /// <summary>
    /// Çalışanın tam adı
    /// Raporlarda ve UI'da kullanılır
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Yaş hesaplama
    /// İnsan Kaynakları analizleri için
    /// </summary>
    public int Age => DateTime.Now.Year - BirthDate.Year;

    /// <summary>
    /// Kıdem (yıl olarak)
    /// İzin hakları ve prim hesapları için
    /// </summary>
    public int YearsOfService => DateTime.Now.Year - HireDate.Year;


    // ====== NAVIGATION PROPERTIES ======

    /// <summary>
    /// Çalışanın bağlı olduğu mağaza
    /// Many-to-One ilişki
    /// </summary>
    public virtual Store Store { get; set; } = null!;

    /// <summary>
    /// Çalışanın bağlı olduğu departman
    /// Many-to-One ilişki
    /// </summary>
    public virtual Department Department { get; set; } = null!;

    /// <summary>
    /// Çalışanın yaptığı satışlar
    /// One-to-Many ilişki
    /// Haluk Bey'in raporu: "Hangi çalışanım ne kadar satmış"
    /// Gül Satar'ın prim hesabı için
    /// </summary>
    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    /// <summary>
    /// Çalışana yapılan maaş ödemeleri
    /// Expense tablosundan gelecek (ExpenseType = CalisanOdemesi)
    /// Feyza Paragöz'ün takip ettiği ödemeler
    /// </summary>
    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    /// <summary>
    /// Çalışanın bildirdiği teknik servis kayıtları
    /// Sorun bildiren çalışan
    /// </summary>
    public virtual ICollection<TechnicalService> ReportedTechnicalServices { get; set; } = new List<TechnicalService>();

    /// <summary>
    /// Çalışanın çözdüğü teknik servis kayıtları
    /// Özgün Kablocu ve ekibinin üzerine aldığı sorunlar
    /// </summary>
    public virtual ICollection<TechnicalService> AssignedTechnicalServices { get; set; } = new List<TechnicalService>();
}
