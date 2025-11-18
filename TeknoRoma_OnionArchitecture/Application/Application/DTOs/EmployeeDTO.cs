using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.EmployeeDTO
{
    public class EmployeeDTO
    {
        public int Id { get; set; }
        public string IdentityUserId { get; set; }
        public string IdentityNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public UserRole Role { get; set; }
        public decimal? SalesQuota { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateEmployeeDTO
    {
        [Required]
        [StringLength(11, MinimumLength = 11)]
        public string IdentityNumber { get; set; }
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(100)]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Phone]
        public string Phone { get; set; }
        [Required]
        public DateTime BirthDate { get; set; }
        [Required]
        public DateTime HireDate { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Salary { get; set; }
        [Required]
        public UserRole Role { get; set; }
        public decimal? SalesQuota { get; set; }
        [Required]
        public int StoreId { get; set; }
        [Required]
        public int DepartmentId { get; set; }
    }

    public class UpdateEmployeeDTO : CreateEmployeeDTO
    {
        [Required]
        public int Id { get; set; }
        public bool IsActive { get; set; }
    }
}