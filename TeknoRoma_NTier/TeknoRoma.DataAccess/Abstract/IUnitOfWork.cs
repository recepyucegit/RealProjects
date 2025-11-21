using TeknoRoma.Entities;

namespace TeknoRoma.DataAccess.Abstract;

/// <summary>
/// Unit of Work Interface
/// Birden fazla repository işlemini tek bir transaction içinde yönetir
///
/// NEDEN Unit of Work?
/// - Birden fazla entity işlemini TEK bir transaction'da yönetir
/// - ACID prensiplerini sağlar (Atomicity, Consistency, Isolation, Durability)
/// - Örnek: Satış + Satış Detayları + Stok Güncelleme işlemi atomik olmalı
/// - Hata olursa TÜMÜ geri alınır (rollback)
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // ====== REPOSITORY PROPERTIES ======
    // Her entity için repository erişimi
    // NEDEN Lazy Initialization? İhtiyaç olduğunda oluşturulur, performans artar

    /// <summary>
    /// Mağaza/Şube repository'si - 55 mağaza
    /// </summary>
    IRepository<Store> Stores { get; }

    /// <summary>
    /// Departman repository'si - 30 departman
    /// </summary>
    IRepository<Department> Departments { get; }

    /// <summary>
    /// Çalışan repository'si - 258 çalışan
    /// </summary>
    IRepository<Employee> Employees { get; }

    /// <summary>
    /// Kategori repository'si
    /// </summary>
    IRepository<Category> Categories { get; }

    /// <summary>
    /// Ürün repository'si
    /// </summary>
    IRepository<Product> Products { get; }

    /// <summary>
    /// Tedarikçi repository'si
    /// </summary>
    IRepository<Supplier> Suppliers { get; }

    /// <summary>
    /// Müşteri repository'si
    /// </summary>
    IRepository<Customer> Customers { get; }

    /// <summary>
    /// Satış repository'si (Order yerine Sale)
    /// </summary>
    IRepository<Sale> Sales { get; }

    /// <summary>
    /// Satış Detayı repository'si
    /// </summary>
    IRepository<SaleDetail> SaleDetails { get; }

    /// <summary>
    /// Gider repository'si - Muhasebe için
    /// </summary>
    IRepository<Expense> Expenses { get; }

    /// <summary>
    /// Teknik Servis repository'si
    /// </summary>
    IRepository<TechnicalService> TechnicalServices { get; }


    // ====== TRANSACTION METHODS ======

    /// <summary>
    /// Tüm değişiklikleri veritabanına kaydeder (Commit)
    /// Başarılı ise true döner, hata varsa false döner
    /// KULLANIM: await _unitOfWork.SaveChangesAsync();
    /// </summary>
    Task<bool> SaveChangesAsync();

    /// <summary>
    /// Tüm değişiklikleri veritabanına kaydeder ve kayıt sayısını döner
    /// Etkilenen satır sayısını döner
    /// </summary>
    Task<int> CommitAsync();

    /// <summary>
    /// Transaction başlatır
    /// Birden fazla işlemi tek bir transaction içinde yapmak için kullanılır
    ///
    /// ÖRNEK KULLANIM:
    /// await _unitOfWork.BeginTransactionAsync();
    /// try {
    ///     // Satış ekle
    ///     await _unitOfWork.Sales.AddAsync(sale);
    ///     // Stok azalt
    ///     product.Stock -= quantity;
    ///     _unitOfWork.Products.Update(product);
    ///     // Commit
    ///     await _unitOfWork.CommitTransactionAsync();
    /// } catch {
    ///     await _unitOfWork.RollbackTransactionAsync();
    /// }
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Transaction'ı commit eder (onaylar)
    /// Tüm işlemler başarılı olduysa çağrılır
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Transaction'ı rollback eder (geri alır)
    /// Hata durumunda tüm işlemler geri alınır
    /// NEDEN? Veritabanı tutarlılığını korumak için
    /// </summary>
    Task RollbackTransactionAsync();
}
