using TeknoRoma.Entities;

namespace TeknoRoma.DataAccess.Abstract;

/// <summary>
/// Unit of Work Interface
/// Birden fazla repository işlemini tek bir transaction içinde yönetir
/// Transaction koordinasyonu ve atomicity sağlar
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Repository Properties - Her entity için repository erişimi
    IRepository<Category> Categories { get; }
    IRepository<Product> Products { get; }
    IRepository<Supplier> Suppliers { get; }
    IRepository<Customer> Customers { get; }
    IRepository<Order> Orders { get; }
    IRepository<OrderDetail> OrderDetails { get; }

    /// <summary>
    /// Tüm değişiklikleri veritabanına kaydeder (Commit)
    /// Başarılı ise true döner, hata varsa false döner
    /// </summary>
    Task<bool> SaveChangesAsync();

    /// <summary>
    /// Tüm değişiklikleri veritabanına kaydeder ve kayıt sayısını döner
    /// </summary>
    Task<int> CommitAsync();

    /// <summary>
    /// Transaction başlatır
    /// Birden fazla işlemi tek bir transaction içinde yapmak için kullanılır
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Transaction'ı commit eder (onaylar)
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Transaction'ı rollback eder (geri alır)
    /// Hata durumunda tüm işlemler geri alınır
    /// </summary>
    Task RollbackTransactionAsync();
}
