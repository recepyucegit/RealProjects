using TeknoRoma.Business.DTOs;
using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Business.Services.Abstract;

/// <summary>
/// TechnicalService Service Interface - Teknik servis işlemleri
/// Özgün Kablocu'nun (Teknik Servis Temsilcisi) kullandığı servis
///
/// SORUMLULUKLAR:
/// 1. CRUD İşlemleri
/// 2. Otomatik ServiceNumber oluşturma (TS-2024-00001)
/// 3. SLA (Service Level Agreement) takibi
/// 4. Öncelik yönetimi (Priority 1-4)
/// 5. Atama sistemi (ReportedBy ve AssignedTo)
/// 6. Durum yönetimi (Acik → Islemde → Tamamlandi/Cozulemedi)
/// 7. Müşteri ve sistem sorunlarının ayrı takibi
///
/// SLA KURALLARI:
/// - Hedef çözüm süresi: 24 saat
/// - Priority = 4 (Kritik): 4 saat
/// - Priority = 3 (Yüksek): 12 saat
/// - Priority = 2 (Orta): 24 saat
/// - Priority = 1 (Düşük): 72 saat
///
/// İŞ KURALLARI:
/// - IsCustomerIssue = true ise CustomerId zorunlu
/// - Status = Tamamlandi/Cozulemedi ise Resolution zorunlu
/// - Status = Tamamlandi/Cozulemedi ise ResolvedDate otomatik set edilir
/// </summary>
public interface ITechnicalServiceService
{
    // ====== CRUD OPERATIONS ======

    /// <summary>
    /// Tüm teknik servis kayıtlarını getirir
    /// </summary>
    /// <param name="includeDeleted">Silinmiş kayıtlar da dahil edilsin mi?</param>
    /// <returns>Teknik servis kayıtları</returns>
    Task<IEnumerable<TechnicalServiceSummaryDto>> GetAllTechnicalServicesAsync(bool includeDeleted = false);

    /// <summary>
    /// ID'ye göre teknik servis kaydı getirir
    /// </summary>
    /// <param name="id">Teknik servis ID</param>
    /// <returns>Teknik servis bilgileri veya null</returns>
    Task<TechnicalServiceDto?> GetTechnicalServiceByIdAsync(int id);

    /// <summary>
    /// Servis numarasına göre getirir
    /// KULLANIM: Özgün Kablocu: "TS-2024-00042 numaralı sorunu bul"
    /// </summary>
    /// <param name="serviceNumber">Servis numarası</param>
    /// <returns>Teknik servis bilgileri veya null</returns>
    Task<TechnicalServiceDto?> GetTechnicalServiceByNumberAsync(string serviceNumber);

    /// <summary>
    /// Yeni teknik servis kaydı oluşturur
    ///
    /// OTOMATIK İŞLEMLER:
    /// 1. ServiceNumber oluşturulur: TS-{Yıl}-{5 haneli sıra}
    /// 2. ReportedDate = DateTime.Now
    /// 3. Status = Acik
    ///
    /// VALIDASYON:
    /// - IsCustomerIssue = true ise CustomerId kontrolü
    /// - Priority 1-4 arasında olmalı
    /// - ReportedByEmployeeId geçerli olmalı
    ///
    /// BİLDİRİM:
    /// - Özgün Kablocu'ya bildirim gönderilir (Email/SMS)
    /// - Priority = 4 ise acil bildirim (Push notification)
    /// </summary>
    /// <param name="createDto">Oluşturulacak teknik servis bilgileri</param>
    /// <returns>Oluşturulan teknik servis bilgileri</returns>
    Task<TechnicalServiceDto?> CreateTechnicalServiceAsync(CreateTechnicalServiceDto createDto);

    /// <summary>
    /// Teknik servis kaydını günceller
    ///
    /// ÖZEL KURALLAR:
    /// - ServiceNumber değiştirilemez
    /// - ReportedDate değiştirilemez
    /// - IsCustomerIssue değiştirilemez
    /// - Status = Tamamlandi/Cozulemedi ise ResolvedDate otomatik set edilir
    ///
    /// BİLDİRİM:
    /// - Status değişirse bildiren çalışana bildirim
    /// - AssignedTo değişirse yeni atanan kişiye bildirim
    /// </summary>
    /// <param name="updateDto">Güncellenecek teknik servis bilgileri</param>
    /// <returns>Güncellenen teknik servis bilgileri veya null</returns>
    Task<TechnicalServiceDto?> UpdateTechnicalServiceAsync(UpdateTechnicalServiceDto updateDto);

    /// <summary>
    /// Teknik servis kaydını siler (Soft Delete)
    /// NEDEN? Yanlış bildirim veya duplikasyon
    /// </summary>
    /// <param name="id">Silinecek teknik servis ID</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> DeleteTechnicalServiceAsync(int id);


    // ====== BUSINESS LOGIC METHODS ======

    /// <summary>
    /// Mağazanın teknik servis kayıtlarını getirir
    /// </summary>
    /// <param name="storeId">Mağaza ID</param>
    /// <param name="status">Durum filtresi (opsiyonel)</param>
    /// <returns>Mağazanın teknik servis kayıtları</returns>
    Task<IEnumerable<TechnicalServiceSummaryDto>> GetTechnicalServicesByStoreAsync(int storeId, TechnicalServiceStatus? status = null);

    /// <summary>
    /// Müşterinin teknik servis kayıtlarını getirir
    /// KULLANIM: Müşteri geçmişi, garanti takibi
    /// </summary>
    /// <param name="customerId">Müşteri ID</param>
    /// <returns>Müşterinin teknik servis kayıtları</returns>
    Task<IEnumerable<TechnicalServiceDto>> GetTechnicalServicesByCustomerAsync(int customerId);

