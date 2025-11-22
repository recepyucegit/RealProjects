using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Repositories
{
    /// <summary>
    /// Generic Repository Interface
    /// Tüm entity'ler için ortak CRUD operasyonlarını tanımlar
    /// </summary>
    public interface IRepository<T> where T : BaseEntity
    {
        // ====== QUERY (READ) ======
        Task<T?> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> GetAllAsync(Expression<Func<T, bool>> predicate);
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        // ====== COMMAND (WRITE) ======
        Task<T> AddAsync(T entity);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task SoftDeleteAsync(T entity);
    }

    /// <summary>
    /// Simple Repository - Basit entity'ler için (Category, Supplier, Store, Department)
    /// </summary>
    public interface ISimpleRepository<T> : IRepository<T> where T : BaseEntity
    {
    }
}
