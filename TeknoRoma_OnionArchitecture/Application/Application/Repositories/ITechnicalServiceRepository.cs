using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories
{
    /// <summary>
    /// Technical Service Repository Interface
    /// Teknik servis kayıtları için özel metodlar
    ///
    /// AMAÇ:
    /// - Özgün Kablocu'nun yönettiği teknik destek taleplerini takip eder
    /// - Müşteri sorunları ve şube içi teknik sorunları yönetir
    /// - SLA (Service Level Agreement) takibi sağlar
    ///
    /// 2 TİP SORUN:
    /// 1. Müşteri sorunları (Satış sonrası ürün arızası, teknik destek)
    /// 2. Sistem sorunları (Yazılım hatası, network sorunu, donanım)
    ///
    /// KULLANIM ALANLARI:
    /// - Sorun bildirimi ve takibi
    /// - Teknik servis performans raporları
    /// - Müşteri memnuniyeti analizi
    /// </summary>
    public interface ITechnicalServiceRepository : IRepository<TechnicalService>
    {
        /// <summary>
        /// Servis numarasına göre kayıt bulur
        /// NEDEN?
        /// - Özgün Kablocu: "Servis numarasıyla sorunu hızlıca bulabilmeliyim"
        /// - Unique field olduğu için özel metod
        /// </summary>
        /// <param name="serviceNumber">Servis numarası (TS-2024-00001)</param>
        Task<TechnicalService> GetByServiceNumberAsync(string serviceNumber);

        /// <summary>
        /// Duruma göre servis kayıtlarını getirir
        /// NEDEN?
        /// - Özgün Kablocu: "Açık durumdaki sorunları listelemem lazım"
        /// - Dashboard için bekleyen işler
        /// </summary>
        /// <param name="status">Servis durumu</param>
        Task<IReadOnlyList<TechnicalService>> GetByStatusAsync(TechnicalServiceStatus status);

        /// <summary>
        /// Mağazaya göre servis kayıtlarını getirir
        /// NEDEN?
        /// - Şube müdürü kendi şubesinin sorunlarını görmek ister
        /// - Mağaza bazlı sorun analizi için
        /// </summary>
        /// <param name="storeId">Mağaza ID</param>
        Task<IReadOnlyList<TechnicalService>> GetByStoreAsync(int storeId);

        /// <summary>
        /// Atanmış çalışana göre servis kayıtlarını getirir
        /// NEDEN?
        /// - Özgün Kablocu: "Bana atanan sorunları görmem lazım"
        /// - Teknik personel iş listesi
        /// </summary>
        /// <param name="employeeId">Çalışan ID</param>
        Task<IReadOnlyList<TechnicalService>> GetByAssignedEmployeeAsync(int employeeId);

        /// <summary>
        /// Bildiren çalışana göre servis kayıtlarını getirir
        /// NEDEN?
        /// - "Bildirdiğim sorunların durumu ne?"
        /// - Sorun takibi için
        /// </summary>
        /// <param name="employeeId">Çalışan ID</param>
        Task<IReadOnlyList<TechnicalService>> GetByReportedEmployeeAsync(int employeeId);

        /// <summary>
        /// Müşteriye göre servis kayıtlarını getirir
        /// NEDEN?
        /// - Müşteri sorun geçmişi için
        /// - "Bu müşterinin kaç kez sorunu olmuş?"
        /// </summary>
        /// <param name="customerId">Müşteri ID</param>
        Task<IReadOnlyList<TechnicalService>> GetByCustomerAsync(int customerId);

        /// <summary>
        /// Müşteri sorunlarını getirir (IsCustomerIssue = true)
        /// NEDEN?
        /// - Satış sonrası destek raporları için
        /// - Ürün kalite analizi için
        /// </summary>
        Task<IReadOnlyList<TechnicalService>> GetCustomerIssuesAsync();

        /// <summary>
        /// Sistem sorunlarını getirir (IsCustomerIssue = false)
        /// NEDEN?
        /// - IT altyapı sorunlarının analizi için
        /// - Şube teknik sorunları raporu
        /// </summary>
        Task<IReadOnlyList<TechnicalService>> GetSystemIssuesAsync();

        /// <summary>
        /// Önceliğe göre servis kayıtlarını getirir
        /// NEDEN?
        /// - Kritik sorunlara öncelik vermek için
        /// - "Öncelik 4 olan sorunları hemen göster"
        /// </summary>
        /// <param name="priority">Öncelik seviyesi (1-4)</param>
        Task<IReadOnlyList<TechnicalService>> GetByPriorityAsync(int priority);

        /// <summary>
        /// Tarih aralığındaki servis kayıtlarını getirir
        /// NEDEN?
        /// - Dönemsel rapor için
        /// - "Bu ay kaç sorun bildirilmiş?"
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        Task<IReadOnlyList<TechnicalService>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Açık ve atanmamış sorunları getirir
        /// NEDEN?
        /// - Atama bekleyen sorunları listelemek için
        /// - Özgün Kablocu'nun iş dağılımı için
        /// </summary>
        Task<IReadOnlyList<TechnicalService>> GetUnassignedAsync();

        /// <summary>
        /// Kritik öncelikli ve açık durumda olan sorunları getirir
        /// NEDEN?
        /// - Dashboard'da acil uyarılar için
        /// - Hemen müdahale gereken sorunlar
        /// </summary>
        Task<IReadOnlyList<TechnicalService>> GetCriticalOpenIssuesAsync();

        /// <summary>
        /// Belirli süreyi aşmış çözülmemiş sorunları getirir
        /// NEDEN?
        /// - SLA ihlali takibi için
        /// - "48 saatten fazla açık kalan sorunlar"
        /// </summary>
        /// <param name="hours">Saat cinsinden süre</param>
        Task<IReadOnlyList<TechnicalService>> GetOverdueIssuesAsync(int hours);

        /// <summary>
        /// Servis kaydını ilişkili verilerle getirir (Eager Loading)
        /// NEDEN?
        /// - Detay sayfasında tüm bilgileri göstermek için
        /// - Store, Employee, Customer bilgileri dahil
        /// </summary>
        /// <param name="serviceId">Servis ID</param>
        Task<TechnicalService> GetWithDetailsAsync(int serviceId);

        /// <summary>
        /// Ortalama çözüm süresini hesaplar
        /// NEDEN?
        /// - SLA performans ölçümü için
        /// - "Ortalama kaç saatte sorun çözülüyor?"
        /// </summary>
        /// <param name="startDate">Opsiyonel: Başlangıç tarihi</param>
        /// <param name="endDate">Opsiyonel: Bitiş tarihi</param>
        Task<double> GetAverageResolutionTimeAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Yeni servis numarası oluşturur
        /// Format: TS-2024-00001, TS-2024-00002
        /// </summary>
        Task<string> GenerateServiceNumberAsync();

        /// <summary>
        /// Durum bazlı istatistikleri getirir
        /// NEDEN?
        /// - Dashboard için özet bilgiler
        /// - Açık/İşlemde/Tamamlandı sayıları
        /// </summary>
        Task<Dictionary<TechnicalServiceStatus, int>> GetStatusStatisticsAsync();
    }
}
