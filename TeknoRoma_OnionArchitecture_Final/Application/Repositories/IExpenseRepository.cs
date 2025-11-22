// ============================================================================
// IExpenseRepository.cs - Gider Repository Interface
// ============================================================================
// AÇIKLAMA:
// Gider entity'sine özgü veri erişim metodlarını tanımlar.
// Maliyet analizi, ödeme takibi ve finansal raporlama için.
//
// İŞ SENARYOLARI:
// - Aylık gider raporları
// - Ödenmemiş giderler (nakit akış yönetimi)
// - Gider türü bazlı analiz
// ============================================================================

using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories
{
    /// <summary>
    /// Gider Repository Interface
    ///
    /// FİNANSAL KONTROL: Maliyet takibi ve bütçe yönetimi
    /// </summary>
    public interface IExpenseRepository : IRepository<Expense>
    {
        /// <summary>
        /// Gider Numarası ile Getir
        /// "G-2024-00001" formatında benzersiz numara
        /// </summary>
        Task<Expense?> GetByExpenseNumberAsync(string expenseNumber);

        /// <summary>
        /// Mağaza Giderleri
        /// Şube bazlı maliyet analizi
        /// </summary>
        Task<IReadOnlyList<Expense>> GetByStoreAsync(int storeId);

        /// <summary>
        /// Türe Göre Giderler
        /// Personel, fatura, teknik giderler ayrımı
        /// </summary>
        Task<IReadOnlyList<Expense>> GetByExpenseTypeAsync(ExpenseType expenseType);

        /// <summary>
        /// Tarih Aralığına Göre Giderler
        /// Dönemsel raporlar
        /// </summary>
        Task<IReadOnlyList<Expense>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Çalışana Ait Giderler
        /// Maaş, prim ödemeleri (ExpenseType.CalisanOdemesi)
        /// </summary>
        Task<IReadOnlyList<Expense>> GetByEmployeeAsync(int employeeId);

        /// <summary>
        /// Ödenmemiş Giderler
        /// Nakit akış yönetimi, borç takibi
        /// IsPaid = false olan kayıtlar
        /// </summary>
        Task<IReadOnlyList<Expense>> GetUnpaidExpensesAsync();

        /// <summary>
        /// Aylık Toplam Gider
        /// Bütçe karşılaştırma, kar/zarar hesabı
        /// </summary>
        Task<decimal> GetMonthlyTotalAsync(int year, int month, int? storeId = null);

        /// <summary>
        /// Yeni Gider Numarası Oluştur
        /// FORMAT: "G-YYYY-NNNNN"
        /// </summary>
        Task<string> GenerateExpenseNumberAsync();
    }
}