    /// <summary>
    /// Ürünün teknik servis kayıtlarını getirir
    /// KULLANIM: Hangi ürünlerde daha çok arıza var?
    /// Özgün Kablocu: "X modelde sürekli Y arızası çıkıyor"
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <returns>Ürünün teknik servis kayıtları</returns>
    Task<IEnumerable<TechnicalServiceDto>> GetTechnicalServicesByProductAsync(int productId);

    /// <summary>
    /// Çalışana atanan teknik servis kayıtlarını getirir
    /// KULLANIM: Özgün Kablocu'nun iş listesi
    /// </summary>
    /// <param name="employeeId">Çalışan ID</param>
    /// <param name="status">Durum filtresi (opsiyonel)</param>
    /// <returns>Çalışana atanan kayıtlar</returns>
    Task<IEnumerable<TechnicalServiceSummaryDto>> GetTechnicalServicesAssignedToAsync(int employeeId, TechnicalServiceStatus? status = null);

    /// <summary>
    /// Açık (çözülmemiş) teknik servis kayıtlarını getirir
    /// KULLANIM: Dashboard'da "Bekleyen Sorunlar"
    /// </summary>
    /// <param name="storeId">Mağaza ID (opsiyonel)</param>
    /// <returns>Açık teknik servis kayıtları</returns>
    Task<IEnumerable<TechnicalServiceSummaryDto>> GetOpenTechnicalServicesAsync(int? storeId = null);

    /// <summary>
    /// SLA ihlali olan kayıtları getirir
    /// KULLANIM: Haluk Bey: "Hangi sorunlar 24 saati aştı?"
    ///
    /// SLA İHLALİ:
    /// - Priority = 4: 4 saatten fazla açık
    /// - Priority = 3: 12 saatten fazla açık
    /// - Priority = 2: 24 saatten fazla açık
    /// - Priority = 1: 72 saatten fazla açık
    /// </summary>
    /// <param name="storeId">Mağaza ID (opsiyonel)</param>
    /// <returns>SLA ihlali olan kayıtlar</returns>
    Task<IEnumerable<TechnicalServiceSummaryDto>> GetSlaViolationsAsync(int? storeId = null);

    /// <summary>
    /// Önceliğe göre teknik servis kayıtlarını getirir
    /// KULLANIM: "Tüm kritik sorunlar"
    /// </summary>
    /// <param name="priority">Öncelik (1-4)</param>
    /// <param name="storeId">Mağaza ID (opsiyonel)</param>
    /// <returns>Bu öncelikteki kayıtlar</returns>
    Task<IEnumerable<TechnicalServiceSummaryDto>> GetTechnicalServicesByPriorityAsync(int priority, int? storeId = null);

    /// <summary>
    /// Teknik servisi çalışana atar
    ///
    /// GÜNCELLENİR:
    /// - AssignedToEmployeeId
    /// - Status = Islemde (eğer Acik ise)
    ///
    /// BİLDİRİM:
    /// - Atanan çalışana bildirim gönderilir
    /// </summary>
    /// <param name="serviceId">Teknik servis ID</param>
    /// <param name="employeeId">Atanacak çalışan ID</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> AssignTechnicalServiceAsync(int serviceId, int employeeId);

    /// <summary>
    /// Teknik servisi çözümlü olarak işaretler
    ///
    /// GÜNCELLENİR:
    /// - Status = Tamamlandi
    /// - Resolution
    /// - ResolvedDate = DateTime.Now
    /// - Cost (opsiyonel)
    ///
    /// HESAPLAMA:
    /// - ResolutionTimeInHours hesaplanır
    /// - SLA kontrolü yapılır
    /// </summary>
    /// <param name="serviceId">Teknik servis ID</param>
    /// <param name="resolution">Çözüm açıklaması</param>
    /// <param name="cost">Maliyet (opsiyonel)</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> ResolveTechnicalServiceAsync(int serviceId, string resolution, decimal? cost = null);

    /// <summary>
    /// Teknik servisi çözülemedi olarak işaretler
    ///
    /// GÜNCELLENİR:
    /// - Status = Cozulemedi
    /// - Resolution (neden çözülemedi açıklaması)
    /// - ResolvedDate = DateTime.Now
    /// </summary>
    /// <param name="serviceId">Teknik servis ID</param>
    /// <param name="reason">Çözülememe sebebi</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> MarkAsUnresolvableAsync(int serviceId, string reason);

    /// <summary>
    /// Teknik servis performans raporu
    /// Özgün Kablocu ve Haluk Bey için
    ///
    /// İÇERİK:
    /// - Toplam sorun sayısı
    /// - Çözülen sorun sayısı
    /// - Çözülemeyen sorun sayısı
    /// - Ortalama çözüm süresi
    /// - SLA başarı oranı
    /// - En çok arıza olan ürünler
    /// - En çok sorun bildiren mağazalar
    /// - Önceliklere göre dağılım
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="storeId">Mağaza ID (opsiyonel)</param>
    /// <returns>Performans raporu</returns>
    Task<object> GetTechnicalServicePerformanceReportAsync(DateTime startDate, DateTime endDate, int? storeId = null);

    /// <summary>
    /// Çalışan teknik servis istatistikleri
    /// KULLANIM: "Özgün Kablocu bu ay kaç sorun çözdü?"
    /// </summary>
    /// <param name="employeeId">Çalışan ID</param>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <returns>Çalışan istatistikleri</returns>
    Task<object> GetEmployeeTechnicalServiceStatsAsync(int employeeId, DateTime startDate, DateTime endDate);
}
