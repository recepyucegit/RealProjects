// ===================================================================================
// TEKNOROMA - MUSTERI REPOSITORY IMPLEMENTASYONU (CustomerRepository.cs)
// ===================================================================================

using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Musteri Repository Implementasyonu
    /// </summary>
    public class CustomerRepository : EfRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// TC Kimlik ile musteri getir
        /// </summary>
        public async Task<Customer?> GetByIdentityNumberAsync(string identityNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.IdentityNumber == identityNumber);
        }

        /// <summary>
        /// Aktif musteriler
        /// </summary>
        public async Task<IReadOnlyList<Customer>> GetActiveCustomersAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        /// <summary>
        /// Isme gore arama
        /// </summary>
        public async Task<IReadOnlyList<Customer>> SearchByNameAsync(string searchTerm)
        {
            return await _dbSet
                .Where(c => c.FirstName.Contains(searchTerm) || c.LastName.Contains(searchTerm))
                .ToListAsync();
        }

        /// <summary>
        /// En cok alisveris yapan musteriler
        /// </summary>
        public async Task<IReadOnlyList<Customer>> GetTopCustomersAsync(int count, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Sales.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(s => s.SaleDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(s => s.SaleDate <= endDate.Value);

            var topCustomerIds = await query
                .Where(s => s.CustomerId.HasValue)
                .GroupBy(s => s.CustomerId!.Value)
                .OrderByDescending(g => g.Sum(s => s.TotalAmount))
                .Take(count)
                .Select(g => g.Key)
                .ToListAsync();

            return await _dbSet
                .Where(c => topCustomerIds.Contains(c.Id))
                .ToListAsync();
        }
    }
}
