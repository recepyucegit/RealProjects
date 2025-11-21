using System.ComponentModel.DataAnnotations;
using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Business.DTOs;

/// <summary>
/// TechnicalService DTO - Teknik servis kayıtları için
/// Özgün Kablocu'nun (Teknik Servis Temsilcisi) kullandığı DTO'lar
///
/// 2 TİP SORUN:
/// 1. Müşteri Sorunları (IsCustomerIssue = true)
///    - Satış sonrası ürün arızası
///    - Garanti kapsamında servis
///    - Müşteri bilgisi ve ürün bilgisi VAR
///
/// 2. Sistem Sorunları (IsCustomerIssue = false)
///    - Windows uygulaması hatası
///    - Network bağlantı sorunu
///    - Donanım arızası
///    - Müşteri bilgisi YOK
///
/// ÖNEMLİ ÖZELLİKLER:
/// - Öncelik seviyesi (1-4): Kritik sorunlar önce çözülür
/// - SLA takibi: Çözüm süresi hesaplanır
/// - Atama sistemi: ReportedBy ve AssignedTo farklı kişiler
/// - Durum takibi: Acik → Islemde → Tamamlandi/Cozulemedi
/// </summary>

/// <summary>
/// TechnicalService Read DTO - GET /api/technical-services/{id} endpoint'inden döner
/// </summary>
public class TechnicalServiceDto
{
    public int Id { get; set; }

    public string ServiceNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsCustomerIssue { get; set; }

    public TechnicalServiceStatus Status { get; set; }

    /// <summary>
    /// Öncelik seviyesi (1-4)
    /// 1: Düşük, 2: Orta, 3: Yüksek, 4: Kritik
    /// Frontend'de renkli badge olarak gösterilir
    /// </summary>
    public int Priority { get; set; }

    public DateTime ReportedDate { get; set; }

    public DateTime? ResolvedDate { get; set; }

    public string? Resolution { get; set; }

    public decimal? Cost { get; set; }

    // Foreign Keys
    public int StoreId { get; set; }
    public int ReportedByEmployeeId { get; set; }
    public int? AssignedToEmployeeId { get; set; }
    public int? CustomerId { get; set; }
    public int? ProductId { get; set; }

    // Ek bilgiler (JOIN ile getirilir)
    public string StoreName { get; set; } = string.Empty;
    public string ReportedByEmployeeName { get; set; } = string.Empty;
    public string? AssignedToEmployeeName { get; set; }
    public string? CustomerName { get; set; }
    public string? ProductName { get; set; }

    /// <summary>
    /// Calculated property - Çözüm süresi (saat olarak)
    /// SLA kontrolü için kullanılır
    /// Hedef: 24 saat içinde çözüm
    /// </summary>
    public double? ResolutionTimeInHours { get; set; }

    /// <summary>
    /// Calculated property - SLA durumu
    /// Frontend'de gösterilir:
    /// - "SLA İçinde" (yeşil) - 24 saatten az
    /// - "SLA Riski" (turuncu) - 20-24 saat arası
    /// - "SLA İhlali" (kırmızı) - 24 saatten fazla
    /// </summary>
    public string SlaStatus
    {
        get
        {
            if (Status == TechnicalServiceStatus.Tamamlandi || Status == TechnicalServiceStatus.Cozulemedi)
            {
                if (ResolutionTimeInHours.HasValue)
                {
                    if (ResolutionTimeInHours.Value <= 24)
                        return "SLA İçinde";
                    else
                        return "SLA İhlali";
                }
            }
            else
            {
                var hoursSinceReported = (DateTime.Now - ReportedDate).TotalHours;
                if (hoursSinceReported < 20)
                    return "SLA İçinde";
                else if (hoursSinceReported < 24)
                    return "SLA Riski";
                else
                    return "SLA İhlali";
            }
            return "Belirsiz";
        }
    }

    /// <summary>
    /// Calculated property - Öncelik metni
    /// </summary>
    public string PriorityText => Priority switch
    {
        1 => "Düşük",
        2 => "Orta",
        3 => "Yüksek",
        4 => "Kritik",
        _ => "Belirsiz"
    };

    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// TechnicalService Create DTO - POST /api/technical-services endpoint'ine gönderilir
///
/// İŞ AKIŞI:
/// 1. Çalışan Windows uygulamasından "Sorun Bildir" butonuna tıklar
/// 2. Form açılır: Başlık, Açıklama, Öncelik seçer
/// 3. IsCustomerIssue checkbox'ı işaretler (müşteri sorunu ise)
/// 4. Müşteri sorunu ise Customer ve Product seçer
/// 5. Kaydedince Özgün Kablocu'ya bildirim gider
/// 6. Özgün Kablocu sorunu kendine veya ekibine atar (AssignedTo)
/// </summary>
public class CreateTechnicalServiceDto
{
    // ServiceNumber otomatik oluşturulur (TS-2024-00001)

