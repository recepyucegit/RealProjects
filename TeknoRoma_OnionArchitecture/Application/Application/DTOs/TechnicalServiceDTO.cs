using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.TechnicalServiceDTOs
{
    public class TechnicalServiceDTO
    {
        public int Id { get; set; }
        public string ServiceNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public int ReportedByEmployeeId { get; set; }
        public string ReportedByEmployeeName { get; set; }
        public int? AssignedToEmployeeId { get; set; }
        public string AssignedToEmployeeName { get; set; }
        public bool IsCustomerIssue { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public TechnicalServiceStatus Status { get; set; }
        public int Priority { get; set; }
        public DateTime ReportedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string Resolution { get; set; }
    }

    public class CreateTechnicalServiceDTO
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        [Required]
        [StringLength(2000)]
        public string Description { get; set; }
        [Required]
        public int StoreId { get; set; }
        [Required]
        public int ReportedByEmployeeId { get; set; }
        public bool IsCustomerIssue { get; set; }
        public int? CustomerId { get; set; }
        [Range(1, 4)]
        public int Priority { get; set; } = 2;
    }

    public class UpdateTechnicalServiceDTO
    {
        [Required]
        public int Id { get; set; }
        [StringLength(200)]
        public string Title { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
        public int? AssignedToEmployeeId { get; set; }
        public TechnicalServiceStatus Status { get; set; }
        [Range(1, 4)]
        public int Priority { get; set; }
        [StringLength(2000)]
        public string Resolution { get; set; }
    }
}