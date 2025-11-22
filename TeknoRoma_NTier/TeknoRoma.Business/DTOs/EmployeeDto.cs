using System.ComponentModel.DataAnnotations;
using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Business.DTOs;

/// <summary>
/// Employee DTO - Çalışan bilgilerini API'de taşımak için
///
/// GÜVENLİK KRİTİK!
/// - Salary (Maaş) bilgisi sadece yetkili kişiler görebilir
/// - EmployeeDto: Maaş bilgisi YOK (genel kullanım için)
/// - EmployeeDetailDto: Maaş bilgisi VAR (sadece Muhasebe ve Yönetici için)
///
/// NEDEN?
/// - 258 çalışan var, herkes herkesin maaşını görmemeli
/// - Feyza Paragöz (Muhasebe) ve Haluk Bey maaş bilgisini görebilir
/// - Gül Satar kendi maaşını görebilir ama başkasınınkini göremez
/// </summary>

/// <summary>
/// Employee Read DTO - GET /api/employees/{id} endpoint'inden döner
/// GENEL KULLANIM İÇİN - Maaş bilgisi YOK
/// </summary>
public class EmployeeDto
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string IdentityNumber { get; set; } = string.Empty;

    public DateTime BirthDate { get; set; }

    public Gender Gender { get; set; }

    public UserRole Role { get; set; }

    public DateTime HireDate { get; set; }

    public bool IsActive { get; set; }

    // Foreign Keys
    public int StoreId { get; set; }
    public int DepartmentId { get; set; }

    // Ek bilgiler (JOIN ile getirilir)
    public string StoreName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;

    /// <summary>
    /// Calculated property - Kaç yaşında?
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// Calculated property - Kaç yıldır çalışıyor?
    /// </summary>
    public int YearsOfService { get; set; }

    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// Employee Detail DTO - Maaş bilgisi de VAR
/// SADECE YETKİLİ KİŞİLER İÇİN (Muhasebe, Yönetici, Kendisi)
///
/// KULLANIM:
/// - Feyza Paragöz (Muhasebe): Tüm çalışanların maaşını görebilir
/// - Haluk Bey (Yönetici): Tüm çalışanların maaşını görebilir
/// - Gül Satar: Sadece kendi maaşını görebilir
/// - Diğerleri: Maaş bilgisine erişemez (403 Forbidden)
///
/// API:
/// GET /api/employees/{id}/details
/// [Authorize(Roles = "SubeYoneticisi,Muhasebe")]
/// </summary>
public class EmployeeDetailDto : EmployeeDto
{
    /// <summary>
    /// Maaş bilgisi (HASSAS!)
    /// Sadece yetkili kişiler görebilir
    /// </summary>
    public decimal Salary { get; set; }

    /// <summary>
    /// Satış kotası (sadece KasaSatis için)
    /// Gül Satar: "Bu ay 50,000 TL satış kotam var"
    /// </summary>
    public decimal? SalesQuota { get; set; }

    /// <summary>
    /// Acil durum kişisi
    /// HR (İnsan Kaynakları) için gerekli
    /// </summary>
    public string? EmergencyContact { get; set; }

    /// <summary>
    /// Acil durum telefonu
    /// </summary>
    public string? EmergencyPhone { get; set; }
}

/// <summary>
/// Employee Create DTO - POST /api/employees endpoint'ine gönderilir
/// Yeni çalışan işe alınırken kullanılır
///
/// NEDEN IdentityUserId YOK?
/// - IdentityUserId ASP.NET Identity'den gelir
/// - Önce kullanıcı hesabı oluşturulur (Register)
/// - Sonra Employee kaydı oluşturulur ve IdentityUserId bağlanır
/// - Bu iki aşamalı process Service layer'da yönetilir
/// </summary>
public class CreateEmployeeDto
{
    [Required(ErrorMessage = "Ad zorunludur")]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad zorunludur")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "TC Kimlik No zorunludur")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "TC Kimlik No sadece rakamlardan oluşmalıdır")]
    public string IdentityNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Doğum tarihi zorunludur")]
    public DateTime BirthDate { get; set; }

    [Required(ErrorMessage = "Cinsiyet zorunludur")]
    public Gender Gender { get; set; }

    [Required(ErrorMessage = "Rol zorunludur")]
    public UserRole Role { get; set; }

    [Required(ErrorMessage = "İşe giriş tarihi zorunludur")]
    public DateTime HireDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Maaş zorunludur")]
    [Range(17002, 1000000, ErrorMessage = "Maaş asgari ücretin (17,002 TL) üzerinde olmalıdır")]
    public decimal Salary { get; set; }

    /// <summary>
    /// Satış kotası (sadece KasaSatis için zorunlu)
    /// Frontend'de Role = KasaSatis seçilirse bu alan zorunlu hale gelir
    /// </summary>
    [Range(0, 10000000, ErrorMessage = "Satış kotası geçerli bir değer olmalıdır")]
    public decimal? SalesQuota { get; set; }

    [StringLength(100)]
    public string? EmergencyContact { get; set; }

    [Phone]
    [StringLength(20)]
    public string? EmergencyPhone { get; set; }

    [Required(ErrorMessage = "Mağaza seçimi zorunludur")]
    public int StoreId { get; set; }

    [Required(ErrorMessage = "Departman seçimi zorunludur")]
    public int DepartmentId { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Employee Update DTO - PUT /api/employees/{id} endpoint'ine gönderilir
///
/// NEDEN IdentityNumber Değiştirilemez?
/// - TC Kimlik No değişmez, yanlış girildiyse yeni kayıt açılır
/// - Güvenlik: Kimlik değişimi fraud risk'i oluşturur
///
/// NEDEN Role Değiştirilebilir?
/// - Çalışan terfi edebilir: KasaSatis → SubeYoneticisi
/// - Departman değişikliği olabilir: Depo → TeknikServis
/// </summary>
public class UpdateEmployeeDto
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Ad zorunludur")]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad zorunludur")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
    [StringLength(20)]
    public string? Phone { get; set; }

    // IdentityNumber değiştirilemez - güvenlik

    // BirthDate değiştirilemez - demografik veri

    [Required(ErrorMessage = "Cinsiyet zorunludur")]
    public Gender Gender { get; set; }

    [Required(ErrorMessage = "Rol zorunludur")]
    public UserRole Role { get; set; }

    // HireDate değiştirilemez - tarihi kayıt

    [Required(ErrorMessage = "Maaş zorunludur")]
    [Range(17002, 1000000, ErrorMessage = "Maaş asgari ücretin (17,002 TL) üzerinde olmalıdır")]
    public decimal Salary { get; set; }

    [Range(0, 10000000, ErrorMessage = "Satış kotası geçerli bir değer olmalıdır")]
    public decimal? SalesQuota { get; set; }

    [StringLength(100)]
    public string? EmergencyContact { get; set; }

    [Phone]
    [StringLength(20)]
    public string? EmergencyPhone { get; set; }

    [Required(ErrorMessage = "Mağaza seçimi zorunludur")]
    public int StoreId { get; set; }

    [Required(ErrorMessage = "Departman seçimi zorunludur")]
    public int DepartmentId { get; set; }

    public bool IsActive { get; set; }
}

/// <summary>
/// Employee Summary DTO - Liste görünümü için hafif DTO
/// GET /api/employees endpoint'inden döner
///
/// NEDEN?
/// - 258 çalışan var, liste ağır gelmesin
/// - Sadece gerekli alanlar: Ad, Rol, Mağaza, Departman
/// - Detay için ayrı endpoint: GET /api/employees/{id}
/// </summary>
public class EmployeeSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
