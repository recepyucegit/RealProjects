// ============================================================================
// ITechnicalServiceRepository.cs - Teknik Servis Repository Interface
// ============================================================================
// AÇIKLAMA:
// Teknik servis taleplerini yönetmek için veri erişim metodları.
// Ticket sistemi, SLA takibi ve iş dağılımı için.
//
// İŞ SENARYOLARI:
// - Açık talep listesi (dashboard)
// - Teknisyene atanmamış talepler
// - Durum bazlı filtreleme
// ============================================================================

using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories
{
    /// <summary>
    /// Teknik Servis Repository Interface
    ///
    /// TİCKET SİSTEMİ: Sorun takibi ve çözüm yönetimi
    /// </summary>
    public interface ITechnicalServiceRepository : IRepository<TechnicalService>
    {
        /// <summary>
        /// Servis Numarası ile Getir
        /// "TS-2024-00001" formatında
        /// Müşteri sorgulaması için
        /// </summary>
        Task<TechnicalService?> GetByServiceNumberAsync(string serviceNumber);

        /// <summary>
        /// Duruma Göre Talepler
        /// Açık, İşlemde, Tamamlandı, Çözülemedi
        /// </summary>
        Task<IReadOnlyList<TechnicalService>> GetByStatusAsync(TechnicalServiceStatus status);

        /// <summary>
        /// Mağaza Talepleri
        /// Şube bazlı servis takibi
        /// </summary>
        Task<IReadOnlyList<TechnicalService>> GetByStoreAsync(int storeId);

        /// <summary>
        /// Teknisyene Atanan Talepler
        /// Çalışan iş yükü görüntüleme
        /// </summary>
        Task<IReadOnlyList<TechnicalService>> GetByAssignedEmployeeAsync(int employeeId);

        /// <summary>
        /// Müşteri Talepleri
        /// Müşteri şikayet geçmişi
        /// </summary>
        Task<IReadOnlyList<TechnicalService>> GetByCustomerAsync(int customerId);

        /// <summary>
        /// Açık Talepler
        /// Status != Tamamlandı && Status != Cozulemedi
        /// Dashboard uyarısı için
        /// </summary>
        Task<IReadOnlyList<TechnicalService>> GetOpenIssuesAsync();

        /// <summary>
        /// Atanmamış Talepler
        /// AssignedToEmployeeId = null
        /// İş dağılımı için kritik
        /// </summary>
        Task<IReadOnlyList<TechnicalService>> GetUnassignedAsync();

        /// <summary>
        /// Açık Talep Sayısı
        /// Dashboard badge/counter için
        /// Hızlı sorgu - sadece COUNT
        /// </summary>
        Task<int> GetOpenIssuesCountAsync();

        /// <summary>
        /// Yeni Servis Numarası Oluştur
        /// FORMAT: "TS-YYYY-NNNNN"
        /// </summary>
        Task<string> GenerateServiceNumberAsync();
    }
}
