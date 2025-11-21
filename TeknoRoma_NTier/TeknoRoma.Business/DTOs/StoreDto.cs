using System.ComponentModel.DataAnnotations;

namespace TeknoRoma.Business.DTOs;

/// <summary>
/// Store DTO - Mağaza bilgilerini API'de taşımak için
///
/// NEDEN 3 Farklı DTO?
/// 1. StoreDto: GET isteklerinde döner (Read)
/// 2. CreateStoreDto: POST isteklerinde gelir (Create)
/// 3. UpdateStoreDto: PUT isteklerinde gelir (Update)
///
/// NEDEN Entity Yerine DTO?
/// - Güvenlik: Navigation property'leri gizleriz (gereksiz veri sızıntısı önlenir)
/// - Performans: Sadece gerekli alanlar transfer edilir
/// - Validasyon: DataAnnotations ile gelen veriyi kontrol ederiz
/// </summary>

/// <summary>
/// Store Read DTO - GET /api/stores/{id} endpoint'inden döner
/// Haluk Bey: "Mağazalarımın performansını görmek istiyorum"
/// </summary>
public class StoreDto
{
    public int Id { get; set; }

    public string StoreName { get; set; } = string.Empty;

    public string StoreCode { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string? District { get; set; }

    public string Address { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public DateTime OpeningDate { get; set; }

    public int SquareMeters { get; set; }

    public int EmployeeCapacity { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Calculated property - Kaç çalışan var?
    /// Frontend'de gösterilir: "23/30 Kapasite"
    /// </summary>
    public int CurrentEmployeeCount { get; set; }

    /// <summary>
    /// Calculated property - Kapasite doluluk oranı
    /// Frontend'de progress bar olarak gösterilir
    /// </summary>
    public decimal CapacityUtilization => EmployeeCapacity > 0
        ? Math.Round((decimal)CurrentEmployeeCount / EmployeeCapacity * 100, 2)
        : 0;
}

/// <summary>
/// Store Create DTO - POST /api/stores endpoint'ine gönderilir
/// Haluk Bey yeni mağaza açarken kullanır
///
/// NEDEN Id Yok?
/// - Id otomatik oluşturulur (Identity field)
///
/// NEDEN CreatedDate Yok?
/// - Otomatik olarak sunucuda DateTime.Now atanır
/// </summary>
public class CreateStoreDto
{
    [Required(ErrorMessage = "Mağaza adı zorunludur")]
    [StringLength(100, ErrorMessage = "Mağaza adı en fazla 100 karakter olabilir")]
    public string StoreName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mağaza kodu zorunludur")]
    [StringLength(20, ErrorMessage = "Mağaza kodu en fazla 20 karakter olabilir")]
    public string StoreCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şehir zorunludur")]
    [StringLength(50)]
    public string City { get; set; } = string.Empty;

    [StringLength(50)]
    public string? District { get; set; }

    [Required(ErrorMessage = "Adres zorunludur")]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Açılış tarihi zorunludur")]
    public DateTime OpeningDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Metrekare bilgisi zorunludur")]
    [Range(50, 10000, ErrorMessage = "Metrekare 50 ile 10000 arasında olmalıdır")]
    public int SquareMeters { get; set; }

    [Required(ErrorMessage = "Çalışan kapasitesi zorunludur")]
    [Range(5, 100, ErrorMessage = "Çalışan kapasitesi 5 ile 100 arasında olmalıdır")]
    public int EmployeeCapacity { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Store Update DTO - PUT /api/stores/{id} endpoint'ine gönderilir
/// Haluk Bey mağaza bilgilerini güncellerken kullanır
///
/// NEDEN CreateStoreDto'dan Farklı?
/// - Update'te bazı alanlar değiştirilemez (StoreCode, OpeningDate)
/// - Id gereklidir (hangi mağaza güncellenecek?)
/// </summary>
public class UpdateStoreDto
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Mağaza adı zorunludur")]
    [StringLength(100, ErrorMessage = "Mağaza adı en fazla 100 karakter olabilir")]
    public string StoreName { get; set; } = string.Empty;

    // StoreCode değiştirilemez - güvenlik ve tutarlılık için

    [Required(ErrorMessage = "Şehir zorunludur")]
    [StringLength(50)]
    public string City { get; set; } = string.Empty;

    [StringLength(50)]
    public string? District { get; set; }

    [Required(ErrorMessage = "Adres zorunludur")]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
    [StringLength(20)]
    public string? Phone { get; set; }

    // OpeningDate değiştirilemez - tarihi değer

    [Required(ErrorMessage = "Metrekare bilgisi zorunludur")]
    [Range(50, 10000, ErrorMessage = "Metrekare 50 ile 10000 arasında olmalıdır")]
    public int SquareMeters { get; set; }

    [Required(ErrorMessage = "Çalışan kapasitesi zorunludur")]
    [Range(5, 100, ErrorMessage = "Çalışan kapasitesi 5 ile 100 arasında olmalıdır")]
    public int EmployeeCapacity { get; set; }

    public bool IsActive { get; set; }
}
