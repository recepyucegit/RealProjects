using Domain.Entities;

namespace Application.Services
{
    public interface ISupplierTransactionService
    {
        Task<SupplierTransaction?> GetByIdAsync(int id);
        Task<SupplierTransaction?> GetByTransactionNumberAsync(string transactionNumber);
        Task<IEnumerable<SupplierTransaction>> GetAllAsync();
        Task<IEnumerable<SupplierTransaction>> GetBySupplierAsync(int supplierId);
        Task<IEnumerable<SupplierTransaction>> GetByProductAsync(int productId);
        Task<IEnumerable<SupplierTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<SupplierTransaction>> GetUnpaidTransactionsAsync();
        Task<SupplierTransaction> CreateAsync(SupplierTransaction transaction);
        Task UpdateAsync(SupplierTransaction transaction);
        Task DeleteAsync(int id);
        Task MarkAsPaidAsync(int transactionId);
        Task<decimal> GetMonthlyTotalAsync(int year, int month);
    }
}
