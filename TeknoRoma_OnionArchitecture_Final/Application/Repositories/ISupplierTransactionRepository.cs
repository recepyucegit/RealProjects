using Domain.Entities;

namespace Application.Repositories
{
    public interface ISupplierTransactionRepository : IRepository<SupplierTransaction>
    {
        Task<SupplierTransaction?> GetByTransactionNumberAsync(string transactionNumber);
        Task<IReadOnlyList<SupplierTransaction>> GetBySupplierAsync(int supplierId);
        Task<IReadOnlyList<SupplierTransaction>> GetByProductAsync(int productId);
        Task<IReadOnlyList<SupplierTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IReadOnlyList<SupplierTransaction>> GetUnpaidTransactionsAsync();
        Task<decimal> GetMonthlyTotalAsync(int year, int month);
        Task<string> GenerateTransactionNumberAsync();
    }
}
