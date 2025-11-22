using TeknoRoma.Business.DTOs;
using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Business.Services.Abstract;

/// <summary>
/// Department Service Interface - Departman işlemleri için
///
/// SORUMLULUKLAR:
/// 1. CRUD İşlemleri
/// 2. Mağaza bazında departman yönetimi
/// 3. Departman tipi (UserRole) bazında filtreleme
/// 4. Departman müdürü atama
///
/// İŞ KURALLARI:
/// - Her mağazada 5 temel departman olmalı (Yönetim, Kasa, Depo, Muhasebe, Teknik Servis)
/// - DepartmanCode mağaza içinde benzersiz olmalı
/// - Departman müdürü o departmanda çalışmalı
/// </summary>
public interface IDepartmentService
{
    // ====== CRUD OPERATIONS ======

    /// <summary>
    /// Tüm departmanları getirir
    /// </summary>
    /// <param name="includeInactive">Pasif departmanlar da dahil edilsin mi?</param>
    /// <returns>Departman listesi</returns>
    Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync(bool includeInactive = false);

    /// <summary>
    /// ID'ye göre departman getirir
    /// </summary>
    /// <param name="id">Departman ID</param>
    /// <returns>Departman bilgileri veya null</returns>
    Task<DepartmentDto?> GetDepartmentByIdAsync(int id);

    /// <summary>
    /// Yeni departman oluşturur
    ///
    /// BUSINESS LOGIC:
    /// - DepartmentCode mağaza içinde benzersiz olmalı
    /// - ManagerEmployeeId verilmişse, o çalışan bu departmanda olmalı
    /// </summary>
    /// <param name="createDepartmentDto">Oluşturulacak departman bilgileri</param>
    /// <returns>Oluşturulan departman bilgileri</returns>
    Task<DepartmentDto?> CreateDepartmentAsync(CreateDepartmentDto createDepartmentDto);

    /// <summary>
    /// Departman bilgilerini günceller
    /// </summary>
    /// <param name="updateDepartmentDto">Güncellenecek departman bilgileri</param>
    /// <returns>Güncellenen departman bilgileri veya null</returns>
    Task<DepartmentDto?> UpdateDepartmentAsync(UpdateDepartmentDto updateDepartmentDto);

    /// <summary>
    /// Departmanı siler (Soft Delete)
    /// </summary>
    /// <param name="id">Silinecek departman ID</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> DeleteDepartmentAsync(int id);


    // ====== BUSINESS LOGIC METHODS ======

    /// <summary>
    /// Mağazanın tüm departmanlarını getirir
    /// KULLANIM: Haluk Bey mağaza detay sayfasında departmanları görür
    /// </summary>
    /// <param name="storeId">Mağaza ID</param>
    /// <returns>Mağazanın departmanları</returns>
    Task<IEnumerable<DepartmentDto>> GetDepartmentsByStoreAsync(int storeId);

    /// <summary>
    /// Departman tipine göre departmanları getirir
    /// KULLANIM: "Tüm mağazalardaki Muhasebe departmanları"
    /// </summary>
    /// <param name="departmentType">Departman tipi (UserRole)</param>
    /// <returns>Bu tipteki departmanlar</returns>
    Task<IEnumerable<DepartmentDto>> GetDepartmentsByTypeAsync(UserRole departmentType);

    /// <summary>
    /// Departmana müdür atar
    /// BUSINESS LOGIC:
    /// - Çalışan bu departmanda çalışmalı
    /// - Çalışanın rolü departman tipine uygun olmalı
    /// </summary>
    /// <param name="departmentId">Departman ID</param>
    /// <param name="employeeId">Müdür olacak çalışan ID</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> AssignManagerAsync(int departmentId, int employeeId);

    /// <summary>
    /// Departmanın çalışan sayısını getirir
    /// </summary>
    /// <param name="departmentId">Departman ID</param>
    /// <returns>Çalışan sayısı</returns>
    Task<int> GetEmployeeCountAsync(int departmentId);

    /// <summary>
    /// Yeni mağaza için temel departmanları otomatik oluşturur
    /// KULLANIM: Haluk Bey yeni mağaza açtığında otomatik 5 departman oluşur
    ///
    /// OLUŞTURULAN DEPARTMANLAR:
    /// 1. Mağaza Yönetimi (SubeYoneticisi)
    /// 2. Kasa/Satış (KasaSatis)
    /// 3. Depo (Depo)
    /// 4. Muhasebe (Muhasebe)
    /// 5. Teknik Servis (TeknikServis)
    /// </summary>
    /// <param name="storeId">Mağaza ID</param>
    /// <returns>Oluşturulan departman sayısı</returns>
    Task<int> CreateDefaultDepartmentsForStoreAsync(int storeId);
}
