using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TeknoRoma.DataAccess.Abstract;
using TeknoRoma.DataAccess.Context;
using TeknoRoma.Entities;

namespace TeknoRoma.DataAccess.Concrete;

/// <summary>
/// Generic Repository Implementation
/// IRepository interface'ini implemente eder ve tüm CRUD işlemlerini EF Core ile gerçekleştirir
/// </summary>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly TeknoRomaDbContext _context;
    private readonly DbSet<T> _dbSet;

    /// <summary>
    /// Constructor - DbContext'i dependency injection ile alır
    /// </summary>
    public Repository(TeknoRomaDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>(); // T tipindeki DbSet'i alır
    }

    // QUERY IMPLEMENTATIONS

    public async Task<T?> GetByIdAsync(int id)
    {
        // AsNoTracking: Sadece okuma için, tracking overhead'ini azaltır
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        // Global query filter otomatik olarak IsDeleted = false olanları getirir
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        // Lambda expression ile dinamik sorgulama
        // Örnek: Find(p => p.Price > 100)
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate == null)
            return await _dbSet.CountAsync();

        return await _dbSet.CountAsync(predicate);
    }

    public IQueryable<T> GetQueryable()
    {
        // Karmaşık sorgular için IQueryable döner
        // Örnek: repo.GetQueryable().Include(p => p.Category).Where(p => p.IsActive).OrderBy(p => p.Name)
        return _dbSet.AsQueryable();
    }

    // COMMAND IMPLEMENTATIONS

    public async Task AddAsync(T entity)
    {
        // Entity'yi DbContext'e ekler (henüz veritabanına yazılmaz)
        await _dbSet.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        // Birden fazla entity'yi toplu olarak ekler
        await _dbSet.AddRangeAsync(entities);
    }

    public void Update(T entity)
    {
        // Entity'yi modified olarak işaretler
        _dbSet.Update(entity);
        // veya: _context.Entry(entity).State = EntityState.Modified;
    }

    public void UpdateRange(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    public void Delete(T entity)
    {
        // HARD DELETE - Fiziksel olarak siler
        // Dikkat: Geri getirilemez!
        _dbSet.Remove(entity);
    }

    public void DeleteRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public void SoftDelete(T entity)
    {
        // SOFT DELETE - IsDeleted flag'ini true yapar
        // Fiziksel olarak silinmez, sadece mantıksal olarak silinmiş olur
        entity.IsDeleted = true;
        entity.UpdatedDate = DateTime.Now;
        Update(entity);
    }

    public void SoftDeleteRange(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            entity.IsDeleted = true;
            entity.UpdatedDate = DateTime.Now;
        }
        UpdateRange(entities);
    }
}
