// ============================================================================
// ISaleDetailRepository.cs - Satış Detay Repository Interface
// ============================================================================
// AÇIKLAMA:
// Satış kalemlerine (SaleDetail) özgü veri erişim metodlarını tanımlar.
// Ürün bazlı satış analizi ve en çok satılan ürün raporları için.
//
// İŞ SENARYOLARI:
// - Fiş detaylarını görüntüleme
// - Ürün satış geçmişi
// - En çok satılan ürünler (Top 10)
// ============================================================================

using Domain.Entities;

namespace Application.Repositories
{
    /// <summary>
    /// Satış Detay Repository Interface
    ///
    /// ÜRÜN ANALİZİ: En çok satılan, trend olan ürünler
    /// </summary>
    public interface ISaleDetailRepository : IRepository<SaleDetail>
    {
        /// <summary>
        /// Bir Satışın Tüm Kalemleri
        ///
        /// Fiş detayı görüntüleme, iade işlemi
        /// Sale -> SaleDetails ilişkisi
        /// </summary>
        Task<IReadOnlyList<SaleDetail>> GetBySaleAsync(int saleId);

        /// <summary>
        /// Ürünün Satış Geçmişi
        ///
        /// Bu ürün hangi satışlarda, kaç adet satıldı?
        /// Ürün performans analizi
        /// </summary>
        Task<IReadOnlyList<SaleDetail>> GetByProductAsync(int productId);

        /// <summary>
        /// En Çok Satılan Ürünler
        ///
        /// Dashboard widget, trend analizi
        /// Satış adedi veya tutarına göre sıralı
        ///
        /// ÖRNEK:
        /// var top10 = await GetTopSellingProductsAsync(10);
        /// var thisMonthTop = await GetTopSellingProductsAsync(10,
        ///     startDate: new DateTime(2024, 1, 1),
        ///     endDate: new DateTime(2024, 1, 31));
        /// </summary>
        Task<IReadOnlyList<SaleDetail>> GetTopSellingProductsAsync(int count, DateTime? startDate = null, DateTime? endDate = null);
    }
}
