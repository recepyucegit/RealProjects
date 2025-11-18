using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories
{
    /// <summary>
    /// Product Repository Interface
    /// IRepository'den miras alır + özel metodlar ekler
    /// 
    /// NEDEN Özel Interface?
    /// - Generic repository her şey için yeterli değil
    /// - Product için özel iş mantığı var:
    ///   * Stok kontrolü
    ///   * Kritik stok seviyesi altındaki ürünler
    ///   * Barkod ile arama (Fahri Cepçi için)
    ///   * Kategoriye göre filtreleme
    /// </summary>
    public interface IProductRepository : IRepository<Product>
    {
        /// <summary>
        /// Barkod numarasına göre ürün bulur
        /// NEDEN Özel Metod?
        /// - Fahri Cepçi: "Barkod okutup hızlıca stok ve fiyat bilgisi almalıyım"
        /// - Unique field olduğu için özel metod gerekli
        /// </summary>
        Task<Product> GetByBarcodeAsync(string barcode);

        /// <summary>
        /// Kategoriye göre ürünleri getirir
        /// NEDEN?
        /// - Haluk Bey'in raporu: "Kategorilere göre ürünlerin listesi"
        /// - Sık kullanılan bir filtreleme
        /// </summary>
        Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId);

        /// <summary>
        /// Tedarikçiye göre ürünleri getirir
        /// </summary>
        Task<IReadOnlyList<Product>> GetBySupplierAsync(int supplierId);

        /// <summary>
        /// Stok durumuna göre ürünleri getirir
        /// NEDEN?
        /// - Gül Satar: "Kritik seviyenin altına düşen ürünler uyarı vermeli"
        /// - Kerim Zulacı: "Kritik seviyenin altına düşen ürünleri görebilmeliyim"
        /// </summary>
        Task<IReadOnlyList<Product>> GetByStockStatusAsync(StockStatus status);

        /// <summary>
        /// Kritik stok seviyesindeki ürünleri getirir
        /// Kısayol metod: GetByStockStatusAsync(StockStatus.Kritik)
        /// </summary>
        Task<IReadOnlyList<Product>> GetCriticalStockProductsAsync();

        /// <summary>
        /// Stokta olmayan (tükenen) ürünleri getirir
        /// </summary>
        Task<IReadOnlyList<Product>> GetOutOfStockProductsAsync();

        /// <summary>
        /// Aktif olmayan ürünleri getirir
        /// NEDEN?
        /// - Haluk Bey: "Şuanda satmadığımız eskiden sattığımız ürünler"
        /// - Stokta kalan eski ürünleri görmek için
        /// </summary>
        Task<IReadOnlyList<Product>> GetInactiveProductsAsync();

        /// <summary>
        /// Fiyat aralığına göre ürünleri getirir
        /// Örn: 1000-5000 TL arası ürünler
        /// </summary>
        Task<IReadOnlyList<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);

        /// <summary>
        /// En çok satan ürünleri getirir
        /// NEDEN?
        /// - Haluk Bey'in raporu: "En çok satılan 10 ürün"
        /// - Analiz ve stok planlaması için
        /// </summary>
        /// <param name="count">Kaç ürün getirileceği (örn: 10)</param>
        Task<IReadOnlyList<Product>> GetTopSellingProductsAsync(int count);

        /// <summary>
        /// Ürünün stok durumunu günceller
        /// NEDEN Service'de değil Repository'de?
        /// - Database logic (trigger benzeri)
        /// - Stok miktarına göre otomatik status hesabı
        /// </summary>
        Task UpdateStockStatusAsync(int productId);

        /// <summary>
        /// Stok miktarını azaltır (Satış sonrası)
        /// </summary>
        Task DecreaseStockAsync(int productId, int quantity);

        /// <summary>
        /// Stok miktarını artırır (Tedarikçiden alım sonrası)
        /// </summary>
        Task IncreaseStockAsync(int productId, int quantity);
    }
}