using TeknoRoma.Business.DTOs;

namespace TeknoRoma.Business.Services.Abstract;

/// <summary>
/// Store Service Interface - Mağaza işlemleri için
///
/// SORUMLULUKLAR:
/// 1. CRUD İşlemleri (Create, Read, Update, Delete)
/// 2. Mağaza performans raporları
/// 3. Çalışan kapasitesi kontrolü
/// 4. Mağaza bazlı filtreleme
///
/// NEDEN Repository Değil Service?
/// - Repository: Sadece veri erişimi (GetById, Add, Update, Delete)
/// - Service: Business logic (kapasite kontrolü, performans hesaplama)
///
/// ÖRNEK:
/// Repository: GetById(5) → Store entity döner
/// Service: GetStoreWithEmployees(5) → StoreDto + çalışan sayısı + kapasite oranı döner
/// </summary>
public interface IStoreService
{
    // ====== CRUD OPERATIONS ======

    /// <summary>
    /// Tüm mağazaları getirir
    /// </summary>
    /// <param name="includeInactive">Pasif mağazalar da dahil edilsin mi?</param>
    /// <returns>Mağaza listesi</returns>
    Task<IEnumerable<StoreDto>> GetAllStoresAsync(bool includeInactive = false);

    /// <summary>
    /// ID'ye göre mağaza getirir
    /// </summary>
    /// <param name="id">Mağaza ID</param>
    /// <returns>Mağaza bilgileri veya null</returns>
    Task<StoreDto?> GetStoreByIdAsync(int id);

    /// <summary>
    /// Mağaza kodu ile mağaza getirir
    /// NEDEN? Haluk Bey: "IST-001 mağazasının bilgilerini görmek istiyorum"
    /// </summary>
    /// <param name="storeCode">Mağaza kodu (örn: IST-001)</param>
    /// <returns>Mağaza bilgileri veya null</returns>
    Task<StoreDto?> GetStoreByCodeAsync(string storeCode);

    /// <summary>
    /// Yeni mağaza oluşturur
    /// </summary>
    /// <param name="createStoreDto">Oluşturulacak mağaza bilgileri</param>
    /// <returns>Oluşturulan mağaza bilgileri</returns>
    Task<StoreDto?> CreateStoreAsync(CreateStoreDto createStoreDto);

    /// <summary>
    /// Mağaza bilgilerini günceller
    /// </summary>
    /// <param name="updateStoreDto">Güncellenecek mağaza bilgileri</param>
    /// <returns>Güncellenen mağaza bilgileri veya null</returns>
    Task<StoreDto?> UpdateStoreAsync(UpdateStoreDto updateStoreDto);

    /// <summary>
    /// Mağazayı siler (Soft Delete)
    /// NEDEN Soft Delete? Mağaza kapansa bile geçmiş satış kayıtları mağazaya referans ediyor
    /// </summary>
    /// <param name="id">Silinecek mağaza ID</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> DeleteStoreAsync(int id);


    // ====== BUSINESS LOGIC METHODS ======

    /// <summary>
    /// Şehre göre mağazaları getirir
    /// Haluk Bey: "İstanbul'daki mağazalarımı görmek istiyorum"
    /// </summary>
    /// <param name="city">Şehir adı</param>
    /// <returns>Şehirdeki mağazalar</returns>
    Task<IEnumerable<StoreDto>> GetStoresByCityAsync(string city);

    /// <summary>
    /// Mağazanın çalışan kapasitesi kontrolü
    /// KULLANIM: Yeni çalışan işe alınırken kontrol edilir
    /// </summary>
    /// <param name="storeId">Mağaza ID</param>
    /// <returns>true: Kapasite var, false: Kapasite dolu</returns>
    Task<bool> HasEmployeeCapacityAsync(int storeId);

    /// <summary>
    /// Mağazanın mevcut çalışan sayısını getirir
    /// </summary>
    /// <param name="storeId">Mağaza ID</param>
    /// <returns>Çalışan sayısı</returns>
    Task<int> GetEmployeeCountAsync(int storeId);

    /// <summary>
    /// Mağaza performans raporu
    /// Haluk Bey'in istediği özel rapor
    ///
    /// İÇERİK:
    /// - Toplam satış tutarı
    /// - Toplam gider tutarı
    /// - Net kar
    /// - Çalışan sayısı
    /// - Çalışan başına satış
    /// - Metrekare başına satış
    /// </summary>
    /// <param name="storeId">Mağaza ID</param>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <returns>Performans raporu</returns>
    Task<object> GetStorePerformanceReportAsync(int storeId, DateTime startDate, DateTime endDate);
}
