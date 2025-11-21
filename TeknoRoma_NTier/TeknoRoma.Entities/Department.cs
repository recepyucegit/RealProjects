using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Entities;

/// <summary>
/// Departman Entity
/// TEKNOROMA'da toplam 30 departman var (mağaza başına ortalama 5-6 departman)
/// Örn: Bilgisayar Donanımları, Cep Telefonları, Kameralar, Satış, Depo, Muhasebe
///
/// NEDEN ÖNEMLİ?
/// - Her çalışan bir departmana atanır
/// - Departman bazında performans raporları
/// - Yetkilendirme departman bazında yapılır
/// - Her departmanın ayrı hedefleri var
/// </summary>
public class Department : BaseEntity
{
    /// <summary>
    /// Departman Adı
    /// Örn: "Bilgisayar Donanımları", "Satış Departmanı", "Depo", "Muhasebe"
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Departman Kodu
    /// Kısa unique kod
    /// Örn: "BIL-DON", "SATIS", "DEPO"
    /// </summary>
    public string DepartmentCode { get; set; } = string.Empty;

    /// <summary>
    /// Departman Açıklaması
    /// İş tanımları ve sorumluluklar
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Hangi mağazaya ait? (Foreign Key)
    /// Her departman bir mağazaya aittir
    /// NEDEN? Kadıköy mağazasının "Satış" departmanı ile Bornova'nınki farklı
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// Departmanın rolü/türü
    /// Örn: SubeYoneticisi, KasaSatis, Depo, Muhasebe, TeknikServis
    /// NEDEN? Yetkilendirme için departman türünü bilmemiz gerekiyor
    /// Bu departmandaki çalışanlar bu rolün yetkilerine sahip olur
    /// </summary>
    public UserRole DepartmentType { get; set; }

    /// <summary>
    /// Departman aktif mi?
    /// Kapatılan departmanlar için false (örn: o mağazada artık teknik servis yok)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Departman için aylık hedef satış (sadece satış departmanları için)
    /// Null olabilir (depo, muhasebe gibi departmanlarda hedef olmaz)
    /// </summary>
    public decimal? MonthlySalesTarget { get; set; }


    // ====== NAVIGATION PROPERTIES ======

    /// <summary>
    /// Departmanın bağlı olduğu mağaza
    /// Many-to-One ilişki (Birden fazla departman bir mağazaya ait)
    /// </summary>
    public virtual Store Store { get; set; } = null!;

    /// <summary>
    /// Bu departmandaki çalışanlar
    /// One-to-Many ilişki (Bir departmanda birden fazla çalışan)
    /// Örn: Satış departmanında 10 kişi çalışıyor
    /// </summary>
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
