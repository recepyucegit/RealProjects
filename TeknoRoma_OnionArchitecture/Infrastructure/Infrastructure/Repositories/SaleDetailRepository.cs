using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// SaleDetail Repository Implementation
    ///
    /// AMAÇ:
    /// - Satış satırlarını yönetir
    /// - En çok satılan ürün analizi
    /// - Ürün satış geçmişi
    ///
    /// NEDEN AYRI REPOSITORY?
    /// - SaleDetail'ler üzerinde özel sorgular gerekli
    /// - En çok satılan ürünler için aggregate fonksiyonlar
    /// - Ürün bazlı analiz için
    ///
    /// HALUK BEY'İN RAPORU:
    /// - "En çok satılan 10 ürün"
    /// </summary>
    public class SaleDetailRepository : Repository<SaleDetail>, ISaleDetailRepository
    {
        public SaleDetailRepository(TeknoromaDbContext context) : base(context)
        {
        }


        // ====== ARAMA METODLARI ======

        /// <summary>
        /// Satışa göre detayları getirir
        /// Fatura görüntüleme için
        /// </summary>
        public async Task<IReadOnlyList<SaleDetail>> GetBySaleIdAsync(int saleId)
        {
            return await _dbSet
                .Include(sd => sd.Product)
                    .ThenInclude(p => p.Category)
                .Where(sd => sd.SaleId == saleId)
                .OrderBy(sd => sd.ID) // Eklenme sırasına göre
                .ToListAsync();
        }

        /// <summary>
        /// Ürüne göre satış detaylarını getirir
        /// Ürün satış geçmişi için
        /// </summary>
        public async Task<IReadOnlyList<SaleDetail>> GetByProductIdAsync(int productId)
        {
            return await _dbSet
                .Include(sd => sd.Sale)
                    .ThenInclude(s => s.Customer)
                .Include(sd => sd.Sale)
                    .ThenInclude(s => s.Employee)
                .Where(sd => sd.ProductId == productId)
                .OrderByDescending(sd => sd.Sale.SaleDate)
                .ToListAsync();
        }

        /// <summary>
        /// Belirli bir ürünün tarih aralığındaki satışlarını getirir
        /// </summary>
        public async Task<IReadOnlyList<SaleDetail>> GetByProductAndDateRangeAsync(int productId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(sd => sd.Sale)
                    .ThenInclude(s => s.Customer)
                .Include(sd => sd.Sale)
                    .ThenInclude(s => s.Employee)
                .Where(sd => sd.ProductId == productId
                          && sd.Sale.SaleDate >= startDate
                          && sd.Sale.SaleDate <= endDate)
                .OrderByDescending(sd => sd.Sale.SaleDate)
                .ToListAsync();
        }

        /// <summary>
        /// Satış detayını ürün bilgisiyle birlikte getirir (Eager Loading)
        /// </summary>
        public async Task<SaleDetail> GetWithProductAsync(int saleDetailId)
        {
            return await _dbSet
                .Include(sd => sd.Product)
                    .ThenInclude(p => p.Category)
                .Include(sd => sd.Product)
                    .ThenInclude(p => p.Supplier)
                .FirstOrDefaultAsync(sd => sd.ID == saleDetailId);
        }

        /// <summary>
        /// Toplam satış miktarını ürün bazında hesaplar
        /// </summary>
        public async Task<int> GetTotalQuantitySoldAsync(int productId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet
                .Include(sd => sd.Sale)
                .Where(sd => sd.ProductId == productId && sd.Sale.Status == SaleStatus.Tamamlandi);

            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(sd => sd.Sale.SaleDate >= startDate.Value
                                       && sd.Sale.SaleDate <= endDate.Value);
            }

            return await query.SumAsync(sd => sd.Quantity);
        }

        /// <summary>
        /// Toplam satış tutarını ürün bazında hesaplar
        /// </summary>
        public async Task<decimal> GetTotalRevenueByProductAsync(int productId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet
                .Include(sd => sd.Sale)
                .Where(sd => sd.ProductId == productId && sd.Sale.Status == SaleStatus.Tamamlandi);

            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(sd => sd.Sale.SaleDate >= startDate.Value
                                       && sd.Sale.SaleDate <= endDate.Value);
            }

            return await query.SumAsync(sd => sd.TotalAmount);
        }


        // ====== ANALİZ VE RAPORLAMA ======

        /// <summary>
        /// En çok satılan ürünleri getirir
        /// Haluk Bey: "En çok satılan 10 ürünü görmek istiyorum"
        /// </summary>
        public async Task<IReadOnlyList<(int ProductId, string ProductName, int TotalQuantity, decimal TotalRevenue)>> GetTopSellingProductsAsync(
            int count, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (count <= 0)
                throw new ArgumentException("Miktar pozitif olmalı", nameof(count));

            var query = _dbSet
                .Include(sd => sd.Product)
                .Include(sd => sd.Sale)
                .Where(sd => sd.Sale.Status == SaleStatus.Tamamlandi);

            // Tarih filtresi varsa
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(sd => sd.Sale.SaleDate >= startDate.Value
                                       && sd.Sale.SaleDate <= endDate.Value);
            }

            // Ürün bazında gruplama ve toplam satış miktarı
            var topProducts = await query
                .GroupBy(sd => new { sd.ProductId, sd.Product.ProductName })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    TotalQuantity = g.Sum(sd => sd.Quantity),
                    TotalRevenue = g.Sum(sd => sd.TotalAmount)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(count)
                .ToListAsync();

            return topProducts.Select(x => (x.ProductId, x.ProductName, x.TotalQuantity, x.TotalRevenue)).ToList();
        }

        /// <summary>
        /// Kategoriye göre satış toplamlarını getirir
        /// </summary>
        public async Task<IReadOnlyList<(int CategoryId, string CategoryName, decimal TotalRevenue)>> GetSalesByCategoryAsync(
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet
                .Include(sd => sd.Product)
                    .ThenInclude(p => p.Category)
                .Include(sd => sd.Sale)
                .Where(sd => sd.Sale.Status == SaleStatus.Tamamlandi);

            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(sd => sd.Sale.SaleDate >= startDate.Value
                                       && sd.Sale.SaleDate <= endDate.Value);
            }

            var salesByCategory = await query
                .GroupBy(sd => new { sd.Product.CategoryId, sd.Product.Category.CategoryName })
                .Select(g => new
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    TotalRevenue = g.Sum(sd => sd.TotalAmount)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToListAsync();

            return salesByCategory.Select(x => (x.CategoryId, x.CategoryName, x.TotalRevenue)).ToList();
        }
    }
}
