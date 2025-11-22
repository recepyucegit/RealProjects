using TeknoRoma.Domain.Enums;

namespace TeknoRoma.Application.DTOs;

public class SaleDto
{
    public int Id { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentType PaymentType { get; set; }
    public string PaymentTypeDisplay => PaymentType.ToString();
    public SaleStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int StoreId { get; set; }
    public string? StoreName { get; set; }
    public List<SaleDetailDto> Details { get; set; } = new();
}

public class SaleDetailDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? Barcode { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreateSaleDto
{
    public int? CustomerId { get; set; }
    public int EmployeeId { get; set; }
    public int StoreId { get; set; }
    public PaymentType PaymentType { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal? UseLoyaltyPoints { get; set; }
    public List<CreateSaleDetailDto> Details { get; set; } = new();
}

public class CreateSaleDetailDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal? DiscountPercent { get; set; }
}

public class SaleSummaryDto
{
    public int TotalSales { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public class DailySalesSummaryDto
{
    public DateTime Date { get; set; }
    public int SaleCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal CashSales { get; set; }
    public decimal CardSales { get; set; }
}
