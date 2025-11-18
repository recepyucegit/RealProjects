using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories
{
    /// <summary>
    /// Expense Repository Interface
    /// Gider işlemleri için özel metodlar
    /// </summary>
    public interface IExpenseRepository : IRepository<Expense>
    {
        /// <summary>
        /// Gider numarasına göre gider bulur
        /// </summary>
        Task<Expense> GetByExpenseNumberAsync(string expenseNumber);

        /// <summary>
        /// Mağazaya göre giderleri getirir
        /// </summary>
        Task<IReadOnlyList<Expense>> GetByStoreAsync(int storeId);

        /// <summary>
        /// Gider türüne göre getirir
        /// NEDEN?
        /// - Haluk Bey'in raporu: "Çalışan ödemeleri, Faturalar, Diğer giderler"
        /// </summary>
        Task<IReadOnlyList<Expense>> GetByExpenseTypeAsync(ExpenseType expenseType);

        /// <summary>
        /// Tarih aralığındaki giderleri getirir
        /// Aylık gider raporu için
        /// </summary>
        Task<IReadOnlyList<Expense>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Çalışana göre giderleri getirir (Maaş ödemeleri)
        /// </summary>
        Task<IReadOnlyList<Expense>> GetByEmployeeAsync(int employeeId);

        /// <summary>
        /// Ödenmemiş giderleri getirir
        /// NEDEN?
        /// - Feyza Paragöz: "Ödeme takibi yapmalıyım"
        /// </summary>
        Task<IReadOnlyList<Expense>> GetUnpaidExpensesAsync();

        /// <summary>
        /// Aylık toplam gideri hesaplar
        /// NEDEN?
        /// - Feyza Paragöz: "Aylık giriş çıkışları görebilmeliyim"
        /// </summary>
        Task<decimal> GetMonthlyTotalExpenseAsync(int year, int month, int? storeId = null);

        /// <summary>
        /// Gider türüne göre aylık toplam hesaplar
        /// Detaylı gider analizi için
        /// </summary>
        Task<decimal> GetMonthlyExpenseByTypeAsync(int year, int month, ExpenseType expenseType, int? storeId = null);

        /// <summary>
        /// Yeni gider numarası oluşturur
        /// Format: G-2024-00001
        /// </summary>
        Task<string> GenerateExpenseNumberAsync();
    }

    /// <summary>
    /// TechnicalService Repository Interface
    /// Teknik servis işlemleri için özel metodlar
    /// </summary>
    public interface ITechnicalServiceRepository : IRepository<TechnicalService>
    {
        /// <summary>
        /// Servis numarasına göre servis kaydı bulur
        /// </summary>
        Task<TechnicalService> GetByServiceNumberAsync(string serviceNumber);

        /// <summary>
        /// Duruma göre servis kayıtlarını getirir
        /// NEDEN?
        /// - Özgün Kablocu: "Açık sorunları görüp öncelik sırasına göre çözmeliyim"
        /// </summary>
        Task<IReadOnlyList<TechnicalService>> GetByStatusAsync(TechnicalServiceStatus status);

        /// <summary>
        /// Sorunu bildiren çalışana göre getirir
        /// </summary>
        Task<IReadOnlyList<TechnicalService>> GetByReportedEmployeeAsync(int employeeId);

        /// <summary>
        /// Atanan teknik servis elemanına göre getirir
        /// </summary>
        Task<IReadOnlyList<TechnicalService>> GetByAssignedEmployeeAsync(int employeeId);

        /// <summary>
        /// Müşteriye göre servis kayıtlarını getirir
        /// </summary>
        Task<IReadOnlyList<TechnicalService>> GetByCustomerAsync(int customerId);

        /// <summary>
        /// Mağazaya göre servis kayıtlarını getirir
        /// </summary>
        Task<IReadOnlyList<TechnicalService>> GetByStoreAsync(int storeId);

        /// <summary>
        /// Müşteri sorunlarını getirir
        /// </summary>
        Task<IReadOnlyList<TechnicalService>> GetCustomerIssuesAsync();

        /// <summary>
        /// Sistem sorunlarını getirir
        /// </summary>
        Task<IReadOnlyList<TechnicalService>> GetSystemIssuesAsync();

        /// <summary>
        /// Açık sorunları öncelik sırasına göre getirir
        /// NEDEN?
        /// - Önce kritik sorunlar çözülmeli
        /// </summary>
        Task<IReadOnlyList<TechnicalService>> GetOpenIssuesByPriorityAsync();

        /// <summary>
        /// Yeni servis numarası oluşturur
        /// Format: TS-2024-00001
        /// </summary>
        Task<string> GenerateServiceNumberAsync();
    }

    /// <summary>
    /// SupplierTransaction Repository Interface
    /// Tedarikçi hareketi işlemleri için özel metodlar
    /// </summary>
    public interface ISupplierTransactionRepository : IRepository<SupplierTransaction>
    {
        /// <summary>
        /// İşlem numarasına göre hareket bulur
        /// </summary>
        Task<SupplierTransaction> GetByTransactionNumberAsync(string transactionNumber);

        /// <summary>
        /// Tedarikçiye göre hareketleri getirir
        /// </summary>
        Task<IReadOnlyList<SupplierTransaction>> GetBySupplierAsync(int supplierId);

        /// <summary>
        /// Ürüne göre hareketleri getirir
        /// Ürün alım geçmişi için
        /// </summary>
        Task<IReadOnlyList<SupplierTransaction>> GetByProductAsync(int productId);

        /// <summary>
        /// Tarih aralığındaki hareketleri getirir
        /// NEDEN?
        /// - Haluk Bey'in raporu: "Bu ay hangi tedarikçiden ne kadar almışız"
        /// </summary>
        Task<IReadOnlyList<SupplierTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Ödenmemiş hareketleri getirir
        /// Borç takibi için
        /// </summary>
        Task<IReadOnlyList<SupplierTransaction>> GetUnpaidTransactionsAsync();

        /// <summary>
        /// Aylık toplam alım tutarını hesaplar
        /// </summary>
        Task<decimal> GetMonthlyTotalPurchaseAsync(int year, int month);

        /// <summary>
        /// Tedarikçinin aylık toplam alım tutarını hesaplar
        /// </summary>
        Task<decimal> GetSupplierMonthlyPurchaseAsync(int supplierId, int year, int month);

        /// <summary>
        /// Yeni işlem numarası oluşturur
        /// Format: TH-2024-00001
        /// </summary>
        Task<string> GenerateTransactionNumberAsync();
    }

    /// <summary>
    /// SaleDetail Repository Interface
    /// Satış detayları için özel metodlar
    /// </summary>
    public interface ISaleDetailRepository : IRepository<SaleDetail>
    {
        /// <summary>
        /// Satışa göre detayları getirir
        /// </summary>
        Task<IReadOnlyList<SaleDetail>> GetBySaleAsync(int saleId);

        /// <summary>
        /// Ürüne göre satış detaylarını getirir
        /// Ürün satış geçmişi için
        /// </summary>
        Task<IReadOnlyList<SaleDetail>> GetByProductAsync(int productId);

        /// <summary>
        /// En çok satılan ürünleri getirir
        /// NEDEN?
        /// - Haluk Bey'in raporu: "En çok satılan 10 ürün"
        /// </summary>
        Task<IReadOnlyList<SaleDetail>> GetTopSellingProductsAsync(int count, DateTime? startDate = null, DateTime? endDate = null);
    }
}