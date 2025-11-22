using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        Task<Employee?> GetByIdentityNumberAsync(string identityNumber);
        Task<Employee?> GetByIdentityUserIdAsync(string identityUserId);
        Task<IReadOnlyList<Employee>> GetByStoreAsync(int storeId);
        Task<IReadOnlyList<Employee>> GetByDepartmentAsync(int departmentId);
        Task<IReadOnlyList<Employee>> GetByRoleAsync(UserRole role);
        Task<IReadOnlyList<Employee>> GetActiveEmployeesAsync();
        Task<IReadOnlyList<Employee>> GetTopSellersAsync(int count, DateTime? startDate = null, DateTime? endDate = null);
    }
}
