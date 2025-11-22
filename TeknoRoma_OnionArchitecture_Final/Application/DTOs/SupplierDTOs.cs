namespace TeknoRoma.Application.DTOs;

public class SupplierDto
{
    public int Id { get; set; }
    public string SupplierName { get; set; } = null!;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public int ProductCount { get; set; }
    public decimal TotalTransactionAmount { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateSupplierDto
{
    public string SupplierName { get; set; } = null!;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}

public class UpdateSupplierDto
{
    public int Id { get; set; }
    public string SupplierName { get; set; } = null!;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}
