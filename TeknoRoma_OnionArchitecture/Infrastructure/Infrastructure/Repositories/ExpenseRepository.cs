using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Expense Repository Implementation
    ///
    /// AMAÇ:
    /// - Feyza Paragöz (Muhasebe) için gider yönetimi
    /// - Çalışan ödemeleri, faturalar, altyapı giderleri takibi
    /// - Aylık gider raporları
    /// - Ödenmemiş gider takibi
    ///
    /// GİDER TÜRLERİ:
    /// 1. CalisanOdemesi: Maaş ödemeleri
    /// 2. TeknikaltyapiGideri: Sunucu, internet, elektrik vb.
    /// 3. Fatura: Tedarikçi faturaları
    /// 4. DigerGider: Diğer giderler
    /// </summary>
    public class ExpenseRepository : Repository<Expense>, IExpenseRepository
    {
        public ExpenseRepository(TeknoromaDbContext context) : base(context)
        {
        }


        // ====== ARAMA METODLARI ======

        /// <summary>
        /// Gider numarasına göre gider bulur
        /// Format: G-2024-00001
        /// </summary>
        public async Task<Expense> GetByExpenseNumberAsync(string expenseNumber)
        {
            if (string.IsNullOrWhiteSpace(expenseNumber))
                throw new ArgumentException("Gider numarası boş olamaz", nameof(expenseNumber));

            return await _dbSet
                .Include(e => e.Store)
                .Include(e => e.Employee)
                .FirstOrDefaultAsync(e => e.ExpenseNumber == expenseNumber);
        }

        /// <summary>
        /// Mağazaya göre giderleri getirir
        /// </summary>
        public async Task<IReadOnlyList<Expense>> GetByStoreAsync(int storeId)
        {
            return await _dbSet
                .Include(e => e.Employee)
                .Where(e => e.StoreId == storeId)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
        }

        /// <summary>
        /// Gider türüne göre getirir
        /// Haluk Bey: "Çalışan ödemeleri, Faturalar, Diğer giderler"
        /// </summary>
        public async Task<IReadOnlyList<Expense>> GetByExpenseTypeAsync(ExpenseType expenseType)
        {
            return await _dbSet
                .Include(e => e.Store)
                .Include(e => e.Employee)
                .Where(e => e.ExpenseType == expenseType)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
        }

        /// <summary>
        /// Tarih aralığındaki giderleri getirir
        /// Aylık gider raporu için
        /// </summary>
        public async Task<IReadOnlyList<Expense>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Başlangıç tarihi bitiş tarihinden sonra olamaz");

            return await _dbSet
                .Include(e => e.Store)
                .Include(e => e.Employee)
                .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
        }

        /// <summary>
        /// Çalışana göre giderleri getirir (Maaş ödemeleri)
        /// </summary>
        public async Task<IReadOnlyList<Expense>> GetByEmployeeAsync(int employeeId)
        {
            return await _dbSet
                .Include(e => e.Store)
                .Where(e => e.EmployeeId == employeeId)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
        }

        /// <summary>
        /// Ödenmemiş giderleri getirir
        /// Feyza Paragöz: "Ödeme takibi yapmalıyım"
        /// </summary>
        public async Task<IReadOnlyList<Expense>> GetUnpaidExpensesAsync()
        {
            return await _dbSet
                .Include(e => e.Store)
                .Include(e => e.Employee)
                .Where(e => e.IsPaid == false)
                .OrderBy(e => e.ExpenseDate) // Eski borçlar önce
                .ToListAsync();
        }


        // ====== FİNANSAL HESAPLAMALAR ======

        /// <summary>
        /// Aylık toplam gideri hesaplar
        /// Feyza Paragöz: "Aylık giriş çıkışları görebilmeliyim"
        ///
        /// NEDEN AmountInTRY?
        /// - Dövizli giderler de var (USD, EUR)
        /// - Tüm giderler TL'ye çevrilmiş halde saklanır
        /// - Toplamı TL üzerinden hesaplıyoruz
        /// </summary>
        public async Task<decimal> GetMonthlyTotalExpenseAsync(int year, int month, int? storeId = null)
        {
            var firstDayOfMonth = new DateTime(year, month, 1);
            var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

            var query = _dbSet
                .Where(e => e.ExpenseDate >= firstDayOfMonth
                         && e.ExpenseDate < firstDayOfNextMonth);

            // Mağaza filtresi varsa
            if (storeId.HasValue)
            {
                query = query.Where(e => e.StoreId == storeId.Value);
            }

            var total = await query.SumAsync(e => e.AmountInTRY);
            return total;
        }

        /// <summary>
        /// Gider türüne göre aylık toplam hesaplar
        /// Detaylı gider analizi için
        /// Örnek: Bu ay toplam maaş ödemeleri ne kadar?
        /// </summary>
        public async Task<decimal> GetMonthlyExpenseByTypeAsync(
            int year,
            int month,
            ExpenseType expenseType,
            int? storeId = null)
        {
            var firstDayOfMonth = new DateTime(year, month, 1);
            var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

            var query = _dbSet
                .Where(e => e.ExpenseDate >= firstDayOfMonth
                         && e.ExpenseDate < firstDayOfNextMonth
                         && e.ExpenseType == expenseType);

            // Mağaza filtresi varsa
            if (storeId.HasValue)
            {
                query = query.Where(e => e.StoreId == storeId.Value);
            }

            var total = await query.SumAsync(e => e.AmountInTRY);
            return total;
        }


        // ====== NUMARA OLUŞTURMA ======

        /// <summary>
        /// Yeni gider numarası oluşturur
        /// Format: G-2024-00001, G-2024-00002, ...
        /// </summary>
        public async Task<string> GenerateExpenseNumberAsync()
        {
            var currentYear = DateTime.Now.Year;
            var prefix = $"G-{currentYear}-";

            var lastExpense = await _dbSet
                .Where(e => e.ExpenseNumber.StartsWith(prefix))
                .OrderByDescending(e => e.ExpenseNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastExpense != null)
            {
                var lastNumberPart = lastExpense.ExpenseNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D5}"; // Format: G-2024-00001
        }
    }
}
