// ===================================================================================
// TEKNOROMA - SATIS REPOSITORY IMPLEMENTASYONU (SaleRepository.cs)
// ===================================================================================
//
// Bu dosya satis islemleri icin veritabani erisim katmanini saglar.
// Raporlama, analiz ve fis numarasi olusturma icin kritik metodlar icerir.
//
// REPOSITORY PATTERN AVANTAJLARI:
// - Veritabani erisim mantigi merkezi konumda
// - Unit testler icin mock'lanabilir
// - Sorgu optimizasyonu tek noktadan yapilabilir
//
// ===================================================================================

using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Satis Repository Implementasyonu
    ///
    /// RAPORLAMA MERKEZI:
    /// En cok kullanilan repository - dashboard ve raporlarin kaynagi
    /// </summary>
    public class SaleRepository : EfRepository<Sale>, ISaleRepository
    {
        public SaleRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Fis numarasi ile satis getir
        /// "S-2024-00001" formatinda benzersiz numara
        /// </summary>
        public async Task<Sale?> GetBySaleNumberAsync(string saleNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber);
        }

        /// <summary>
        /// Musterinin satislari
        /// Musteri profili, satin alma gecmisi
        /// </summary>
        public async Task<IReadOnlyList<Sale>> GetByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(s => s.CustomerId == customerId)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        /// <summary>
        /// Calisanin satislari
        /// Performans degerlendirme, prim hesaplama
        /// </summary>
        public async Task<IReadOnlyList<Sale>> GetByEmployeeAsync(int employeeId)
        {
            return await _dbSet
                .Where(s => s.EmployeeId == employeeId)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        /// <summary>
        /// Magazanin satislari
        /// Sube bazli raporlama, karsilastirma
        /// </summary>
        public async Task<IReadOnlyList<Sale>> GetByStoreAsync(int storeId)
        {
            return await _dbSet
                .Where(s => s.StoreId == storeId)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        /// <summary>
        /// Duruma gore satislar
        /// Bekleyen siparisler, tamamlanan satislar
        /// </summary>
        public async Task<IReadOnlyList<Sale>> GetByStatusAsync(SaleStatus status)
        {
            return await _dbSet
                .Where(s => s.Status == status)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        /// <summary>
        /// Tarih araligina gore satislar
        /// Donemsel raporlar, trend analizi
        /// </summary>
        public async Task<IReadOnlyList<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        /// <summary>
        /// Gunluk toplam ciro
        /// Dashboard "Bugunun cirosu" widget'i
        /// </summary>
        public async Task<decimal> GetDailyTotalAsync(DateTime date, int? storeId = null)
        {
            var query = _dbSet.Where(s => s.SaleDate.Date == date.Date);

            if (storeId.HasValue)
                query = query.Where(s => s.StoreId == storeId.Value);

            return await query.SumAsync(s => s.TotalAmount);
        }

        /// <summary>
        /// Aylik toplam ciro
        /// Aylik raporlar, butce karsilastirma
        /// </summary>
        public async Task<decimal> GetMonthlyTotalAsync(int year, int month, int? storeId = null)
        {
            var query = _dbSet
                .Where(s => s.SaleDate.Year == year && s.SaleDate.Month == month);

            if (storeId.HasValue)
                query = query.Where(s => s.StoreId == storeId.Value);

            return await query.SumAsync(s => s.TotalAmount);
        }

        /// <summary>
        /// Yeni fis numarasi olustur
        ///
        /// FORMAT: "S-YYYY-NNNNN"
        /// Ornek: "S-2024-00001"
        ///
        /// THREAD-SAFE:
        /// Ayni anda birden fazla satis olusturulabilir
        /// </summary>
        public async Task<string> GenerateSaleNumberAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"S-{year}-";

            // Bu yilin son satis numarasini bul
            var lastSale = await _dbSet
                .Where(s => s.SaleNumber.StartsWith(prefix))
                .OrderByDescending(s => s.SaleNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastSale != null)
            {
                // "S-2024-00001" -> "00001" -> 1
                var lastNumberStr = lastSale.SaleNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            // "S-2024-00001" formatinda dondur
            return $"{prefix}{nextNumber:D5}";
        }
    }
}
