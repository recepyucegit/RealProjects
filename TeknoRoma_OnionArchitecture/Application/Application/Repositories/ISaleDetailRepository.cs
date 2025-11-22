using Domain.Entities;

namespace Application.Repositories
{
    /// <summary>
    /// Sale Detail Repository Interface
    /// Satış detay kayıtları için özel metodlar
    ///
    /// AMAÇ:
    /// - Her satışın içindeki ürün satırlarını yönetir
    /// - Sale ve Product arasındaki ilişkiyi takip eder
    /// - Raporlama için detay bazlı sorgular sağlar
    ///
    /// KULLANIM ALANLARI:
    /// - Satış detaylarının görüntülenmesi
    /// - Ürün bazlı satış raporları
    /// - Stok değişikliklerinin takibi
    /// </summary>
    public interface ISaleDetailRepository : IRepository<SaleDetail>
    {
        /// <summary>
        /// Belirli bir satışın tüm detaylarını getirir
        /// NEDEN?
        /// - Satış faturası/fişi oluşturmak için
        /// - Satış içindeki tüm ürünleri listelemek için
        /// </summary>
        /// <param name="saleId">Satış ID</param>
        /// <returns>Satış detayları listesi</returns>
        Task<IReadOnlyList<SaleDetail>> GetBySaleIdAsync(int saleId);

        /// <summary>
        /// Belirli bir ürünün satış geçmişini getirir
        /// NEDEN?
        /// - Haluk Bey'in raporu: "Bu ürün ne kadar satılmış?"
        /// - Ürün performans analizi için
        /// </summary>
        /// <param name="productId">Ürün ID</param>
        /// <returns>Ürünün satış detayları</returns>
        Task<IReadOnlyList<SaleDetail>> GetByProductIdAsync(int productId);

        /// <summary>
        /// Belirli bir ürünün tarih aralığındaki satışlarını getirir
        /// NEDEN?
        /// - Dönemsel ürün satış raporu için
        /// - "Bu ay bu üründen ne kadar sattık?"
        /// </summary>
        /// <param name="productId">Ürün ID</param>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        Task<IReadOnlyList<SaleDetail>> GetByProductAndDateRangeAsync(int productId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Satış detayını ürün bilgisiyle birlikte getirir (Eager Loading)
        /// NEDEN?
        /// - Satış detayında ürün adı, kategorisi gibi bilgileri göstermek için
        /// - N+1 Query problemini önler
        /// </summary>
        /// <param name="saleDetailId">Satış Detay ID</param>
        Task<SaleDetail> GetWithProductAsync(int saleDetailId);

        /// <summary>
        /// Toplam satış miktarını ürün bazında hesaplar
        /// NEDEN?
        /// - Ürünün toplam kaç adet satıldığını görmek için
        /// - Stok ve satış karşılaştırması için
        /// </summary>
        /// <param name="productId">Ürün ID</param>
        /// <param name="startDate">Opsiyonel: Başlangıç tarihi</param>
        /// <param name="endDate">Opsiyonel: Bitiş tarihi</param>
        Task<int> GetTotalQuantitySoldAsync(int productId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Toplam satış tutarını ürün bazında hesaplar
        /// NEDEN?
        /// - Ürünün toplam hasılatını görmek için
        /// - Ürün performans raporu için
        /// </summary>
        /// <param name="productId">Ürün ID</param>
        /// <param name="startDate">Opsiyonel: Başlangıç tarihi</param>
        /// <param name="endDate">Opsiyonel: Bitiş tarihi</param>
        Task<decimal> GetTotalRevenueByProductAsync(int productId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// En çok satılan ürünleri getirir
        /// NEDEN?
        /// - Haluk Bey'in raporu: "En çok satılan 10 ürün"
        /// - SaleDetail tablosundan group by ile hesaplanır
        /// </summary>
        /// <param name="count">Kaç ürün getirileceği</param>
        /// <param name="startDate">Opsiyonel: Başlangıç tarihi</param>
        /// <param name="endDate">Opsiyonel: Bitiş tarihi</param>
        Task<IReadOnlyList<(int ProductId, string ProductName, int TotalQuantity, decimal TotalRevenue)>> GetTopSellingProductsAsync(
            int count, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Kategoriye göre satış toplamlarını getirir
        /// NEDEN?
        /// - Kategori bazlı performans raporu için
        /// - "Hangi kategoriden ne kadar satış yaptık?"
        /// </summary>
        /// <param name="startDate">Opsiyonel: Başlangıç tarihi</param>
        /// <param name="endDate">Opsiyonel: Bitiş tarihi</param>
        Task<IReadOnlyList<(int CategoryId, string CategoryName, decimal TotalRevenue)>> GetSalesByCategoryAsync(
            DateTime? startDate = null, DateTime? endDate = null);
    }
}