    [Required(ErrorMessage = "Başlık zorunludur")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Açıklama zorunludur")]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mağaza seçimi zorunludur")]
    public int StoreId { get; set; }

    /// <summary>
    /// Sorunu bildiren çalışan
    /// Windows uygulamasında otomatik dolar (giriş yapan kullanıcı)
    /// </summary>
    [Required(ErrorMessage = "Bildiren çalışan zorunludur")]
    public int ReportedByEmployeeId { get; set; }

    /// <summary>
    /// Sorunu çözecek teknik servis elemanı (opsiyonel)
    /// İlk oluşturmada null olabilir
    /// Özgün Kablocu sonradan atar
    /// </summary>
    public int? AssignedToEmployeeId { get; set; }

    [Required(ErrorMessage = "Sorun tipi belirtilmelidir")]
    public bool IsCustomerIssue { get; set; }

    /// <summary>
    /// Müşteri seçimi (sadece IsCustomerIssue = true için zorunlu)
    /// Frontend'de IsCustomerIssue checkbox'ı işaretliyse zorunlu hale gelir
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Ürün seçimi (opsiyonel)
    /// Müşteri sorunlarında hangi ürün arızalı
    /// Sistem sorunlarında hangi donanım/yazılım
    /// </summary>
    public int? ProductId { get; set; }

    // Status otomatik "Acik" olarak oluşturulur

    [Required(ErrorMessage = "Öncelik seviyesi zorunludur")]
    [Range(1, 4, ErrorMessage = "Öncelik 1-4 arasında olmalıdır")]
    public int Priority { get; set; } = 2;

    // ReportedDate otomatik DateTime.Now
}

/// <summary>
/// TechnicalService Update DTO - PUT /api/technical-services/{id} endpoint'ine gönderilir
/// Özgün Kablocu sorunu günceller
///
/// GÜNCELLEME SENARYOLARI:
/// 1. Atama: AssignedToEmployeeId değişir, Status = Islemde olur
/// 2. Çözüm: Resolution yazılır, ResolvedDate set edilir, Status = Tamamlandi
/// 3. Çözülemedi: Resolution yazılır (neden çözülemedi), Status = Cozulemedi
/// 4. Öncelik değişimi: Priority artırılır/azaltılır
/// </summary>
public class UpdateTechnicalServiceDto
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Açıklama zorunludur")]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    // StoreId değiştirilemez - sorun nerede bildirildiyse orada
    // ReportedByEmployeeId değiştirilemez - kim bildirdiyse o
    // IsCustomerIssue değiştirilemez - sorun tipi değişmez
    // CustomerId değiştirilemez - hangi müşteriye aitse ona ait
    // ProductId değiştirilemez - hangi ürüne aitse ona ait

    /// <summary>
    /// Atama yapılabilir
    /// Özgün Kablocu kendine veya ekibine atar
    /// </summary>
    public int? AssignedToEmployeeId { get; set; }

    [Required(ErrorMessage = "Durum zorunludur")]
    public TechnicalServiceStatus Status { get; set; }

    [Required(ErrorMessage = "Öncelik seviyesi zorunludur")]
    [Range(1, 4, ErrorMessage = "Öncelik 1-4 arasında olmalıdır")]
    public int Priority { get; set; }

    /// <summary>
    /// Çözüm açıklaması
    /// Status = Tamamlandi veya Cozulemedi ise zorunlu
    /// Frontend'de validation yapılır
    /// </summary>
    [StringLength(2000)]
    public string? Resolution { get; set; }

    /// <summary>
    /// Çözüm tarihi
    /// Status = Tamamlandi veya Cozulemedi ise otomatik set edilir
    /// Null ise henüz çözülmemiş
    /// </summary>
    public DateTime? ResolvedDate { get; set; }

    /// <summary>
    /// Maliyet
    /// Değişen parça varsa maliyeti
    /// Garanti kapsamındaysa 0
    /// </summary>
    [Range(0, 1000000, ErrorMessage = "Maliyet geçerli bir değer olmalıdır")]
    public decimal? Cost { get; set; }
}

/// <summary>
/// TechnicalService Summary DTO - Liste ve dashboard için hafif DTO
/// GET /api/technical-services endpoint'inden döner
///
/// KULLANIM:
/// - Özgün Kablocu: Açık sorunlar listesi
/// - Haluk Bey: SLA ihlali olan sorunlar
/// - Dashboard: Kritik öncelikli sorunlar
/// </summary>
public class TechnicalServiceSummaryDto
{
    public int Id { get; set; }
    public string ServiceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public TechnicalServiceStatus Status { get; set; }
    public int Priority { get; set; }
    public string PriorityText { get; set; } = string.Empty;
    public string SlaStatus { get; set; } = string.Empty;
    public bool IsCustomerIssue { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string? AssignedToEmployeeName { get; set; }
    public DateTime ReportedDate { get; set; }
}
