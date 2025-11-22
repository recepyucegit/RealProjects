using TeknoRoma.Business.DTOs;
using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Business.Services.Abstract;

/// <summary>
/// Employee Service Interface - Çalışan işlemleri için
///
/// EN KOMPLEKS SERVİS!
///
/// SORUMLULUKLAR:
/// 1. CRUD İşlemleri (GÜVENLİK ÖNEMLİ!)
/// 2. ASP.NET Identity entegrasyonu (IdentityUserId bağlantısı)
/// 3. Role-Based Access Control (RBAC)
/// 4. Maaş yönetimi (sadece yetkili kişiler)
/// 5. Performans takibi (satış kotası, gerçekleşme)
/// 6. Çalışan transferi (mağaza/departman değişikliği)
///
/// GÜVENLİK KURALLARI:
/// - Maaş bilgisi sadece: Muhasebe, Yönetici, Kendisi görebilir
/// - TC Kimlik No güncellenemez (fraud riski)
/// - IdentityUserId değiştirilemez (güvenlik)
///
/// İŞ KURALLARI:
/// - Çalışan işe alınırken önce ASP.NET Identity kullanıcısı oluşturulur
/// - Sonra Employee kaydı oluşturulur ve IdentityUserId bağlanır
/// - Mağaza çalışan kapasitesi kontrolü yapılır
/// - Departman rolü ile çalışan rolü uyumlu olmalı
/// </summary>
public interface IEmployeeService
{
    // ====== CRUD OPERATIONS ======

    /// <summary>
    /// Tüm çalışanları getirir (Özet bilgi - Maaş YOK)
    /// </summary>
    /// <param name="includeInactive">İşten ayrılmış çalışanlar da dahil edilsin mi?</param>
    /// <returns>Çalışan özet listesi</returns>
    Task<IEnumerable<EmployeeSummaryDto>> GetAllEmployeesAsync(bool includeInactive = false);

    /// <summary>
    /// ID'ye göre çalışan getirir (Genel bilgi - Maaş YOK)
    /// </summary>
    /// <param name="id">Çalışan ID</param>
    /// <returns>Çalışan bilgileri veya null</returns>
    Task<EmployeeDto?> GetEmployeeByIdAsync(int id);

    /// <summary>
    /// ID'ye göre çalışan detay getirir (Maaş bilgisi VAR)
    /// GÜVENLİK: Sadece yetkili kişiler çağırabilir
    /// Controller'da [Authorize(Roles = "SubeYoneticisi,Muhasebe")] kontrolü yapılır
    /// </summary>
    /// <param name="id">Çalışan ID</param>
    /// <param name="requestingUserId">İsteği yapan kullanıcının ID'si (kendisine bakıyorsa izin verilir)</param>
    /// <returns>Çalışan detay bilgileri veya null</returns>
    Task<EmployeeDetailDto?> GetEmployeeDetailByIdAsync(int id, string? requestingUserId = null);

    /// <summary>
    /// IdentityUserId'ye göre çalışan getirir
    /// KULLANIM: Kullanıcı giriş yaptığında Employee kaydına erişim için
    /// </summary>
    /// <param name="identityUserId">ASP.NET Identity kullanıcı ID</param>
    /// <returns>Çalışan bilgileri veya null</returns>
    Task<EmployeeDto?> GetEmployeeByIdentityUserIdAsync(string identityUserId);

    /// <summary>
    /// Yeni çalışan oluşturur
    ///
    /// İKİ AŞAMALI PROCESS:
    /// 1. ASP.NET Identity kullanıcısı oluşturulur (UserManager.CreateAsync)
    /// 2. Employee kaydı oluşturulur ve IdentityUserId bağlanır
    ///
    /// BUSINESS LOGIC:
    /// - Mağaza çalışan kapasitesi kontrolü
    /// - TC Kimlik No benzersizliği kontrolü
    /// - Email benzersizliği kontrolü
    /// - Departman rolü ile çalışan rolü uyum kontrolü
    /// </summary>
    /// <param name="createEmployeeDto">Oluşturulacak çalışan bilgileri</param>
    /// <param name="password">ASP.NET Identity şifresi</param>
    /// <returns>Oluşturulan çalışan bilgileri</returns>
    Task<EmployeeDto?> CreateEmployeeAsync(CreateEmployeeDto createEmployeeDto, string password);

