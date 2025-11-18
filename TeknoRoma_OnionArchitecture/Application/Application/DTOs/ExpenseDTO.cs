using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ExpenseDTO
{
    public class ExpenseDTO
    {
        public int Id { get; set; }
        public string ExpenseNumber { get; set; }
        public DateTime ExpenseDate { get; set; }
        public ExpenseType ExpenseType { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public int? EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public decimal? ExchangeRate { get; set; }
        public decimal AmountInTRY { get; set; }
        public string Description { get; set; }
        public string DocumentNumber { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
    }

    public class CreateExpenseDTO
    {
        [Required]
        public DateTime ExpenseDate { get; set; }
        [Required]
        public ExpenseType ExpenseType { get; set; }
        [Required]
        public int StoreId { get; set; }
        public int? EmployeeId { get; set; }
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        public Currency Currency { get; set; } = Currency.TRY;
        public decimal? ExchangeRate { get; set; }
        [Required]
        [StringLength(500)]
        public string Description { get; set; }
        [StringLength(100)]
        public string DocumentNumber { get; set; }
    }

    public class UpdateExpenseDTO : CreateExpenseDTO
    {
        [Required]
        public int Id { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}

