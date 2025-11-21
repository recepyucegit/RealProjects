using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Product Repository Implementation
    ///
    /// AMAÇ:
    /// - Product entity'sine özel iş mantığı
    /// - Stok yönetimi (artırma, azaltma, durum güncelleme)
    /// - Barkod arama (Fahri Cepçi için)
    /// - Kategori ve tedarikçi filtreleme
    /// - En çok satılan ürünler (Haluk Bey raporları)
    ///
    /// NEDEN REPOSITORY<PRODUCT> MIRAS ALIR?
    /// - Generic CRUD operasyonlarını kullanır
    /// - Sadece özel metodları implement eder
    /// - Kod tekrarını önler
    ///
    /// MİMARİ:
    /// Application.Repositories.IProductRepository → Infrastructure.Repositories.ProductRepository
    /// (Interface)                                   (Implementation)
    /// </summary>
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        /// <summary>
        /// Constructor - Base class'a DbContext geçer
        /// </summary>
        public ProductRepository(TeknoromaDbContext context) : base(context)
        {
        }


        // ====== ARAMA METODLARI ======

        /// <summary>
        /// Barkod numarasına göre ürün bulur
        /// Fahri Cepçi: "Barkod okuttuğumda ürün bilgilerini görebilmeliyim"
        ///
        /// NEDEN İNCLUDE?
        /// - Category ve Supplier bilgileri de gerekli
        /// - Eager Loading ile tek sorguda getirilir
        /// </summary>
        public async Task<Product> GetByBarcodeAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                throw new ArgumentException("Barkod boş olamaz", nameof(barcode));

            return await _dbSet
                .Include(p => p.Category)      // Kategori bilgisi
                .Include(p => p.Supplier)      // Tedarikçi bilgisi
                .FirstOrDefaultAsync(p => p.Barcode == barcode);
            // Global Query Filter: IsDeleted = false otomatik
        }

        /// <summary>
        /// Kategoriye göre ürünleri getirir
        /// Haluk Bey: "Kategorilere göre ürünlerin listesi"
        ///
        /// NEDEN READONLY LIST?
        /// - Dönen liste değiştirilmesin (immutable)
        /// - Memory optimizasyonu
        /// </summary>
        public async Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.CategoryId == categoryId)
                .OrderBy(p => p.Name) // Alfabetik sıralama
                .ToListAsync();
        }

        /// <summary>
        /// Tedarikçiye göre ürünleri getirir
        /// Haluk Bey: "Hangi tedarikçiden ne kadar ürün alıyoruz?"
        /// </summary>
        public async Task<IReadOnlyList<Product>> GetBySupplierAsync(int supplierId)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.SupplierId == supplierId)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Fiyat aralığına göre ürünleri getirir
        /// Örnek: 1000-5000 TL arası ürünler
        /// </summary>
        public async Task<IReadOnlyList<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            if (minPrice < 0 || maxPrice < 0)
                throw new ArgumentException("Fiyatlar negatif olamaz");

            if (minPrice > maxPrice)
                throw new ArgumentException("Minimum fiyat maksimum fiyattan büyük olamaz");

            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.UnitPrice >= minPrice && p.UnitPrice <= maxPrice)
                .OrderBy(p => p.UnitPrice) // Fiyata göre sıralama
                .ToListAsync();
        }


        // ====== STOK BAZLI SORGULAR ======

        /// <summary>
        /// Stok durumuna göre ürünleri getirir
        /// Gül Satar & Kerim Zulacı: "Kritik seviyedeki ürünleri görmem lazım"
        /// </summary>
        public async Task<IReadOnlyList<Product>> GetByStockStatusAsync(StockStatus status)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.StockStatus == status)
                .OrderBy(p => p.UnitsInStock) // Stok miktarına göre sıralama
                .ToListAsync();
        }

        /// <summary>
        /// Kritik stok seviyesindeki ürünleri getirir
        /// Kısayol metod - Sık kullanılır
        /// </summary>
        public async Task<IReadOnlyList<Product>> GetCriticalStockProductsAsync()
        {
            return await GetByStockStatusAsync(StockStatus.Kritik);
        }

        /// <summary>
        /// Stokta olmayan (tükenen) ürünleri getirir
        /// </summary>
        public async Task<IReadOnlyList<Product>> GetOutOfStockProductsAsync()
        {
            return await GetByStockStatusAsync(StockStatus.Tukendi);
        }

        /// <summary>
        /// Aktif olmayan ürünleri getirir
        /// Haluk Bey: "Şu anda satmadığımız eski ürünler"
        ///
        /// NEDEN WHERE ile IsActive = false?
        /// - Global Query Filter IsDeleted kontrol eder
        /// - IsActive ayrı bir business logic
        /// </summary>
        public async Task<IReadOnlyList<Product>> GetInactiveProductsAsync()
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.IsActive == false)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }


        // ====== ANALİZ VE RAPORLAMA ======

        /// <summary>
        /// En çok satılan ürünleri getirir
        /// Haluk Bey: "En çok satılan 10 ürünü görmek istiyorum"
        ///
        /// NASIL HESAPLANIYOR?
        /// - SaleDetail tablosundan Quantity toplamı
        /// - GROUP BY ProductId
        /// - ORDER BY TotalQuantity DESC
        /// - TAKE(count)
        ///
        /// NEDEN JOIN?
        /// - SaleDetail ile ilişki kurulur
        /// - Her ürünün toplam satış miktarı hesaplanır
        /// </summary>
        public async Task<IReadOnlyList<Product>> GetTopSellingProductsAsync(int count)
        {
            if (count <= 0)
                throw new ArgumentException("Miktar pozitif olmalı", nameof(count));

            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.SaleDetails) // Satış detayları
                .OrderByDescending(p => p.SaleDetails.Sum(sd => sd.Quantity)) // Toplam satış miktarı
                .Take(count) // İlk N ürün
                .ToListAsync();
        }


        // ====== STOK YÖNETİMİ ======

        /// <summary>
        /// Ürünün stok durumunu günceller
        ///
        /// İŞ KURALI:
        /// - UnitsInStock > CriticalStockLevel → Yeterli
        /// - 0 < UnitsInStock ≤ CriticalStockLevel → Kritik
        /// - UnitsInStock = 0 → Tükendi
        ///
        /// NEDEN AYRI METOD?
        /// - Stok durumu her stok değişiminde güncellenmeli
        /// - Merkezi business logic
        /// - Satış ve alım işlemlerinden sonra çağrılır
        /// </summary>
        public async Task UpdateStockStatusAsync(int productId)
        {
            var product = await _dbSet.FindAsync(productId);

            if (product == null)
                throw new ArgumentException($"Ürün bulunamadı: {productId}");

            // İş kuralı: Stok durumu hesaplama
            if (product.UnitsInStock == 0)
            {
                product.StockStatus = StockStatus.Tukendi;
            }
            else if (product.UnitsInStock <= product.CriticalStockLevel)
            {
                product.StockStatus = StockStatus.Kritik;
            }
            else
            {
                product.StockStatus = StockStatus.Yeterli;
            }

            product.ModifiedDate = DateTime.Now;
            _dbSet.Update(product);
            // SaveChangesAsync çağrılmalı (UnitOfWork pattern'de)
        }

        /// <summary>
        /// Stok miktarını azaltır (Satış sonrası)
        ///
        /// SENARIO:
        /// 1. Müşteri ürün satın alır
        /// 2. SaleDetail kaydı oluşturulur
        /// 3. Product.UnitsInStock azaltılır ← BU METOD
        /// 4. StockStatus güncellenir
        ///
        /// NEDEN TRANSACTION?
        /// - Stok azaltma ve durum güncelleme atomik olmalı
        /// - Ya her ikisi de olur ya hiçbiri (ACID)
        /// </summary>
        public async Task DecreaseStockAsync(int productId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Miktar pozitif olmalı", nameof(quantity));

            var product = await _dbSet.FindAsync(productId);

            if (product == null)
                throw new ArgumentException($"Ürün bulunamadı: {productId}");

            if (product.UnitsInStock < quantity)
                throw new InvalidOperationException(
                    $"Yetersiz stok! Mevcut: {product.UnitsInStock}, İstenen: {quantity}");

            // Stok azaltma
            product.UnitsInStock -= quantity;
            product.ModifiedDate = DateTime.Now;

            _dbSet.Update(product);

            // Stok durumunu güncelle
            await UpdateStockStatusAsync(productId);
        }

        /// <summary>
        /// Stok miktarını artırır (Tedarikçiden alım sonrası)
        ///
        /// SENARIO:
        /// 1. Tedarikçiden ürün alınır
        /// 2. SupplierTransaction kaydı oluşturulur
        /// 3. Product.UnitsInStock artırılır ← BU METOD
        /// 4. StockStatus güncellenir
        /// </summary>
        public async Task IncreaseStockAsync(int productId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Miktar pozitif olmalı", nameof(quantity));

            var product = await _dbSet.FindAsync(productId);

            if (product == null)
                throw new ArgumentException($"Ürün bulunamadı: {productId}");

            // Stok artırma
            product.UnitsInStock += quantity;
            product.ModifiedDate = DateTime.Now;

            _dbSet.Update(product);

            // Stok durumunu güncelle
            await UpdateStockStatusAsync(productId);
        }
    }
}
