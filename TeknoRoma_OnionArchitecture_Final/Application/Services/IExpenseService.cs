using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public interface IExpenseService
    {
        Task<Expense?> GetByIdAsync(int id);
        Task<Expense?> GetByExpenseNumberAsync(string expenseNumber);
        Task<IEnumerable<Expense>> GetAllAsync();
        Task<IEnumerable<Expense>> GetByStoreAsync(int storeId);
        Task<IEnumerable<Expense>> GetByTypeAsync(ExpenseType expenseType);
        Task<IEnumerable<Expense>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Expense>> GetUnpaidExpensesAsync();
        Task<Expense> CreateAsync(Expense expense);
        Task UpdateAsync(Expense expense);
        Task DeleteAsync(int id);
        Task MarkAsPaidAsync(int expenseId);
        Task<decimal> GetMonthlyTotalAsync(int year, int month, int? storeId = null);
    }
}
