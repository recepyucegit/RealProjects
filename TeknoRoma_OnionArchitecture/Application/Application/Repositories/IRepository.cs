using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Repositories
{
    /// <summary>
    /// Generic Repository Interface
    /// Tüm entity'ler için ortak CRUD operasyonlarını tanımlar
    /// 
    /// NEDEN Generic?
    /// - Her entity için ayrı interface yazmaktan kurtarır
    /// - Product, Category, Supplier hepsi aynı temel işlemleri kullanır
    /// 
    /// NEDEN Interface?
    /// - Loose Coupling (Gevşek bağımlılık)
    /// - Test edilebilirlik (Mock repository oluşturulabilir)
    /// - Dependency Inversion Principle (SOLID'in D'si)
    /// </summary>
    /// <typeparam name="T">BaseEntity'den türeyen herhangi bir entity</typeparam>
    public interface IRepository<T> where T : BaseEntity
    {
        // ====== QUERY (READ) OPERATIONS ======

        /// <summary>
        /// ID'ye göre tek bir kayıt getirir
        /// ASYNC: Database işlemi asenkron yapılır (UI donmaz)
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>Bulunan entity veya null</returns>
        Task<T> GetByIdAsync(int id);

        /// <summary>
        /// Tüm kayıtları getirir
        /// NEDEN IReadOnlyList?
        /// - List<T> yerine IReadOnlyList<T> kullanıyoruz
        /// - Dönen listeye ekleme/çıkarma yapılamaz (güvenlik)
        /// - Sadece okuma amaçlı
        /// </summary>
        Task<IReadOnlyList<T>> GetAllAsync();

        /// <summary>
        /// Koşula göre kayıtları getirir
        /// NEDEN Expression?
        /// - Lambda ifadeleri ile kullanılır
        /// - Örn: GetAllAsync(p => p.UnitPrice > 1000)
        /// - Filtreleme işlemleri için
        /// </summary>
        /// <param name="predicate">Filtreleme koşulu (lambda expression)</param>
        Task<IReadOnlyList<T>> GetAllAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// İlişkili tabloları dahil ederek kayıt getirir (Eager Loading)
        /// NEDEN Include?
        /// - Product getirirken Category ve Supplier bilgilerini de getirmek için
        /// - Örn: GetByIdWithIncludesAsync(1, p => p.Category, p => p.Supplier)
        /// - N+1 Query problemini önler
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <param name="includes">Dahil edilecek navigation property'ler</param>
        Task<T> GetByIdWithIncludesAsync(int id, params Expression<Func<T, object>>[] includes);

        /// <summary>
        /// Koşula göre tek bir kayıt getirir (FirstOrDefault)
        /// </summary>
        Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Kayıt var mı kontrolü
        /// </summary>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Toplam kayıt sayısı
        /// </summary>
        Task<int> CountAsync();

        /// <summary>
        /// Koşula göre kayıt sayısı
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);


        // ====== COMMAND (WRITE) OPERATIONS ======

        /// <summary>
        /// Yeni kayıt ekler
        /// NEDEN Task<T> dönüyor?
        /// - Eklenen entity'yi geri döndürür
        /// - Database'de otomatik oluşturulan ID'yi alabilmek için
        /// </summary>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Birden fazla kayıt ekler
        /// NEDEN Bulk Insert?
        /// - Performance: Tek tek eklemek yerine toplu ekleme
        /// - Transaction içinde yapılır (hepsi başarılı veya hiçbiri)
        /// </summary>
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Kaydı günceller
        /// NEDEN void değil de Task?
        /// - Async operation olduğu için Task dönmeli
        /// - SaveChangesAsync'i beklemek için
        /// </summary>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Birden fazla kaydı günceller
        /// </summary>
        Task UpdateRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Kaydı siler (Fiziksel silme)
        /// ÖNEMLİ: Genellikle Soft Delete tercih edilir
        /// </summary>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Kaydı soft delete yapar (IsDeleted = true)
        /// NEDEN Soft Delete?
        /// - Veri kaybı olmaz
        /// - Raporlarda geçmiş verilere erişim
        /// - Yanlışlıkla silinen kayıtları geri getirme
        /// </summary>
        Task SoftDeleteAsync(T entity);

        /// <summary>
        /// Birden fazla kaydı siler
        /// </summary>
        Task DeleteRangeAsync(IEnumerable<T> entities);
    }

    /// <summary>
    /// Simple Repository Interface
    /// Generic Repository için alias - özel iş mantığı gerektirmeyen entity'ler için
    /// NEDEN?
    /// - UnitOfWork'te basit entity'ler için IRepository<T> yerine daha okunabilir bir isim
    /// - Category, Supplier, Store, Department gibi entity'ler için
    /// </summary>
    public interface ISimpleRepository<T> : IRepository<T> where T : BaseEntity
    {
        // IRepository'den tüm metodları miras alır
        // Ekstra metod gerekmez - sadece isim değişikliği için
    }
}