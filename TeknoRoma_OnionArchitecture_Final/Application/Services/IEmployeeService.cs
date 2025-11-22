using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public interface IEmployeeService
    {
        Task<Employee?> GetByIdAsync(int id);
        Task<Employee?> GetByIdentityNumberAsync(string identityNumber);
        Task<Employee?> GetByIdentityUserIdAsync(string identityUserId);
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<IEnumerable<Employee>> GetByStoreAsync(int storeId);
        Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId);
        Task<IEnumerable<Employee>> GetByRoleAsync(UserRole role);
        Task<IEnumerable<Employee>> GetActiveEmployeesAsync();
        Task<IEnumerable<Employee>> GetTopSellersAsync(int count, DateTime? startDate = null, DateTime? endDate = null);
        Task<Employee> CreateAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task DeleteAsync(int id);
        Task<bool> IsIdentityNumberTakenAsync(string identityNumber, int? excludeId = null);
    }
}
