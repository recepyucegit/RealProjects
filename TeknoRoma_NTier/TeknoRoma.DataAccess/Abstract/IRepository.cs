using System.Linq.Expressions;
using TeknoRoma.Entities;

namespace TeknoRoma.DataAccess.Abstract;

/// <summary>
/// Generic Repository Interface
/// Tüm entity'ler için ortak CRUD işlemlerini tanımlar
/// T: BaseEntity'den türeyen herhangi bir entity olabilir
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    // QUERY METHODS (Sorgulama İşlemleri)

    /// <summary>
    /// ID'ye göre tek bir entity getirir
    /// </summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Tüm entity'leri getirir (soft delete filtreleriyle)
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Belirli bir koşula uyan entity'leri getirir
    /// Örnek kullanım: Find(p => p.Price > 100 && p.IsActive == true)
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Belirli bir koşula uyan tek bir entity getirir
    /// </summary>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Belirli bir koşula uyan entity var mı kontrol eder
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Koşula uyan entity sayısını döner
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    /// <summary>
    /// IQueryable olarak entity'leri döner - Daha karmaşık sorgular için
    /// Linq metodlarıyla zincirleme yapılabilir
    /// </summary>
    IQueryable<T> GetQueryable();

    // COMMAND METHODS (Veri İşlemleri)

    /// <summary>
    /// Yeni entity ekler
    /// SaveChanges çağrılana kadar veritabanına yazılmaz
    /// </summary>
    Task AddAsync(T entity);

    /// <summary>
    /// Birden fazla entity ekler
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Entity'yi günceller
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Birden fazla entity'yi günceller
    /// </summary>
    void UpdateRange(IEnumerable<T> entities);

    /// <summary>
    /// Entity'yi fiziksel olarak siler (Hard Delete)
    /// Dikkat: Veri kalıcı olarak silinir!
    /// </summary>
    void Delete(T entity);

    /// <summary>
    /// Birden fazla entity'yi fiziksel olarak siler
    /// </summary>
    void DeleteRange(IEnumerable<T> entities);

    /// <summary>
    /// Entity'yi mantıksal olarak siler (Soft Delete)
    /// IsDeleted flag'i true yapılır, fiziksel olarak silinmez
    /// </summary>
    void SoftDelete(T entity);

    /// <summary>
    /// Birden fazla entity'yi mantıksal olarak siler
    /// </summary>
    void SoftDeleteRange(IEnumerable<T> entities);
}