    /// <summary>
    /// Çalışan bilgilerini günceller
    ///
    /// DEĞİŞTİRİLEMEYENLER:
    /// - IdentityNumber (TC Kimlik No)
    /// - BirthDate
    /// - HireDate
    /// - IdentityUserId
    /// </summary>
    /// <param name="updateEmployeeDto">Güncellenecek çalışan bilgileri</param>
    /// <returns>Güncellenen çalışan bilgileri veya null</returns>
    Task<EmployeeDto?> UpdateEmployeeAsync(UpdateEmployeeDto updateEmployeeDto);

    /// <summary>
    /// Çalışanı işten çıkarır (Soft Delete)
    /// NEDEN Soft Delete? Geçmiş satış/gider kayıtları çalışana referans ediyor
    /// </summary>
    /// <param name="id">İşten çıkarılacak çalışan ID</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> TerminateEmployeeAsync(int id);


    // ====== BUSINESS LOGIC METHODS ======

    /// <summary>
    /// Mağazanın tüm çalışanlarını getirir
    /// </summary>
    /// <param name="storeId">Mağaza ID</param>
    /// <returns>Mağazanın çalışanları</returns>
    Task<IEnumerable<EmployeeSummaryDto>> GetEmployeesByStoreAsync(int storeId);

    /// <summary>
    /// Departmanın tüm çalışanlarını getirir
    /// </summary>
    /// <param name="departmentId">Departman ID</param>
    /// <returns>Departmanın çalışanları</returns>
    Task<IEnumerable<EmployeeSummaryDto>> GetEmployeesByDepartmentAsync(int departmentId);

    /// <summary>
    /// Role göre çalışanları getirir
    /// KULLANIM: "Tüm Muhasebe çalışanları"
    /// </summary>
    /// <param name="role">Rol (UserRole)</param>
    /// <returns>Bu roldeki çalışanlar</returns>
    Task<IEnumerable<EmployeeSummaryDto>> GetEmployeesByRoleAsync(UserRole role);

    /// <summary>
    /// Çalışanı başka mağazaya/departmana transfer eder
    ///
    /// BUSINESS LOGIC:
    /// - Hedef mağazanın kapasitesi kontrolü
    /// - Hedef departmanın rolü ile çalışan rolü uyumu
    /// - Transfer kayıt tablosuna log yazılır (opsiyonel)
    /// </summary>
    /// <param name="employeeId">Çalışan ID</param>
    /// <param name="newStoreId">Yeni mağaza ID</param>
    /// <param name="newDepartmentId">Yeni departman ID</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> TransferEmployeeAsync(int employeeId, int newStoreId, int newDepartmentId);

    /// <summary>
    /// Çalışan maaşını günceller
    /// GÜVENLİK: Sadece Muhasebe ve Yönetici yapabilir
    /// </summary>
    /// <param name="employeeId">Çalışan ID</param>
    /// <param name="newSalary">Yeni maaş</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> UpdateSalaryAsync(int employeeId, decimal newSalary);

    /// <summary>
    /// Çalışan satış kotasını günceller
    /// KULLANIM: Gül Satar'ın aylık satış kotası belirlenir
    /// Sadece KasaSatis rolü için geçerli
    /// </summary>
    /// <param name="employeeId">Çalışan ID</param>
    /// <param name="newQuota">Yeni satış kotası</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> UpdateSalesQuotaAsync(int employeeId, decimal newQuota);

    /// <summary>
    /// Çalışan performans raporu
    /// Haluk Bey'in istediği rapor
    ///
    /// İÇERİK:
    /// - Toplam satış tutarı
    /// - Satış kotası
    /// - Kota gerçekleşme oranı
    /// - Aylık performans grafiği
    /// - En çok sattığı ürün kategorileri
    /// </summary>
    /// <param name="employeeId">Çalışan ID</param>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <returns>Performans raporu</returns>
    Task<object> GetEmployeePerformanceReportAsync(int employeeId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Mağaza çalışan maliyet raporu
    /// Feyza Paragöz için: Mağazanın toplam maaş gideri
    /// </summary>
    /// <param name="storeId">Mağaza ID</param>
    /// <returns>Toplam maaş gideri</returns>
    Task<decimal> GetStoreSalaryCostAsync(int storeId);
}
