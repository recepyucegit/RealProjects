// ===================================================================================
// TEKNOROMA - CALISAN REPOSITORY IMPLEMENTASYONU (EmployeeRepository.cs)
// ===================================================================================

using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Calisan Repository Implementasyonu
    /// </summary>
    public class EmployeeRepository : EfRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// TC Kimlik ile calisan getir
        /// </summary>
        public async Task<Employee?> GetByIdentityNumberAsync(string identityNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(e => e.IdentityNumber == identityNumber);
        }

        /// <summary>
        /// Identity UserId ile calisan getir (Login sonrasi)
        /// </summary>
        public async Task<Employee?> GetByIdentityUserIdAsync(string identityUserId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(e => e.IdentityUserId == identityUserId);
        }

        /// <summary>
        /// Magaza calisanlari
        /// </summary>
        public async Task<IReadOnlyList<Employee>> GetByStoreAsync(int storeId)
        {
            return await _dbSet
                .Where(e => e.StoreId == storeId)
                .ToListAsync();
        }

        /// <summary>
        /// Departman calisanlari
        /// </summary>
        public async Task<IReadOnlyList<Employee>> GetByDepartmentAsync(int departmentId)
        {
            return await _dbSet
                .Where(e => e.DepartmentId == departmentId)
                .ToListAsync();
        }

        /// <summary>
        /// Role gore calisanlar
        /// </summary>
        public async Task<IReadOnlyList<Employee>> GetByRoleAsync(UserRole role)
        {
            return await _dbSet
                .Where(e => e.Role == role)
                .ToListAsync();
        }

        /// <summary>
        /// Aktif calisanlar
        /// </summary>
        public async Task<IReadOnlyList<Employee>> GetActiveEmployeesAsync()
        {
            return await _dbSet
                .Where(e => e.IsActive)
                .ToListAsync();
        }

        /// <summary>
        /// En cok satan personel
        /// </summary>
        public async Task<IReadOnlyList<Employee>> GetTopSellersAsync(int count, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Sales.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(s => s.SaleDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(s => s.SaleDate <= endDate.Value);

            var topEmployeeIds = await query
                .GroupBy(s => s.EmployeeId)
                .OrderByDescending(g => g.Sum(s => s.TotalAmount))
                .Take(count)
                .Select(g => g.Key)
                .ToListAsync();

            return await _dbSet
                .Where(e => topEmployeeIds.Contains(e.Id))
                .ToListAsync();
        }
    }
}
