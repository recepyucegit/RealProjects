// ============================================================================
// IProductRepository.cs - Ürün Repository Interface
// ============================================================================
// AÇIKLAMA:
// Ürün entity'sine özgü veri erişim metodlarını tanımlar.
// IRepository<Product>'ı extend ederek domain-specific metodlar ekler.
//
// NEDEN ÖZEL REPOSITORY?
// Generic IRepository'deki CRUD yeterli değil, ürüne özgü sorgular gerekli:
// - Barkod ile arama (kasa işlemleri için kritik)
// - Stok durumu filtreleme
// - Kategori/Tedarikçi bazlı listeleme
//
// İŞ SENARYOLARI:
// - POS: Barkod okutunca ürün bilgisi çek
// - Stok Yönetimi: Düşük stoklu ürünleri listele
// - Raporlama: Kategori/tedarikçi bazlı analizler
// ============================================================================

using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories
{
    /// <summary>
    /// Ürün Repository Interface
    ///
    /// IRepository&lt;Product&gt; MİRAS ALIR:
    /// - GetByIdAsync, GetAllAsync, AddAsync, UpdateAsync, DeleteAsync
    /// - Bu interface sadece ek metodları tanımlar
    ///
    /// KULLANIM:
    /// <code>
    /// public ProductService(IUnitOfWork unitOfWork)
    /// {
    ///     var products = await unitOfWork.Products.GetLowStockProductsAsync();
    /// }
    /// </code>
    /// </summary>
    public interface IProductRepository : IRepository<Product>
    {
        /// <summary>
        /// Barkod ile Ürün Getir
        ///
        /// EN KRİTİK METOD - Kasa işlemleri buna bağlı!
        /// POS'ta barkod okutulunca bu metod çağrılır.
        ///
        /// PERFORMANS: Barkod alanında INDEX olmalı
        ///
        /// ÖRNEK:
        /// <code>
        /// var product = await _products.GetByBarcodeAsync("8680000000001");
        /// if (product == null)
        ///     throw new ProductNotFoundException("Ürün bulunamadı");
        /// </code>
        /// </summary>
        Task<Product?> GetByBarcodeAsync(string barcode);

        /// <summary>
        /// Kategoriye Göre Ürünler
        ///
        /// E-ticaret kategori sayfaları, yönetim paneli filtreleme
        /// </summary>
        Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId);

        /// <summary>
        /// Tedarikçiye Göre Ürünler
        ///
        /// Tedarikçi sipariş planlama, fiyat analizi
        /// </summary>
        Task<IReadOnlyList<Product>> GetBySupplierAsync(int supplierId);

        /// <summary>
        /// Stok Durumuna Göre Ürünler
        ///
        /// Dashboard widget'ları, stok raporları
        ///
        /// ÖRNEK:
        /// <code>
        /// var outOfStock = await _products.GetByStockStatusAsync(StockStatus.Tukendi);
        /// var critical = await _products.GetByStockStatusAsync(StockStatus.Kritik);
        /// </code>
        /// </summary>
        Task<IReadOnlyList<Product>> GetByStockStatusAsync(StockStatus status);

        /// <summary>
        /// Düşük Stoklu Ürünler
        ///
        /// UnitsInStock <= ReorderLevel olan ürünler
        /// Tedarik planlama için kritik
        ///
        /// DASHBOARD: "X ürün stok uyarısı veriyor"
        /// </summary>
        Task<IReadOnlyList<Product>> GetLowStockProductsAsync();

        /// <summary>
        /// Aktif Ürünler (IsActive = true)
        ///
        /// Satışa açık ürünler, POS listesi
        /// </summary>
        Task<IReadOnlyList<Product>> GetActiveProductsAsync();

        /// <summary>
        /// İsme Göre Arama
        ///
        /// POS'ta ürün arama, e-ticaret arama kutusu
        /// LIKE '%searchTerm%' sorgusu
        /// </summary>
        Task<IReadOnlyList<Product>> SearchByNameAsync(string searchTerm);

        /// <summary>
        /// Stok Güncelle
        ///
        /// Satış/alım sonrası stok değişikliği
        ///
        /// PARAMETRE quantity:
        /// - Pozitif: Stok girişi (tedarikçiden alım)
        /// - Negatif: Stok çıkışı (satış, fire)
        ///
        /// DÖNÜŞ:
        /// - true: Başarılı
        /// - false: Başarısız (yetersiz stok vb.)
        ///
        /// ÖRNEK:
        /// <code>
        /// // Satış sonrası stok düş
        /// await _products.UpdateStockAsync(productId, -5);
        ///
        /// // Tedarikçiden alım sonrası stok ekle
        /// await _products.UpdateStockAsync(productId, 100);
        /// </code>
        /// </summary>
        Task<bool> UpdateStockAsync(int productId, int quantity);
    }
}
