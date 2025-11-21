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
        public async Task<IReadOnlyList<SaleDetail>> GetBySaleAsync(int saleId)
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
        public async Task<IReadOnlyList<SaleDetail>> GetByProductAsync(int productId)
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


        // ====== ANALİZ VE RAPORLAMA ======

        /// <summary>
        /// En çok satılan ürünleri getirir
        /// Haluk Bey: "En çok satılan 10 ürünü görmek istiyorum"
        ///
        /// NASIL HESAPLANIYOR?
        /// - SaleDetail'leri ProductId'ye göre grupla
        /// - Her grup için SUM(Quantity) hesapla
        /// - Büyükten küçüğe sırala
        /// - İlk N ürünü getir
        ///
        /// NEDEN SADECE TAMAMLANAN SATIŞLAR?
        /// - İptal edilen satışlar dahil olmamalı
        /// - Beklemede olanlar henüz gerçekleşmedi
        ///
        /// SQL Eşdeğeri:
        /// SELECT sd.ProductId, SUM(sd.Quantity) as TotalQuantity
        /// FROM SaleDetails sd
        /// JOIN Sales s ON sd.SaleId = s.ID
        /// WHERE s.Status = Tamamlandi
        ///   [AND s.SaleDate BETWEEN startDate AND endDate]
        /// GROUP BY sd.ProductId
        /// ORDER BY TotalQuantity DESC
        /// LIMIT count
        /// </summary>
        public async Task<IReadOnlyList<SaleDetail>> GetTopSellingProductsAsync(
            int count,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (count <= 0)
                throw new ArgumentException("Miktar pozitif olmalı", nameof(count));

            var query = _dbSet
                .Include(sd => sd.Product)
                    .ThenInclude(p => p.Category)
                .Include(sd => sd.Product)
                    .ThenInclude(p => p.Supplier)
                .Include(sd => sd.Sale)
                .Where(sd => sd.Sale.Status == SaleStatus.Tamamlandi);

            // Tarih filtresi varsa
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(sd => sd.Sale.SaleDate >= startDate.Value
                                       && sd.Sale.SaleDate <= endDate.Value);
            }

            // Ürün bazında gruplama ve toplam satış miktarı
            var topProductDetails = await query
                .GroupBy(sd => sd.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(sd => sd.Quantity),
                    FirstDetail = g.OrderByDescending(sd => sd.Sale.SaleDate).First() // En son satılan detay
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(count)
                .Select(x => x.FirstDetail)
                .ToListAsync();

            return topProductDetails;
        }
    }
}
