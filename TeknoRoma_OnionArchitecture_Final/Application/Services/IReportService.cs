namespace Application.Services
{
    /// <summary>
    /// Rapor servisi - Haluk Bey'in istediği raporlar
    /// </summary>
    public interface IReportService
    {
        // Satış Raporları
        Task<decimal> GetDailySalesAsync(DateTime date, int? storeId = null);
        Task<decimal> GetMonthlySalesAsync(int year, int month, int? storeId = null);
        Task<decimal> GetYearlySalesAsync(int year, int? storeId = null);

        // Ürün Raporları
        Task<IEnumerable<TopSellingProductReport>> GetTopSellingProductsAsync(int count, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<LowStockProductReport>> GetLowStockReportAsync();

        // Çalışan Raporları
        Task<IEnumerable<EmployeeSalesReport>> GetEmployeeSalesReportAsync(DateTime startDate, DateTime endDate, int? storeId = null);

        // Gider Raporları
        Task<decimal> GetMonthlyExpensesAsync(int year, int month, int? storeId = null);
        Task<IEnumerable<ExpenseByTypeReport>> GetExpensesByTypeAsync(int year, int month, int? storeId = null);

        // Mağaza Raporları
        Task<IEnumerable<StoreSalesReport>> GetStoreSalesComparisonAsync(DateTime startDate, DateTime endDate);
    }

    // Report DTOs
    public class TopSellingProductReport
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class LowStockProductReport
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int CurrentStock { get; set; }
        public int CriticalLevel { get; set; }
    }

    public class EmployeeSalesReport
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = null!;
        public int SaleCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ExpenseByTypeReport
    {
        public string ExpenseType { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public int Count { get; set; }
    }

    public class StoreSalesReport
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; } = null!;
        public decimal TotalSales { get; set; }
        public int SaleCount { get; set; }
    }
}
