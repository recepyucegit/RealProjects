using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories
{
    public interface IExpenseRepository : IRepository<Expense>
    {
        Task<Expense?> GetByExpenseNumberAsync(string expenseNumber);
        Task<IReadOnlyList<Expense>> GetByStoreAsync(int storeId);
        Task<IReadOnlyList<Expense>> GetByExpenseTypeAsync(ExpenseType expenseType);
        Task<IReadOnlyList<Expense>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IReadOnlyList<Expense>> GetByEmployeeAsync(int employeeId);
        Task<IReadOnlyList<Expense>> GetUnpaidExpensesAsync();
        Task<decimal> GetMonthlyTotalAsync(int year, int month, int? storeId = null);
        Task<string> GenerateExpenseNumberAsync();
    }
}
