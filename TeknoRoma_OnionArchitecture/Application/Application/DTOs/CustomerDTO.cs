using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.CustomerDTO
{
    public class CustomerDTO
    {
        public int Id { get; set; }
        public string IdentityNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public DateTime? BirthDate { get; set; }
        public int? Age { get; set; }
        public Gender? Gender { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateCustomerDTO
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
        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; }
        [StringLength(500)]
        public string Address { get; set; }
        [StringLength(100)]
        public string City { get; set; }
    }

    public class UpdateCustomerDTO : CreateCustomerDTO
    {
        [Required]
        public int Id { get; set; }
        public bool IsActive { get; set; }
    }
}