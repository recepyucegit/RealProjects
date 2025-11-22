using System.ComponentModel.DataAnnotations;
using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Business.DTOs;

/// <summary>
/// Department DTO - Departman bilgilerini API'de taşımak için
///
/// NEDEN ÖNEMLİ?
/// - Her mağazada 30 departman var (55 mağaza x 30 = 1650 departman)
/// - Her departman bir UserRole'e sahip (yetkilendirme için)
/// - Haluk Bey: "Hangi departmanım daha verimli çalışıyor"
/// </summary>

/// <summary>
/// Department Read DTO - GET /api/departments/{id} endpoint'inden döner
/// </summary>
public class DepartmentDto
{
    public int Id { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public string DepartmentCode { get; set; } = string.Empty;

    public UserRole DepartmentType { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    // Foreign Keys için sadece Id'ler
    public int StoreId { get; set; }

    public int? ManagerEmployeeId { get; set; }

    // Ek bilgiler (JOIN ile getirilir)
    public string StoreName { get; set; } = string.Empty;

    public string? ManagerFullName { get; set; }

    /// <summary>
    /// Calculated property - Departmanda kaç çalışan var?
    /// Service layer'da hesaplanır
    /// </summary>
    public int EmployeeCount { get; set; }

    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// Department Create DTO - POST /api/departments endpoint'ine gönderilir
/// Yeni departman açılırken kullanılır
///
/// KULLANIM:
/// Haluk Bey yeni mağaza açtığında 5 temel departman otomatik oluşur:
/// - Mağaza Yönetimi (SubeYoneticisi)
/// - Kasa/Satış (KasaSatis)
/// - Depo (Depo)
/// - Muhasebe (Muhasebe)
/// - Teknik Servis (TeknikServis)
/// </summary>
public class CreateDepartmentDto
{
    [Required(ErrorMessage = "Departman adı zorunludur")]
    [StringLength(100)]
    public string DepartmentName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Departman kodu zorunludur")]
    [StringLength(20)]
    public string DepartmentCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Departman tipi zorunludur")]
    public UserRole DepartmentType { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Mağaza seçimi zorunludur")]
    public int StoreId { get; set; }

    /// <summary>
    /// Departman müdürü (isteğe bağlı)
    /// Başlangıçta null olabilir, sonradan atanır
    /// </summary>
    public int? ManagerEmployeeId { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Department Update DTO - PUT /api/departments/{id} endpoint'ine gönderilir
///
/// NEDEN DepartmentType Değiştirilemez?
/// - DepartmentType (UserRole) değişirse yetkilendirme sistemi bozulur
/// - Örnek: "Muhasebe" departmanını "TeknikServis" yapamazsınız
/// - Yeni tip istiyorsanız yeni departman açmalısınız
/// </summary>
public class UpdateDepartmentDto
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Departman adı zorunludur")]
    [StringLength(100)]
    public string DepartmentName { get; set; } = string.Empty;

    // DepartmentCode değiştirilemez - referans integrity için
    // DepartmentType değiştirilemez - yetkilendirme için

    [StringLength(500)]
    public string? Description { get; set; }

    // StoreId değiştirilemez - departman başka mağazaya taşınamaz

    /// <summary>
    /// Departman müdürü değiştirilebilir
    /// Haluk Bey: "X departmanına yeni müdür atadım"
    /// </summary>
    public int? ManagerEmployeeId { get; set; }

    public bool IsActive { get; set; }
}
