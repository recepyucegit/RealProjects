// ============================================================================
// ISupplierTransactionRepository.cs - Tedarikçi İşlem Repository Interface
// ============================================================================
// AÇIKLAMA:
// Tedarikçi alım ve ödeme işlemlerini yönetmek için metodlar.
// Cari hesap takibi, stok giriş kayıtları.
//
// İŞ SENARYOLARI:
// - Tedarikçi borç durumu
// - Aylık alım raporları
// - Ödenmemiş faturalar
// ============================================================================

using Domain.Entities;

namespace Application.Repositories
{
    /// <summary>
    /// Tedarikçi İşlem Repository Interface
    ///
    /// CARİ HESAP: Tedarikçi borç/alacak takibi
    /// STOK ENTEGRASYonu: Alım kaydı = Stok girişi
    /// </summary>
    public interface ISupplierTransactionRepository : IRepository<SupplierTransaction>
    {
        /// <summary>
        /// İşlem Numarası ile Getir
        /// "TH-2024-00001" formatında (Tedarikçi Hareketi)
        /// </summary>
        Task<SupplierTransaction?> GetByTransactionNumberAsync(string transactionNumber);

        /// <summary>
        /// Tedarikçi İşlemleri
        /// Cari hesap ekstresi
        /// Toplam borç hesaplama için
        /// </summary>
        Task<IReadOnlyList<SupplierTransaction>> GetBySupplierAsync(int supplierId);

        /// <summary>
        /// Ürün Alım Geçmişi
        /// Bu ürün hangi tedarikçilerden, ne fiyata alındı?
        /// Maliyet analizi için
        /// </summary>
        Task<IReadOnlyList<SupplierTransaction>> GetByProductAsync(int productId);

        /// <summary>
        /// Tarih Aralığına Göre İşlemler
        /// Dönemsel alım raporları
        /// </summary>
        Task<IReadOnlyList<SupplierTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Ödenmemiş İşlemler
        /// IsPaid = false olan kayıtlar
        /// Tedarikçi borç listesi, nakit akış planlaması
        /// </summary>
        Task<IReadOnlyList<SupplierTransaction>> GetUnpaidTransactionsAsync();

        /// <summary>
        /// Aylık Toplam Alım
        /// Tedarik maliyeti analizi
        /// </summary>
        Task<decimal> GetMonthlyTotalAsync(int year, int month);

        /// <summary>
        /// Yeni İşlem Numarası Oluştur
        /// FORMAT: "TH-YYYY-NNNNN"
        /// </summary>
        Task<string> GenerateTransactionNumberAsync();
    }
}
