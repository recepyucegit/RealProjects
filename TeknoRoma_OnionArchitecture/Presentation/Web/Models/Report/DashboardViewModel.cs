namespace Web.Models.Report
{
    /// <summary>
    /// Dashboard ViewModel
    /// Her rol için özelleştirilmiş dashboard
    ///
    /// ROLLER:
    /// - Şube Müdürü: Genel raporlar, çalışan performansı
    /// - Kasa Satış: Bugünkü satışlar, prime ne kadar yaklaştım
    /// - Mobil Satış: Bugünkü satışlar, kotaya ne kadar kaldı
    /// - Depo: Bekleyen siparişler, kritik stok
    /// - Muhasebe: Ödenmemiş faturalar, aylık giderler
    /// - Teknik Servis: Açık sorunlar, atanan işler
    /// </summary>
    public class DashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;

        // Şube Müdürü için
        public decimal? TodaysTotalSales { get; set; }
        public decimal? MonthlyTotalSales { get; set; }
        public int? TotalEmployees { get; set; }
        public int? CriticalStockCount { get; set; }

        // Satış Temsilcileri için
        public decimal? MonthlySales { get; set; }
        public decimal? SalesQuota { get; set; }
        public decimal? CommissionAmount { get; set; }
        public decimal? QuotaPercentage => SalesQuota > 0 ? (MonthlySales / SalesQuota) * 100 : 0;

        // Depo için
        public int? PendingOrdersCount { get; set; }
        public int? OutOfStockProductsCount { get; set; }

        // Muhasebe için
        public decimal? UnpaidExpensesAmount { get; set; }
        public int? UnpaidExpensesCount { get; set; }
        public decimal? MonthlyExpenses { get; set; }

        // Teknik Servis için
        public int? OpenIssuesCount { get; set; }
        public int? AssignedToMeCount { get; set; }
        public int? CriticalIssuesCount { get; set; }
    }
}
