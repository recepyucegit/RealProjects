

using Domain.Enums;

namespace TeknoRoma.Application.DTOs;

public class SupplierTransactionDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public string CurrencyDisplay => Currency.ToString();
    public decimal? ExchangeRate { get; set; }
    public decimal AmountInTRY { get; set; }
    public DateTime TransactionDate { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaymentDate { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public int? Quantity { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateSupplierTransactionDto
{
    public int SupplierId { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; } = Currency.TRY;
    public decimal? ExchangeRate { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.Now;
    public int? ProductId { get; set; }
    public int? Quantity { get; set; }
}

public class UpdateSupplierTransactionDto
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public decimal? ExchangeRate { get; set; }
    public DateTime TransactionDate { get; set; }
}

public class SupplierTransactionPaymentDto
{
    public int Id { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.Now;
}

public class SupplierBalanceDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = null!;
    public decimal TotalDebt { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal Balance { get; set; }
    public int UnpaidTransactionCount { get; set; }
}
