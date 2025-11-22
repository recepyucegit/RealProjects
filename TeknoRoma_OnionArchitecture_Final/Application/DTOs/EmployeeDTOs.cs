using TeknoRoma.Domain.Enums;

namespace TeknoRoma.Application.DTOs;

public class EmployeeDto
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
    public UserRole Role { get; set; }
    public string RoleDisplay => Role.ToString();
    public decimal Salary { get; set; }
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; }
    public int? StoreId { get; set; }
    public string? StoreName { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateEmployeeDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Phone { get; set; }
    public string Email { get; set; } = null!;
    public string? Address { get; set; }
    public Gender Gender { get; set; }
    public UserRole Role { get; set; } = UserRole.KasaSatis;
    public decimal Salary { get; set; }
    public DateTime HireDate { get; set; } = DateTime.Now;
    public int? StoreId { get; set; }
    public int? DepartmentId { get; set; }
    public string Password { get; set; } = null!;
}

public class UpdateEmployeeDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Phone { get; set; }
    public string Email { get; set; } = null!;
    public string? Address { get; set; }
    public Gender Gender { get; set; }
    public UserRole Role { get; set; }
    public decimal Salary { get; set; }
    public bool IsActive { get; set; }
    public int? StoreId { get; set; }
    public int? DepartmentId { get; set; }
}

public class LoginDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class LoginResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public EmployeeDto? Employee { get; set; }
    public string? Token { get; set; }
}

public class ChangePasswordDto
{
    public int EmployeeId { get; set; }
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}
