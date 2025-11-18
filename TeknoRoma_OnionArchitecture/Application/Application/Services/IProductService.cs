using Application.DTOs.ProductDTO;


namespace Application.Services
{
    /// <summary>
    /// Product Service Interface
    /// İş mantığı koordinasyonu için
    /// 
    /// FARK: Repository vs Service
    /// - Repository: Database işlemleri (Entity ile çalışır)
    /// - Service: İş mantığı (DTO ile çalışır)
    /// 
    /// NEDEN DTO?
    /// - Entity'leri direkt UI'a göndermek güvensiz
    /// - UI'da sadece gerekli alanlar gösterilir
    /// - Validation DTO seviyesinde yapılır
    /// </summary>
    public interface IProductService
    {
        // ====== QUERY OPERATIONS ======

        /// <summary>
        /// Tüm ürünleri getirir (DTO olarak)
        /// </summary>
        Task<IEnumerable<ProductDTO>> GetAllProductsAsync();

        /// <summary>
        /// ID'ye göre ürün getirir
        /// </summary>
        Task<ProductDTO> GetProductByIdAsync(int id);

        /// <summary>
        /// Barkod ile ürün bulur
        /// NEDEN?
        /// - Fahri Cepçi: "Barkod okutup ürün bilgisi almalıyım"
        /// </summary>
        Task<ProductDTO> GetProductByBarcodeAsync(string barcode);

        /// <summary>
        /// Kategoriye göre ürünleri getirir
        /// </summary>
        Task<IEnumerable<ProductDTO>> GetProductsByCategoryAsync(int categoryId);

        /// <summary>
        /// Kritik stok seviyesindeki ürünleri getirir
        /// NEDEN?
        /// - Gül Satar: "Kritik seviyenin altına düşen ürünler uyarı vermeli"
        /// </summary>
        Task<IEnumerable<ProductDTO>> GetCriticalStockProductsAsync();

        /// <summary>
        /// Stokta olmayan ürünleri getirir
        /// </summary>
        Task<IEnumerable<ProductDTO>> GetOutOfStockProductsAsync();

        /// <summary>
        /// Aktif olmayan ürünleri getirir
        /// </summary>
        Task<IEnumerable<ProductDTO>> GetInactiveProductsAsync();

        /// <summary>
        /// En çok satan ürünleri getirir
        /// </summary>
        Task<IEnumerable<ProductDTO>> GetTopSellingProductsAsync(int count);


        // ====== COMMAND OPERATIONS ======

        /// <summary>
        /// Yeni ürün ekler
        /// İŞ MANTIK: Stok durumunu otomatik hesaplar
        /// </summary>
        Task<ProductDTO> CreateProductAsync(CreateProductDTO dto);

        /// <summary>
        /// Ürün günceller
        /// İŞ MANTIK: Stok miktarı değişirse StockStatus güncellenir
        /// </summary>
        Task<ProductDTO> UpdateProductAsync(UpdateProductDTO dto);

        /// <summary>
        /// Ürün siler (Soft Delete)
        /// İŞ MANTIK: IsActive = false yapar
        /// </summary>
        Task<bool> DeleteProductAsync(int id);

        /// <summary>
        /// Stok artırır (Tedarikçiden alım sonrası)
        /// İŞ MANTIK:
        /// 1. Stok miktarını artır
        /// 2. StockStatus güncelle
        /// 3. Log kaydı oluştur
        /// </summary>
        Task<bool> IncreaseStockAsync(int productId, int quantity);

        /// <summary>
        /// Stok azaltır (Satış sonrası)
        /// İŞ MANTIK:
        /// 1. Stok yeterli mi kontrol et
        /// 2. Stok miktarını azalt
        /// 3. StockStatus güncelle
        /// 4. Kritik seviyeye düştüyse uyarı oluştur
        /// </summary>
        Task<bool> DecreaseStockAsync(int productId, int quantity);

        /// <summary>
        /// Stok kontrolü yapar
        /// NEDEN Service'de?
        /// - İş mantığı: Satış yapılabilir mi?
        /// - Kerim Zulacı: "Stokta olmayan ürünü satmayı kesinlikle istemeyiz"
        /// </summary>
        Task<bool> IsStockAvailableAsync(int productId, int quantity);
    }
}