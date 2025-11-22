// ===================================================================================
// TEKNOROMA - SATIS DETAY REPOSITORY IMPLEMENTASYONU (SaleDetailRepository.cs)
// ===================================================================================
//
// Bu dosya satis kalemlerine (SaleDetail) erisim icin veritabani katmanini saglar.
// Urun bazli satis analizi ve en cok satilan urun raporlari icin kullanilir.
//
// IS SENARYOLARI:
// - Fis detaylarini goruntuleme
// - Urun satis gecmisi
// - En cok satilan urunler (Top 10)
//
// ===================================================================================

using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Satis Detay Repository Implementasyonu
    ///
    /// URUN ANALIZI: En cok satilan, trend olan urunler
    /// </summary>
    public class SaleDetailRepository : EfRepository<SaleDetail>, ISaleDetailRepository
    {
        public SaleDetailRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Bir satisin tum kalemleri
        ///
        /// Fis detayi goruntuleme, iade islemi
        /// Sale -> SaleDetails iliskisi
        /// </summary>
        public async Task<IReadOnlyList<SaleDetail>> GetBySaleAsync(int saleId)
        {
            return await _dbSet
                .Where(sd => sd.SaleId == saleId)
                .Include(sd => sd.Product)
                .ToListAsync();
        }

        /// <summary>
        /// Urunun satis gecmisi
        ///
        /// Bu urun hangi satislarda, kac adet satildi?
        /// Urun performans analizi
        /// </summary>
        public async Task<IReadOnlyList<SaleDetail>> GetByProductAsync(int productId)
        {
            return await _dbSet
                .Where(sd => sd.ProductId == productId)
                .Include(sd => sd.Sale)
                .OrderByDescending(sd => sd.Sale.SaleDate)
                .ToListAsync();
        }

        /// <summary>
        /// En cok satilan urunler
        ///
        /// Dashboard widget, trend analizi
        /// Satis adedi veya tutarina gore sirali
        ///
        /// SORGU MANTIGI:
        /// 1. Tarih filtresi uygula (opsiyonel)
        /// 2. UrunId'ye gore grupla
        /// 3. Toplam satisa gore sirala
        /// 4. Istenen sayida al
        /// </summary>
        public async Task<IReadOnlyList<SaleDetail>> GetTopSellingProductsAsync(
            int count,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _dbSet.Include(sd => sd.Sale).AsQueryable();

            // Tarih filtresi
            if (startDate.HasValue)
                query = query.Where(sd => sd.Sale.SaleDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(sd => sd.Sale.SaleDate <= endDate.Value);

            // En cok satilan urunlerin ID'lerini bul
            var topProductIds = await query
                .GroupBy(sd => sd.ProductId)
                .OrderByDescending(g => g.Sum(sd => sd.Quantity))
                .Take(count)
                .Select(g => g.Key)
                .ToListAsync();

            // Bu urunlerin detaylarini getir
            return await _dbSet
                .Where(sd => topProductIds.Contains(sd.ProductId))
                .Include(sd => sd.Product)
                .ToListAsync();
        }
    }
}
