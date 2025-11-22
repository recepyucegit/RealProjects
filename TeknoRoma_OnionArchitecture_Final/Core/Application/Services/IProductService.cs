// ============================================================================
// IProductService.cs - Ürün Servis Interface
// ============================================================================
// AÇIKLAMA:
// Ürün yönetimi için iş mantığı (business logic) katmanı interface'i.
// Repository'den farklı olarak burada iş kuralları uygulanır.
//
// SERVİS vs REPOSITORY FARKI:
// Repository: Sadece CRUD (Create, Read, Update, Delete)
// Service: İş kuralları, validasyon, çoklu repository koordinasyonu
//
// ÖRNEK:
// Repository.UpdateStockAsync: Sadece stok günceller
// Service.UpdateStockAsync: Stok günceller + log yazar + alarm üretir
//
// KULLANIM:
// Controller -> Service -> Repository -> Database
// ============================================================================

using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    /// <summary>
    /// Ürün Servis Interface
    ///
    /// İŞ MANTIKLARI:
    /// - Barkod benzersizlik kontrolü
    /// - Stok güncelleme + alarm
    /// - Soft delete kontrolü
    /// </summary>
    public interface IProductService
    {
        // ========================================================================
        // SORGULAMA (QUERY) METODLARI
        // ========================================================================

        /// <summary>
        /// ID ile Ürün Getir
        ///
        /// NULL DÖNEBİLİR: Ürün bulunamazsa null döner
        /// Product? = nullable reference type (C# 8.0+)
        ///
        /// KULLANIM:
        /// var product = await _productService.GetByIdAsync(1);
        /// if (product == null) return NotFound();
        /// </summary>
        Task<Product?> GetByIdAsync(int id);

        /// <summary>
        /// Barkod ile Ürün Getir
        ///
        /// KASİYER KULLANIMI: Barkod okutulunca ürün bilgisi gelir
        /// POS ENTEGRASYonu: Barkod tarayıcı bu metodu tetikler
        ///
        /// BARKOD FORMATLARI:
        /// - EAN-13: 8691234567890 (Türkiye'de yaygın)
        /// - UPC-A: 012345678901 (ABD)
        /// - İç Barkod: PRD-00001 (mağaza içi)
        /// </summary>
        Task<Product?> GetByBarcodeAsync(string barcode);

        /// <summary>
        /// Tüm Ürünleri Getir
        ///
        /// DİKKAT: Büyük veri setlerinde sayfalama (pagination) gerekir
        /// IEnumerable: Lazy loading, bellek verimli
        ///
        /// ADMIN PANELİ: Ürün listesi sayfası
        /// </summary>
        Task<IEnumerable<Product>> GetAllAsync();

        /// <summary>
        /// Kategoriye Göre Ürünler
        ///
        /// MENÜ FİLTRESİ: "Telefonlar" kategorisi seçilince
        /// categoryId -> Product.CategoryId eşleşmesi
        /// </summary>
        Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);

        /// <summary>
        /// Tedarikçiye Göre Ürünler
        ///
        /// TEDARİKÇİ ANALİZİ: Hangi ürünler bu tedarikçiden?
        /// SİPARİŞ PLANLAMA: Tek tedarikçiden toplu sipariş
        /// </summary>
        Task<IEnumerable<Product>> GetBySupplierAsync(int supplierId);

        /// <summary>
        /// Kritik Stok Seviyesindeki Ürünler
        ///
        /// DASHBOARD UYARISI: Stok tükenmeden önce uyarı
        /// OTOMATİK SİPARİŞ: Bu liste ile otomatik sipariş oluşturulabilir
        ///
        /// KRİTER: CurrentStock &lt;= CriticalStockLevel
        /// </summary>
        Task<IEnumerable<Product>> GetLowStockProductsAsync();

        /// <summary>
        /// Aktif Ürünler
        ///
        /// SOFT DELETE: IsDeleted = false olan ürünler
        /// WEB VİTRİN: Müşteriye gösterilen ürünler
        /// </summary>
        Task<IEnumerable<Product>> GetActiveProductsAsync();

        /// <summary>
        /// Ürün Arama
        ///
        /// FULL-TEXT SEARCH: Ürün adı, barkod, açıklama içinde arama
        /// LIKE sorgusu veya SQL Full-Text Index kullanır
        ///
        /// ÖRNEK: "Samsung" araması -> Samsung ile başlayan tüm ürünler
        /// </summary>
        Task<IEnumerable<Product>> SearchAsync(string searchTerm);

        // ========================================================================
        // KOMUT (COMMAND) METODLARI
        // ========================================================================

        /// <summary>
        /// Yeni Ürün Oluştur
        ///
        /// VALİDASYON KURALLARI:
        /// - Barkod benzersiz olmalı
        /// - Fiyat >= 0
        /// - KDV oranı geçerli olmalı
        ///
        /// GERİ DÖNÜŞ: Oluşturulan entity (ID atanmış)
        /// </summary>
        Task<Product> CreateAsync(Product product);

        /// <summary>
        /// Ürün Güncelle
        ///
        /// CONCURRENCY: Aynı ürün aynı anda iki yerde güncellenirse?
        /// EF Core Optimistic Concurrency ile yönetilir
        ///
        /// AUDİT: UpdatedAt otomatik güncellenir
        /// </summary>
        Task UpdateAsync(Product product);

        /// <summary>
        /// Ürün Sil (Soft Delete)
        ///
        /// SOFT DELETE: Veri fiziksel olarak silinmez
        /// IsDeleted = true yapılır
        ///
        /// NEDEN? Satış geçmişi ve raporlar için veri korunmalı
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Stok Güncelle
        ///
        /// SATIŞ SONRASI: quantity = -1 (stoktan düş)
        /// ALIM SONRASI: quantity = +10 (stoğa ekle)
        ///
        /// İŞ KURALLARI:
        /// - Stok negatife düşemez
        /// - Kritik seviyede alarm üretilir
        /// - Stok hareketi loglanır
        ///
        /// GERİ DÖNÜŞ: İşlem başarılı mı?
        /// </summary>
        Task<bool> UpdateStockAsync(int productId, int quantity);

        /// <summary>
        /// Barkod Benzersizlik Kontrolü
        ///
        /// YENİ ÜRÜN: excludeId = null -> Tüm ürünlerde kontrol
        /// GÜNCELLEME: excludeId = 5 -> ID=5 hariç kontrol
        ///
        /// NEDEN excludeId?
        /// Ürün güncelleme formunda kendi barkodunu değiştirmeyince
        /// "bu barkod kullanımda" hatası vermemeli
        ///
        /// ÖRNEK:
        /// // Yeni ürün için
        /// if (await IsBarcodeTakenAsync("123456789"))
        ///     return BadRequest("Barkod kullanımda");
        ///
        /// // Güncelleme için
        /// if (await IsBarcodeTakenAsync("123456789", excludeId: productId))
        ///     return BadRequest("Barkod başka üründe kullanımda");
        /// </summary>
        Task<bool> IsBarcodeTakenAsync(string barcode, int? excludeId = null);
    }
}
