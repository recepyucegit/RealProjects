using Domain.Entities;

namespace Application.Repositories
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer?> GetByIdentityNumberAsync(string identityNumber);
        Task<IReadOnlyList<Customer>> GetActiveCustomersAsync();
        Task<IReadOnlyList<Customer>> SearchByNameAsync(string searchTerm);
        Task<IReadOnlyList<Customer>> GetTopCustomersAsync(int count, DateTime? startDate = null, DateTime? endDate = null);
    }
}
