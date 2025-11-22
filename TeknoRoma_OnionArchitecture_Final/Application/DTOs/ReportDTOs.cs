namespace TeknoRoma.Application.DTOs;

/// <summary>
/// Sales report for a specific period
/// </summary>
public class SalesReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal NetRevenue { get; set; }
    public int TotalTransactions { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<DailySalesSummaryDto> DailyBreakdown { get; set; } = new();
    public List<PaymentTypeSummaryDto> ByPaymentType { get; set; } = new();
}

/// <summary>
/// Payment type breakdown
/// </summary>
public class PaymentTypeSummaryDto
{
    public string PaymentType { get; set; } = null!;
    public decimal Amount { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Top selling product report
/// </summary>
public class TopSellingProductReportDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? CategoryName { get; set; }
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal Profit { get; set; }
}

/// <summary>
/// Employee sales performance report
/// </summary>
public class EmployeeSalesReportDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = null!;
    public string? StoreName { get; set; }
    public int TotalSales { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int ItemsSold { get; set; }
}

/// <summary>
/// Category sales report
/// </summary>
public class CategorySalesReportDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public int ProductCount { get; set; }
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Stock report
/// </summary>
public class StockReportDto
{
    public int TotalProducts { get; set; }
    public int InStockProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public decimal TotalStockValue { get; set; }
    public List<LowStockProductDto> LowStockItems { get; set; } = new();
}

/// <summary>
/// Low stock product item
/// </summary>
public class LowStockProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? CategoryName { get; set; }
    public string? SupplierName { get; set; }
    public int CurrentStock { get; set; }
    public int MinStock { get; set; }
    public int Deficit { get; set; }
}

/// <summary>
/// Profit and loss report
/// </summary>
public class ProfitLossReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal GrossRevenue { get; set; }
    public decimal CostOfGoodsSold { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public decimal ProfitMargin { get; set; }
    public List<ExpenseSummaryDto> ExpenseBreakdown { get; set; } = new();
}

/// <summary>
/// Customer report
/// </summary>
public class CustomerReportDto
{
    public int TotalCustomers { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public int ActiveCustomers { get; set; }
    public decimal TotalLoyaltyPointsIssued { get; set; }
    public decimal TotalLoyaltyPointsRedeemed { get; set; }
    public List<TopCustomerDto> TopCustomers { get; set; } = new();
}

/// <summary>
/// Top customer item
/// </summary>
public class TopCustomerDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public int TotalPurchases { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal LoyaltyPoints { get; set; }
    public DateTime LastPurchaseDate { get; set; }
}

/// <summary>
/// Technical service report
/// </summary>
public class TechnicalServiceReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalTickets { get; set; }
    public int CompletedTickets { get; set; }
    public int PendingTickets { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRepairTimeHours { get; set; }
    public List<TechnicianPerformanceDto> TechnicianPerformance { get; set; } = new();
    public List<DeviceTypeStatDto> ByDeviceType { get; set; } = new();
}

/// <summary>
/// Technician performance
/// </summary>
public class TechnicianPerformanceDto
{
    public int TechnicianId { get; set; }
    public string TechnicianName { get; set; } = null!;
    public int TicketsCompleted { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRepairTimeHours { get; set; }
}

/// <summary>
/// Device type statistics
/// </summary>
public class DeviceTypeStatDto
{
    public string DeviceType { get; set; } = null!;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}
