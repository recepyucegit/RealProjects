// ===================================================================================
// TEKNOROMA - GENERIC REPOSITORY IMPLEMENTASYONU (EfRepository.cs)
// ===================================================================================
//
// BU DOSYANIN AMACI
// ===================================================================================
// IRepository<T> interface'inin Entity Framework Core implementasyonu.
// Tum entity'ler icin ortak CRUD operasyonlarini saglar.
//
// GENERIC REPOSITORY PATTERN
// ===================================================================================
// - T tipi ile herhangi bir entity icin calisir
// - DbContext ve DbSet kullanarak veritabani islemleri yapar
// - IRepository<T> interface'ini implement eder
//
// ===================================================================================

using System.Linq.Expressions;
using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Generic Repository Implementasyonu
    ///
    /// EF Core ile veritabani islemleri
    /// Tum entity'ler icin ortak CRUD operasyonlari
    /// </summary>
    /// <typeparam name="T">BaseEntity'den tureyen entity tipi</typeparam>
    public class EfRepository<T> : IRepository<T> where T : BaseEntity
    {
        // =================================================================
        // FIELD'LAR
        // =================================================================

        /// <summary>
        /// Veritabani baglami
        /// Protected: Alt siniflar (ProductRepository vb.) erisebilir
        /// </summary>
        protected readonly AppDbContext _context;

        /// <summary>
        /// Entity'nin DbSet'i
        /// T tipine karsilik gelen tablo
        /// </summary>
        protected readonly DbSet<T> _dbSet;

        // =================================================================
        // CONSTRUCTOR
        // =================================================================

        /// <summary>
        /// Repository Constructor
        ///
        /// DI ile AppDbContext inject edilir
        /// </summary>
        public EfRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        // =================================================================
        // QUERY (READ) OPERASYONLARI
        // =================================================================

        /// <summary>
        /// ID'ye gore tekil kayit getir
        /// </summary>
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Tum kayitlari getir (soft delete filtreli)
        /// </summary>
        public virtual async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Kosula gore filtrelenmiş kayitlari getir
        /// </summary>
        public virtual async Task<IReadOnlyList<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        /// <summary>
        /// Kosula uyan ilk kaydi getir
        /// </summary>
        public virtual async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// Kosula uyan kayit var mi kontrolu
        /// </summary>
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        /// <summary>
        /// Toplam kayit sayisi
        /// </summary>
        public virtual async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        /// <summary>
        /// Kosula gore kayit sayisi
        /// </summary>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        // =================================================================
        // COMMAND (WRITE) OPERASYONLARI
        // =================================================================

        /// <summary>
        /// Yeni kayit ekle
        /// </summary>
        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        /// <summary>
        /// Toplu kayit ekleme
        /// </summary>
        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            return entities;
        }

        /// <summary>
        /// Mevcut kaydi guncelle
        /// </summary>
        public virtual Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Kalici silme (Hard Delete)
        /// </summary>
        public virtual Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Yumusak silme (Soft Delete)
        /// IsDeleted = true, DeletedDate = DateTime.Now
        /// </summary>
        public virtual Task SoftDeleteAsync(T entity)
        {
            entity.IsDeleted = true;
            entity.DeletedDate = DateTime.Now;
            _context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }
    }
}
