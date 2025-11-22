using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories
{
    public interface ISaleRepository : IRepository<Sale>
    {
        Task<Sale?> GetBySaleNumberAsync(string saleNumber);
        Task<IReadOnlyList<Sale>> GetByCustomerAsync(int customerId);
        Task<IReadOnlyList<Sale>> GetByEmployeeAsync(int employeeId);
        Task<IReadOnlyList<Sale>> GetByStoreAsync(int storeId);
        Task<IReadOnlyList<Sale>> GetByStatusAsync(SaleStatus status);
        Task<IReadOnlyList<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetDailyTotalAsync(DateTime date, int? storeId = null);
        Task<decimal> GetMonthlyTotalAsync(int year, int month, int? storeId = null);
        Task<string> GenerateSaleNumberAsync();
    }
}
