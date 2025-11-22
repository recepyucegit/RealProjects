namespace TeknoRoma.Application.DTOs;

public class StoreDto
{
    public int Id { get; set; }
    public string StoreName { get; set; } = null!;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int EmployeeCount { get; set; }
    public decimal TotalSales { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateStoreDto
{
    public string StoreName { get; set; } = null!;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class UpdateStoreDto
{
    public int Id { get; set; }
    public string StoreName { get; set; } = null!;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class StorePerformanceDto
{
    public int StoreId { get; set; }
    public string StoreName { get; set; } = null!;
    public decimal TotalRevenue { get; set; }
    public int TotalSales { get; set; }
    public int EmployeeCount { get; set; }
    public decimal AverageOrderValue { get; set; }
}
