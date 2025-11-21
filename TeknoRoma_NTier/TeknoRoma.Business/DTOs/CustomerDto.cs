namespace TeknoRoma.Business.DTOs;

/// <summary>
/// Customer entity için Data Transfer Object
/// GÜVENLİK: Password alanı DTO'da yer almaz!
/// </summary>
public class CustomerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? PostalCode { get; set; }
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// Tam ad
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
}

/// <summary>
/// Müşteri kayıt (register) için DTO
/// </summary>
public class RegisterCustomerDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? PostalCode { get; set; }
}

/// <summary>
/// Müşteri giriş (login) için DTO
/// </summary>
public class LoginCustomerDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Müşteri bilgilerini güncelleme için DTO
/// </summary>
public class UpdateCustomerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? PostalCode { get; set; }
}
