using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// SupplierTransaction Repository Implementation
    ///
    /// AMAÇ:
    /// - Tedarikçiden ürün alım hareketlerini yönetir
    /// - Stok girişi ve maliyet takibi
    /// - Fatura ve ödeme takibi
    /// - Haluk Bey'in raporları: "Hangi tedarikçiden ne kadar almışız?"
    ///
    /// İŞ AKIŞI:
    /// 1. Tedarikçiden ürün alınır
    /// 2. SupplierTransaction kaydı oluşturulur
    /// 3. Product.UnitsInStock artırılır
    /// 4. Fatura kaydedilir
    /// 5. Ödeme yapılır (IsPaid = true)
    /// </summary>
    public class SupplierTransactionRepository : Repository<SupplierTransaction>, ISupplierTransactionRepository
    {
        public SupplierTransactionRepository(TeknoromaDbContext context) : base(context)
        {
        }


        // ====== ARAMA METODLARI ======

        /// <summary>
        /// İşlem numarasına göre hareket bulur
        /// Format: TH-2024-00001
        /// </summary>
        public async Task<SupplierTransaction> GetByTransactionNumberAsync(string transactionNumber)
        {
            if (string.IsNullOrWhiteSpace(transactionNumber))
                throw new ArgumentException("İşlem numarası boş olamaz", nameof(transactionNumber));

            return await _dbSet
                .Include(st => st.Supplier)
                .Include(st => st.Product)
                    .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(st => st.TransactionNumber == transactionNumber);
        }

        /// <summary>
        /// Tedarikçiye göre hareketleri getirir
        /// Haluk Bey: "Hangi tedarikçiden ne kadar almışız?"
        /// </summary>
        public async Task<IReadOnlyList<SupplierTransaction>> GetBySupplierAsync(int supplierId)
        {
            return await _dbSet
                .Include(st => st.Product)
                    .ThenInclude(p => p.Category)
                .Where(st => st.SupplierId == supplierId)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        /// <summary>
        /// Ürüne göre hareketleri getirir
        /// Ürün alım geçmişi için
        /// </summary>
        public async Task<IReadOnlyList<SupplierTransaction>> GetByProductAsync(int productId)
        {
            return await _dbSet
                .Include(st => st.Supplier)
                .Where(st => st.ProductId == productId)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        /// <summary>
        /// Tarih aralığındaki hareketleri getirir
        /// Haluk Bey: "Bu ay hangi tedarikçiden ne kadar almışız?"
        /// </summary>
        public async Task<IReadOnlyList<SupplierTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Başlangıç tarihi bitiş tarihinden sonra olamaz");

            return await _dbSet
                .Include(st => st.Supplier)
                .Include(st => st.Product)
                    .ThenInclude(p => p.Category)
                .Where(st => st.TransactionDate >= startDate && st.TransactionDate <= endDate)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        /// <summary>
        /// Ödenmemiş hareketleri getirir
        /// Borç takibi için
        /// </summary>
        public async Task<IReadOnlyList<SupplierTransaction>> GetUnpaidTransactionsAsync()
        {
            return await _dbSet
                .Include(st => st.Supplier)
                .Include(st => st.Product)
                .Where(st => st.IsPaid == false)
                .OrderBy(st => st.TransactionDate) // Eski borçlar önce
                .ToListAsync();
        }


        // ====== FİNANSAL HESAPLAMALAR ======

        /// <summary>
        /// Aylık toplam alım tutarını hesaplar
        /// Haluk Bey: "Bu ay toplam ne kadar ürün aldık?"
        /// </summary>
        public async Task<decimal> GetMonthlyTotalPurchaseAsync(int year, int month)
        {
            var firstDayOfMonth = new DateTime(year, month, 1);
            var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

            var total = await _dbSet
                .Where(st => st.TransactionDate >= firstDayOfMonth
                          && st.TransactionDate < firstDayOfNextMonth)
                .SumAsync(st => st.TotalAmount);

            return total;
        }

        /// <summary>
        /// Tedarikçinin aylık toplam alım tutarını hesaplar
        /// Haluk Bey: "Bu ay X tedarikçisinden ne kadar aldık?"
        /// </summary>
        public async Task<decimal> GetSupplierMonthlyPurchaseAsync(int supplierId, int year, int month)
        {
            var firstDayOfMonth = new DateTime(year, month, 1);
            var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

            var total = await _dbSet
                .Where(st => st.SupplierId == supplierId
                          && st.TransactionDate >= firstDayOfMonth
                          && st.TransactionDate < firstDayOfNextMonth)
                .SumAsync(st => st.TotalAmount);

            return total;
        }


        // ====== NUMARA OLUŞTURMA ======

        /// <summary>
        /// Yeni işlem numarası oluşturur
        /// Format: TH-2024-00001, TH-2024-00002, ...
        /// </summary>
        public async Task<string> GenerateTransactionNumberAsync()
        {
            var currentYear = DateTime.Now.Year;
            var prefix = $"TH-{currentYear}-";

            var lastTransaction = await _dbSet
                .Where(st => st.TransactionNumber.StartsWith(prefix))
                .OrderByDescending(st => st.TransactionNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastTransaction != null)
            {
                var lastNumberPart = lastTransaction.TransactionNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D5}"; // Format: TH-2024-00001
        }
    }
}
