using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.SupplierDTO
{
    public class SupplierDTO
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string TaxNumber { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateSupplierDTO
    {
        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; }
        [StringLength(100)]
        public string ContactName { get; set; }
        [Phone]
        public string Phone { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [StringLength(500)]
        public string Address { get; set; }
        [StringLength(100)]
        public string City { get; set; }
        [StringLength(100)]
        public string Country { get; set; }
        [StringLength(50)]
        public string TaxNumber { get; set; }
    }

    public class UpdateSupplierDTO : CreateSupplierDTO
    {
        [Required]
        public int Id { get; set; }
        public bool IsActive { get; set; }
    }
}