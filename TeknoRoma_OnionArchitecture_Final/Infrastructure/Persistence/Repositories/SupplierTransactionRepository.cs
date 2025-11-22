// ===================================================================================
// TEKNOROMA - TEDARIKCI ISLEM REPOSITORY IMPLEMENTASYONU (SupplierTransactionRepository.cs)
// ===================================================================================
//
// Bu dosya tedarikci alim ve odeme islemlerini yonetmek icin veritabani katmanini saglar.
// Cari hesap takibi, stok giris kayitlari icin kullanilir.
//
// IS SENARYOLARI:
// - Tedarikci borc durumu
// - Aylik alim raporlari
// - Odenmemis faturalar
//
// ===================================================================================

using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Tedarikci Islem Repository Implementasyonu
    ///
    /// CARI HESAP: Tedarikci borc/alacak takibi
    /// STOK ENTEGRASYONU: Alim kaydi = Stok girisi
    /// </summary>
    public class SupplierTransactionRepository : EfRepository<SupplierTransaction>, ISupplierTransactionRepository
    {
        public SupplierTransactionRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Islem numarasi ile getir
        /// "TH-2024-00001" formatinda (Tedarikci Hareketi)
        /// </summary>
        public async Task<SupplierTransaction?> GetByTransactionNumberAsync(string transactionNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(st => st.TransactionNumber == transactionNumber);
        }

        /// <summary>
        /// Tedarikci islemleri
        ///
        /// Cari hesap ekstresi
        /// Toplam borc hesaplama icin
        /// </summary>
        public async Task<IReadOnlyList<SupplierTransaction>> GetBySupplierAsync(int supplierId)
        {
            return await _dbSet
                .Where(st => st.SupplierId == supplierId)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        /// <summary>
        /// Urun alim gecmisi
        ///
        /// Bu urun hangi tedarikcilerden, ne fiyata alindi?
        /// Maliyet analizi icin
        /// </summary>
        public async Task<IReadOnlyList<SupplierTransaction>> GetByProductAsync(int productId)
        {
            return await _dbSet
                .Where(st => st.ProductId == productId)
                .Include(st => st.Supplier)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        /// <summary>
        /// Tarih araligina gore islemler
        /// Donemsel alim raporlari
        /// </summary>
        public async Task<IReadOnlyList<SupplierTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(st => st.TransactionDate >= startDate && st.TransactionDate <= endDate)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        /// <summary>
        /// Odenmemis islemler
        ///
        /// IsPaid = false olan kayitlar
        /// Tedarikci borc listesi, nakit akis planlamasi
        /// </summary>
        public async Task<IReadOnlyList<SupplierTransaction>> GetUnpaidTransactionsAsync()
        {
            return await _dbSet
                .Where(st => !st.IsPaid)
                .Include(st => st.Supplier)
                .OrderBy(st => st.DueDate)
                .ToListAsync();
        }

        /// <summary>
        /// Aylik toplam alim
        /// Tedarik maliyeti analizi
        /// </summary>
        public async Task<decimal> GetMonthlyTotalAsync(int year, int month)
        {
            return await _dbSet
                .Where(st => st.TransactionDate.Year == year && st.TransactionDate.Month == month)
                .SumAsync(st => st.TotalAmount);
        }

        /// <summary>
        /// Yeni islem numarasi olustur
        ///
        /// FORMAT: "TH-YYYY-NNNNN"
        /// Ornek: "TH-2024-00001"
        /// </summary>
        public async Task<string> GenerateTransactionNumberAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"TH-{year}-";

            // Bu yilin son islem numarasini bul
            var lastTransaction = await _dbSet
                .Where(st => st.TransactionNumber.StartsWith(prefix))
                .OrderByDescending(st => st.TransactionNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastTransaction != null)
            {
                var lastNumberStr = lastTransaction.TransactionNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D5}";
        }
    }
}
