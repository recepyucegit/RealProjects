

using Domain.Enums;

namespace TeknoRoma.Application.DTOs;

public class ExpenseDto
{
    public int Id { get; set; }
    public ExpenseType ExpenseType { get; set; }
    public string ExpenseTypeDisplay => ExpenseType.ToString();
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public int? StoreId { get; set; }
    public string? StoreName { get; set; }
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateExpenseDto
{
    public ExpenseType ExpenseType { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.Now;
    public int? StoreId { get; set; }
    public int? EmployeeId { get; set; }
}

public class UpdateExpenseDto
{
    public int Id { get; set; }
    public ExpenseType ExpenseType { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public int? StoreId { get; set; }
}

public class ExpenseSummaryDto
{
    public ExpenseType ExpenseType { get; set; }
    public string ExpenseTypeDisplay => ExpenseType.ToString();
    public decimal TotalAmount { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class MonthlyExpenseDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = null!;
    public decimal TotalExpense { get; set; }
    public List<ExpenseSummaryDto> ByType { get; set; } = new();
}
