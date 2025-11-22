using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public interface ISaleService
    {
        Task<Sale?> GetByIdAsync(int id);
        Task<Sale?> GetBySaleNumberAsync(string saleNumber);
        Task<IEnumerable<Sale>> GetAllAsync();
        Task<IEnumerable<Sale>> GetByCustomerAsync(int customerId);
        Task<IEnumerable<Sale>> GetByEmployeeAsync(int employeeId);
        Task<IEnumerable<Sale>> GetByStoreAsync(int storeId);
        Task<IEnumerable<Sale>> GetByStatusAsync(SaleStatus status);
        Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Sale> CreateAsync(Sale sale, IEnumerable<SaleDetail> details);
        Task UpdateStatusAsync(int saleId, SaleStatus status);
        Task CancelAsync(int saleId);
        Task<decimal> GetDailyTotalAsync(DateTime date, int? storeId = null);
        Task<decimal> GetMonthlyTotalAsync(int year, int month, int? storeId = null);
    }
}
