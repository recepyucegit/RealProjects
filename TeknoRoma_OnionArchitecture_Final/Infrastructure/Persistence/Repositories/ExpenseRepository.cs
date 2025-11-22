// ===================================================================================
// TEKNOROMA - GIDER REPOSITORY IMPLEMENTASYONU (ExpenseRepository.cs)
// ===================================================================================
//
// Bu dosya gider islemleri icin veritabani erisim katmanini saglar.
// Maliyet analizi, odeme takibi ve finansal raporlama icin kullanilir.
//
// IS SENARYOLARI:
// - Aylik gider raporlari
// - Odenmemis giderler (nakit akis yonetimi)
// - Gider turu bazli analiz
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
    /// Gider Repository Implementasyonu
    ///
    /// FINANSAL KONTROL: Maliyet takibi ve butce yonetimi
    /// </summary>
    public class ExpenseRepository : EfRepository<Expense>, IExpenseRepository
    {
        public ExpenseRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gider numarasi ile getir
        /// "G-2024-00001" formatinda benzersiz numara
        /// </summary>
        public async Task<Expense?> GetByExpenseNumberAsync(string expenseNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(e => e.ExpenseNumber == expenseNumber);
        }

        /// <summary>
        /// Magaza giderleri
        /// Sube bazli maliyet analizi
        /// </summary>
        public async Task<IReadOnlyList<Expense>> GetByStoreAsync(int storeId)
        {
            return await _dbSet
                .Where(e => e.StoreId == storeId)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
        }

        /// <summary>
        /// Ture gore giderler
        /// Personel, fatura, teknik giderler ayrimi
        /// </summary>
        public async Task<IReadOnlyList<Expense>> GetByExpenseTypeAsync(ExpenseType expenseType)
        {
            return await _dbSet
                .Where(e => e.ExpenseType == expenseType)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
        }

        /// <summary>
        /// Tarih araligina gore giderler
        /// Donemsel raporlar
        /// </summary>
        public async Task<IReadOnlyList<Expense>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
        }

        /// <summary>
        /// Calisana ait giderler
        /// Maas, prim odemeleri (ExpenseType.CalisanOdemesi)
        /// </summary>
        public async Task<IReadOnlyList<Expense>> GetByEmployeeAsync(int employeeId)
        {
            return await _dbSet
                .Where(e => e.EmployeeId == employeeId)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
        }

        /// <summary>
        /// Odenmemis giderler
        ///
        /// Nakit akis yonetimi, borc takibi
        /// IsPaid = false olan kayitlar
        /// </summary>
        public async Task<IReadOnlyList<Expense>> GetUnpaidExpensesAsync()
        {
            return await _dbSet
                .Where(e => !e.IsPaid)
                .OrderBy(e => e.DueDate)
                .ToListAsync();
        }

        /// <summary>
        /// Aylik toplam gider
        /// Butce karsilastirma, kar/zarar hesabi
        /// </summary>
        public async Task<decimal> GetMonthlyTotalAsync(int year, int month, int? storeId = null)
        {
            var query = _dbSet
                .Where(e => e.ExpenseDate.Year == year && e.ExpenseDate.Month == month);

            if (storeId.HasValue)
                query = query.Where(e => e.StoreId == storeId.Value);

            return await query.SumAsync(e => e.Amount);
        }

        /// <summary>
        /// Yeni gider numarasi olustur
        ///
        /// FORMAT: "G-YYYY-NNNNN"
        /// Ornek: "G-2024-00001"
        /// </summary>
        public async Task<string> GenerateExpenseNumberAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"G-{year}-";

            // Bu yilin son gider numarasini bul
            var lastExpense = await _dbSet
                .Where(e => e.ExpenseNumber.StartsWith(prefix))
                .OrderByDescending(e => e.ExpenseNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastExpense != null)
            {
                var lastNumberStr = lastExpense.ExpenseNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D5}";
        }
    }
}
