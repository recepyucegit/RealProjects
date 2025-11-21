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
        /// Tedarikçinin belirli tarih aralığındaki işlemlerini getirir
        /// </summary>
        public async Task<IReadOnlyList<SupplierTransaction>> GetBySupplierAndDateRangeAsync(
            int supplierId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(st => st.Product)
                    .ThenInclude(p => p.Category)
                .Where(st => st.SupplierId == supplierId
                          && st.TransactionDate >= startDate
                          && st.TransactionDate <= endDate)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        /// <summary>
        /// Ödenmemiş hareketleri getirir
        /// Borç takibi için
        /// </summary>
        public async Task<IReadOnlyList<SupplierTransaction>> GetUnpaidAsync()
        {
            return await _dbSet
                .Include(st => st.Supplier)
                .Include(st => st.Product)
                .Where(st => st.IsPaid == false)
                .OrderBy(st => st.TransactionDate) // Eski borçlar önce
                .ToListAsync();
        }

        /// <summary>
        /// Tedarikçinin ödenmemiş işlemlerini getirir
        /// </summary>
        public async Task<IReadOnlyList<SupplierTransaction>> GetUnpaidBySupplierAsync(int supplierId)
        {
            return await _dbSet
                .Include(st => st.Product)
                .Where(st => st.SupplierId == supplierId && st.IsPaid == false)
                .OrderBy(st => st.TransactionDate)
                .ToListAsync();
        }

        /// <summary>
        /// Fatura numarasına göre işlem bulur
        /// </summary>
        public async Task<SupplierTransaction> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber))
                throw new ArgumentException("Fatura numarası boş olamaz", nameof(invoiceNumber));

            return await _dbSet
                .Include(st => st.Supplier)
                .Include(st => st.Product)
                .FirstOrDefaultAsync(st => st.InvoiceNumber == invoiceNumber);
        }

        /// <summary>
        /// İşlemi ilişkili verilerle getirir (Eager Loading)
        /// </summary>
        public async Task<SupplierTransaction> GetWithDetailsAsync(int transactionId)
        {
            return await _dbSet
                .Include(st => st.Supplier)
                .Include(st => st.Product)
                    .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(st => st.ID == transactionId);
        }


        // ====== FİNANSAL HESAPLAMALAR ======

        /// <summary>
        /// Tedarikçinin toplam alım tutarını hesaplar
        /// </summary>
        public async Task<decimal> GetTotalAmountBySupplierAsync(int supplierId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.Where(st => st.SupplierId == supplierId);

            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(st => st.TransactionDate >= startDate.Value
                                       && st.TransactionDate <= endDate.Value);
            }

            return await query.SumAsync(st => st.TotalAmount);
        }

        /// <summary>
        /// Toplam borç tutarını hesaplar (tüm tedarikçiler)
        /// </summary>
        public async Task<decimal> GetTotalUnpaidAmountAsync()
        {
            return await _dbSet
                .Where(st => st.IsPaid == false)
                .SumAsync(st => st.TotalAmount);
        }

        /// <summary>
        /// Tedarikçiye olan borç tutarını hesaplar
        /// </summary>
        public async Task<decimal> GetUnpaidAmountBySupplierAsync(int supplierId)
        {
            return await _dbSet
                .Where(st => st.SupplierId == supplierId && st.IsPaid == false)
                .SumAsync(st => st.TotalAmount);
        }

        /// <summary>
        /// En çok alım yapılan tedarikçileri getirir
        /// </summary>
        public async Task<IReadOnlyList<(int SupplierId, string SupplierName, decimal TotalAmount, int TransactionCount)>> GetTopSuppliersAsync(
            int count, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.Include(st => st.Supplier).AsQueryable();

            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(st => st.TransactionDate >= startDate.Value
                                       && st.TransactionDate <= endDate.Value);
            }

            var topSuppliers = await query
                .GroupBy(st => new { st.SupplierId, st.Supplier.CompanyName })
                .Select(g => new
                {
                    SupplierId = g.Key.SupplierId,
                    SupplierName = g.Key.CompanyName,
                    TotalAmount = g.Sum(st => st.TotalAmount),
                    TransactionCount = g.Count()
                })
                .OrderByDescending(x => x.TotalAmount)
                .Take(count)
                .ToListAsync();

            return topSuppliers.Select(x => (x.SupplierId, x.SupplierName, x.TotalAmount, x.TransactionCount)).ToList();
        }

        /// <summary>
        /// Ürün bazlı alım istatistiklerini getirir
        /// </summary>
        public async Task<IReadOnlyList<(int ProductId, string ProductName, int TotalQuantity, decimal TotalCost)>> GetPurchasesByProductAsync(
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.Include(st => st.Product).AsQueryable();

            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(st => st.TransactionDate >= startDate.Value
                                       && st.TransactionDate <= endDate.Value);
            }

            var purchases = await query
                .GroupBy(st => new { st.ProductId, st.Product.ProductName })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    TotalQuantity = g.Sum(st => st.Quantity),
                    TotalCost = g.Sum(st => st.TotalAmount)
                })
                .OrderByDescending(x => x.TotalCost)
                .ToListAsync();

            return purchases.Select(x => (x.ProductId, x.ProductName, x.TotalQuantity, x.TotalCost)).ToList();
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


        // ====== ÖDEME İŞLEMLERİ ======

        /// <summary>
        /// Ödeme durumunu günceller
        /// </summary>
        public async Task MarkAsPaidAsync(int transactionId, DateTime paymentDate)
        {
            var transaction = await _dbSet.FindAsync(transactionId);
            if (transaction != null)
            {
                transaction.IsPaid = true;
                transaction.PaymentDate = paymentDate;
                await UpdateAsync(transaction);
            }
        }

        /// <summary>
        /// Birden fazla işlemi ödenmiş olarak işaretler
        /// </summary>
        public async Task MarkRangeAsPaidAsync(IEnumerable<int> transactionIds, DateTime paymentDate)
        {
            var transactions = await _dbSet
                .Where(st => transactionIds.Contains(st.ID))
                .ToListAsync();

            foreach (var transaction in transactions)
            {
                transaction.IsPaid = true;
                transaction.PaymentDate = paymentDate;
            }

            await UpdateRangeAsync(transactions);
        }
    }
}
