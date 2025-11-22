using Domain.Entities;

namespace Application.Services
{
    public interface ICustomerService
    {
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer?> GetByIdentityNumberAsync(string identityNumber);
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<IEnumerable<Customer>> GetActiveCustomersAsync();
        Task<IEnumerable<Customer>> SearchAsync(string searchTerm);
        Task<IEnumerable<Customer>> GetTopCustomersAsync(int count, DateTime? startDate = null, DateTime? endDate = null);
        Task<Customer> CreateAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(int id);
        Task<bool> IsIdentityNumberTakenAsync(string identityNumber, int? excludeId = null);
    }
}
