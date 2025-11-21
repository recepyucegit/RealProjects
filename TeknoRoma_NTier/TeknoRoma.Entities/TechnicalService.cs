using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Entities;

/// <summary>
/// Teknik Servis Entity
/// Özgün Kablocu'nun (Teknik Servis Temsilcisi) yönettiği sorun kayıtları
///
/// 2 Tip Sorun:
/// 1. Müşteri sorunları (Satış sonrası ürün arızası, teknik destek)
/// 2. Sistem sorunları (Yazılım hatası, network sorunu, donanım arızası)
///
/// NEDEN ÖNEMLİ?
/// - Özgün Kablocu: "Windows uygulamasından sorun bildirimi yapabilmeliler"
/// - Haluk Bey: "Teknik sorunların ne kadar sürede çözüldüğünü görmek istiyorum"
/// - Müşteri memnuniyeti için hızlı çözüm şart
/// </summary>
public class TechnicalService : BaseEntity
{
    /// <summary>
    /// Servis Numarası (Benzersiz)
    /// Format: "TS-2024-00001"
    /// Takip numarası
    /// </summary>
    public string ServiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Sorun Başlığı
    /// Kısa özet - Bir satırda sorun ne?
    /// Örn: "Ürün açılmıyor", "Network bağlantısı kopuyor"
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Sorun Açıklaması
    /// Detaylı açıklama - Ne oldu? Nasıl oldu?
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Hangi mağaza? (Foreign Key)
    /// Sorunun bildirildiği mağaza
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// Sorunu bildiren çalışan (Foreign Key)
    /// Özgün Kablocu: "Çalışanlar Windows uygulamasından sorun bildirebilmeli"
    /// </summary>
    public int ReportedByEmployeeId { get; set; }

    /// <summary>
    /// Sorunu çözen teknik servis elemanı (Foreign Key)
    /// Null olabilir (henüz atanmadıysa)
    /// Özgün Kablocu kendisi veya ekibinden biri
    /// </summary>
    public int? AssignedToEmployeeId { get; set; }

    /// <summary>
    /// Müşteri sorunu mu?
    /// true: Müşteri ürün arızası (satış sonrası)
    /// false: Şube içi teknik sorun (yazılım, donanım, network)
    /// NEDEN? İki tip sorunun takibi ve önceliği farklı
    /// </summary>
    public bool IsCustomerIssue { get; set; }

    /// <summary>
    /// Hangi müşteri? (Foreign Key)
    /// Sadece IsCustomerIssue = true ise dolu
    /// Null olabilir (sistem sorunlarında müşteri yok)
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Hangi ürün? (Foreign Key)
    /// Müşteri sorunlarında hangi ürün arızalı
    /// Sistem sorunlarında hangi donanım/yazılım
    /// Null olabilir (network sorunu gibi)
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// Sorun Durumu
    /// Acik → Islemde → Tamamlandi/Cozulemedi
    /// </summary>
    public TechnicalServiceStatus Status { get; set; } = TechnicalServiceStatus.Acik;

    /// <summary>
    /// Öncelik Seviyesi
    /// 1: Düşük (kozmetik sorunlar)
    /// 2: Orta (normal işlemler)
    /// 3: Yüksek (satışları etkiliyor)
    /// 4: Kritik (tüm işlemler durdu)
    /// NEDEN? Haluk Bey: "Kritik sorunlar en önce çözülmeli"
    /// </summary>
    public int Priority { get; set; } = 2;

    /// <summary>
    /// Sorun bildirme tarihi
    /// </summary>
    public DateTime ReportedDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Çözüm tarihi
    /// Null olabilir (henüz çözülmediyse)
    /// NEDEN? SLA (Service Level Agreement) kontrolü
    /// Hedef: 24 saat içinde çözüm
    /// </summary>
    public DateTime? ResolvedDate { get; set; }

    /// <summary>
    /// Çözüm açıklaması
    /// Ne yapıldı? Nasıl çözüldü?
    /// Null olabilir (henüz çözülmediyse)
    /// Özgün Kablocu: "Gelecekte benzer sorunlar için referans olsun"
    /// </summary>
    public string? Resolution { get; set; }

    /// <summary>
    /// Maliyet
    /// Değişen parça varsa maliyeti
    /// Null olabilir (maliyetsiz çözüm)
    /// </summary>
    public decimal? Cost { get; set; }


    // ====== CALCULATED PROPERTY ======

    /// <summary>
    /// Çözüm süresi (saat olarak)
    /// ResolvedDate - ReportedDate
    /// Null ise henüz çözülmemiş
    /// </summary>
    public double? ResolutionTimeInHours
    {
        get
        {
            if (ResolvedDate.HasValue)
                return (ResolvedDate.Value - ReportedDate).TotalHours;
            return null;
        }
    }


    // ====== NAVIGATION PROPERTIES ======

    /// <summary>
    /// Sorunun bildirildiği mağaza
    /// Many-to-One ilişki
    /// </summary>
    public virtual Store Store { get; set; } = null!;

    /// <summary>
    /// Sorunu bildiren çalışan
    /// Many-to-One ilişki
    /// </summary>
    public virtual Employee ReportedByEmployee { get; set; } = null!;

    /// <summary>
    /// Sorunu çözen teknik servis elemanı
    /// Many-to-One ilişki
    /// Null olabilir (henüz atanmadıysa)
    /// </summary>
    public virtual Employee? AssignedToEmployee { get; set; }

    /// <summary>
    /// Sorunu olan müşteri (müşteri sorunlarında)
    /// Many-to-One ilişki
    /// Null olabilir (sistem sorunlarında müşteri yok)
    /// </summary>
    public virtual Customer? Customer { get; set; }

    /// <summary>
    /// Sorunlu ürün
    /// Many-to-One ilişki
    /// Null olabilir
    /// </summary>
    public virtual Product? Product { get; set; }
}
