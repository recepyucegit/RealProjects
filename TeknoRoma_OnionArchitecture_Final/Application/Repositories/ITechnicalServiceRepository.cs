using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories
{
    public interface ITechnicalServiceRepository : IRepository<TechnicalService>
    {
        Task<TechnicalService?> GetByServiceNumberAsync(string serviceNumber);
        Task<IReadOnlyList<TechnicalService>> GetByStatusAsync(TechnicalServiceStatus status);
        Task<IReadOnlyList<TechnicalService>> GetByStoreAsync(int storeId);
        Task<IReadOnlyList<TechnicalService>> GetByAssignedEmployeeAsync(int employeeId);
        Task<IReadOnlyList<TechnicalService>> GetByCustomerAsync(int customerId);
        Task<IReadOnlyList<TechnicalService>> GetOpenIssuesAsync();
        Task<IReadOnlyList<TechnicalService>> GetUnassignedAsync();
        Task<int> GetOpenIssuesCountAsync();
        Task<string> GenerateServiceNumberAsync();
    }
}
