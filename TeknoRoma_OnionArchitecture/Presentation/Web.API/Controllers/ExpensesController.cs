using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ExpensesController> _logger;

        public ExpensesController(IUnitOfWork unitOfWork, ILogger<ExpensesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Get all expenses (Muhasebe and SubeYoneticisi only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Muhasebe,SubeYoneticisi")]
        public async Task<IActionResult> GetAll()
        {
            var expenses = await _unitOfWork.Expenses.GetAllAsync();
            return Ok(expenses);
        }

        /// <summary>
        /// Get expense by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Muhasebe,SubeYoneticisi")]
        public async Task<IActionResult> GetById(int id)
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
            if (expense == null)
                return NotFound(new { message = "Gider bulunamadı" });

            return Ok(expense);
        }

        /// <summary>
        /// Get unpaid expenses (Feyza Paragöz için)
        /// </summary>
        [HttpGet("unpaid")]
        [Authorize(Roles = "Muhasebe,SubeYoneticisi")]
        public async Task<IActionResult> GetUnpaid()
        {
            var unpaidExpenses = await _unitOfWork.Expenses.GetUnpaidExpensesAsync();
            return Ok(unpaidExpenses);
        }

        /// <summary>
        /// Get monthly expenses total
        /// </summary>
        [HttpGet("monthly/{year}/{month}")]
        [Authorize(Roles = "Muhasebe,SubeYoneticisi")]
        public async Task<IActionResult> GetMonthlyTotal(int year, int month)
        {
            var total = await _unitOfWork.Expenses.GetMonthlyExpensesTotalAsync(year, month);
            return Ok(new { year, month, totalAmount = total });
        }

        /// <summary>
        /// Get expenses by type
        /// </summary>
        [HttpGet("type/{expenseType}")]
        [Authorize(Roles = "Muhasebe,SubeYoneticisi")]
        public async Task<IActionResult> GetByType(ExpenseType expenseType)
        {
            var expenses = await _unitOfWork.Expenses.GetByExpenseTypeAsync(expenseType);
            return Ok(expenses);
        }

        /// <summary>
        /// Get unpaid expenses amount
        /// </summary>
        [HttpGet("unpaid/amount")]
        [Authorize(Roles = "Muhasebe,SubeYoneticisi")]
        public async Task<IActionResult> GetUnpaidAmount()
        {
            var amount = await _unitOfWork.Expenses.GetUnpaidExpensesAmountAsync();
            return Ok(new { unpaidAmount = amount });
        }

        /// <summary>
        /// Create new expense (Muhasebe only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Muhasebe")]
        public async Task<IActionResult> Create([FromBody] ExpenseCreateDto dto)
        {
            try
            {
                var expense = new Expense
                {
                    StoreId = dto.StoreId,
                    ExpenseType = dto.ExpenseType,
                    Description = dto.Description,
                    Amount = dto.Amount,
                    Currency = dto.Currency,
                    ExchangeRate = dto.ExchangeRate,
                    AmountInTRY = dto.Currency == Currency.TRY
                        ? dto.Amount
                        : dto.Amount * dto.ExchangeRate,
                    ExpenseDate = dto.ExpenseDate,
                    IsPaid = false,
                    CreatedDate = DateTime.Now,
                    IsDeleted = false
                };

                await _unitOfWork.Expenses.AddAsync(expense);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Yeni gider oluşturuldu: {ExpenseId}", expense.ID);
                return CreatedAtAction(nameof(GetById), new { id = expense.ID }, expense);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gider eklenirken hata");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mark expense as paid (Muhasebe only)
        /// </summary>
        [HttpPut("{id}/pay")]
        [Authorize(Roles = "Muhasebe")]
        public async Task<IActionResult> MarkAsPaid(int id, [FromBody] PaymentDto dto)
        {
            try
            {
                var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
                if (expense == null)
                    return NotFound(new { message = "Gider bulunamadı" });

                if (expense.IsPaid)
                    return BadRequest(new { message = "Gider zaten ödenmiş" });

                expense.IsPaid = true;
                expense.PaymentDate = dto.PaymentDate;
                expense.ModifiedDate = DateTime.Now;

                _unitOfWork.Expenses.Update(expense);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Gider ödendi: {ExpenseId}", id);
                return Ok(new { message = "Gider ödendi olarak işaretlendi", expense });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gider ödeme işlemi sırasında hata");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete expense (Muhasebe only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Muhasebe,SubeYoneticisi")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
                if (expense == null)
                    return NotFound(new { message = "Gider bulunamadı" });

                await _unitOfWork.Expenses.SoftDeleteAsync(expense);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Gider silindi: {ExpenseId}", id);
                return Ok(new { message = "Gider başarıyla silindi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gider silinirken hata");
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // DTOs
    public class ExpenseCreateDto
    {
        public int StoreId { get; set; }
        public ExpenseType ExpenseType { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public decimal ExchangeRate { get; set; } = 1.0m;
        public DateTime ExpenseDate { get; set; }
    }

    public class PaymentDto
    {
        public DateTime PaymentDate { get; set; }
    }
}
