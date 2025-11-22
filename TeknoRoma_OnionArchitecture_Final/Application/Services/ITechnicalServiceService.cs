using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public interface ITechnicalServiceService
    {
        Task<TechnicalService?> GetByIdAsync(int id);
        Task<TechnicalService?> GetByServiceNumberAsync(string serviceNumber);
        Task<IEnumerable<TechnicalService>> GetAllAsync();
        Task<IEnumerable<TechnicalService>> GetByStatusAsync(TechnicalServiceStatus status);
        Task<IEnumerable<TechnicalService>> GetByStoreAsync(int storeId);
        Task<IEnumerable<TechnicalService>> GetByAssignedEmployeeAsync(int employeeId);
        Task<IEnumerable<TechnicalService>> GetOpenIssuesAsync();
        Task<IEnumerable<TechnicalService>> GetUnassignedAsync();
        Task<TechnicalService> CreateAsync(TechnicalService technicalService);
        Task UpdateAsync(TechnicalService technicalService);
        Task AssignToEmployeeAsync(int serviceId, int employeeId);
        Task UpdateStatusAsync(int serviceId, TechnicalServiceStatus status, string? resolution = null);
        Task<int> GetOpenIssuesCountAsync();
    }
}
