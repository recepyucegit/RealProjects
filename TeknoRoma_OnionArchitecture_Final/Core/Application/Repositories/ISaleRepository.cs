// ============================================================================
// ISaleRepository.cs - Satış Repository Interface
// ============================================================================
// AÇIKLAMA:
// Satış entity'sine özgü veri erişim metodlarını tanımlar.
// Raporlama, analiz ve fiş numarası oluşturma için kritik metodlar içerir.
//
// İŞ SENARYOLARI:
// - Günlük/aylık ciro raporları
// - Müşteri satış geçmişi
// - Çalışan performans analizi
// - Mağaza bazlı karşılaştırma
// ============================================================================

using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories
{
    /// <summary>
    /// Satış Repository Interface
    ///
    /// RAPORLAMA MERKEZİ:
    /// En çok kullanılan repository - dashboard ve raporların kaynağı
    /// </summary>
    public interface ISaleRepository : IRepository<Sale>
    {
        /// <summary>
        /// Fiş Numarası ile Satış Getir
        ///
        /// Müşteri sorgulaması, iade işlemleri
        /// "S-2024-00001" formatında benzersiz numara
        /// </summary>
        Task<Sale?> GetBySaleNumberAsync(string saleNumber);

        /// <summary>
        /// Müşterinin Satışları
        ///
        /// Müşteri profili, satın alma geçmişi
        /// </summary>
        Task<IReadOnlyList<Sale>> GetByCustomerAsync(int customerId);

        /// <summary>
        /// Çalışanın Satışları
        ///
        /// Performans değerlendirme, prim hesaplama
        /// </summary>
        Task<IReadOnlyList<Sale>> GetByEmployeeAsync(int employeeId);

        /// <summary>
        /// Mağazanın Satışları
        ///
        /// Şube bazlı raporlama, karşılaştırma
        /// </summary>
        Task<IReadOnlyList<Sale>> GetByStoreAsync(int storeId);

        /// <summary>
        /// Duruma Göre Satışlar
        ///
        /// Bekleyen siparişler, tamamlanan satışlar
        /// </summary>
        Task<IReadOnlyList<Sale>> GetByStatusAsync(SaleStatus status);

        /// <summary>
        /// Tarih Aralığına Göre Satışlar
        ///
        /// Dönemsel raporlar, trend analizi
        ///
        /// ÖRNEK:
        /// <code>
        /// var ocakSatislari = await _sales.GetByDateRangeAsync(
        ///     new DateTime(2024, 1, 1),
        ///     new DateTime(2024, 1, 31));
        /// </code>
        /// </summary>
        Task<IReadOnlyList<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Günlük Toplam Ciro
        ///
        /// Dashboard "Bugünün cirosu" widget'ı
        ///
        /// PARAMETRE storeId:
        /// - null: Tüm mağazalar
        /// - değer: Belirli mağaza
        ///
        /// ÖRNEK:
        /// <code>
        /// var todayTotal = await _sales.GetDailyTotalAsync(DateTime.Today);
        /// var storeTotal = await _sales.GetDailyTotalAsync(DateTime.Today, storeId: 1);
        /// </code>
        /// </summary>
        Task<decimal> GetDailyTotalAsync(DateTime date, int? storeId = null);

        /// <summary>
        /// Aylık Toplam Ciro
        ///
        /// Aylık raporlar, bütçe karşılaştırma
        /// </summary>
        Task<decimal> GetMonthlyTotalAsync(int year, int month, int? storeId = null);

        /// <summary>
        /// Yeni Fiş Numarası Oluştur
        ///
        /// FORMAT: "S-YYYY-NNNNN"
        /// Örnek: "S-2024-00001"
        ///
        /// OTOMATİK SEQUENCE:
        /// - Yıl değişince numara sıfırlanır
        /// - Thread-safe olmalı (concurrent access)
        /// </summary>
        Task<string> GenerateSaleNumberAsync();
    }
}
