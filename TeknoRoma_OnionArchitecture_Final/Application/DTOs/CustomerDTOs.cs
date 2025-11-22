using TeknoRoma.Domain.Enums;

namespace TeknoRoma.Application.DTOs;

public class CustomerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string FullName => $"{FirstName} {LastName}";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public Gender Gender { get; set; }
    public string GenderDisplay => Gender.ToString();
    public decimal LoyaltyPoints { get; set; }
    public DateTime CreatedDate { get; set; }
    public int TotalPurchases { get; set; }
}

public class CreateCustomerDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public Gender Gender { get; set; }
}

public class UpdateCustomerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public Gender Gender { get; set; }
}

public class CustomerLoyaltyDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public decimal CurrentPoints { get; set; }
    public decimal LifetimePoints { get; set; }
    public decimal PointsRedeemed { get; set; }
}
