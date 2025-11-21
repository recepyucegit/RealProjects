using Microsoft.EntityFrameworkCore.Storage;
using TeknoRoma.DataAccess.Abstract;
using TeknoRoma.DataAccess.Context;
using TeknoRoma.Entities;

namespace TeknoRoma.DataAccess.Concrete;

/// <summary>
/// Unit of Work Implementation
/// Transaction yönetimi ve repository koordinasyonu yapar
///
/// NEDEN Bu Kadar Önemli?
/// - Birden fazla repository işlemini tek transaction'da toplar
/// - SaveChangesAsync tek noktadan çağrılır
/// - Transaction yönetimi (Begin, Commit, Rollback)
/// - Lazy Loading pattern ile performans optimizasyonu
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly TeknoRomaDbContext _context;
    private IDbContextTransaction? _transaction;

    // ====== REPOSITORY INSTANCE'LARI ======
    // Lazy initialization için nullable
    // İlk erişimde oluşturulur, sonrasında cache'ten döner

    private IRepository<Store>? _stores;
    private IRepository<Department>? _departments;
    private IRepository<Employee>? _employees;
    private IRepository<Category>? _categories;
    private IRepository<Product>? _products;
    private IRepository<Supplier>? _suppliers;
    private IRepository<Customer>? _customers;
    private IRepository<Sale>? _sales;
    private IRepository<SaleDetail>? _saleDetails;
    private IRepository<Expense>? _expenses;
    private IRepository<TechnicalService>? _technicalServices;

    /// <summary>
    /// Constructor - DbContext'i dependency injection ile alır
    /// </summary>
    public UnitOfWork(TeknoRomaDbContext context)
    {
        _context = context;
    }


    // ====== REPOSITORY PROPERTIES ======
    // Lazy initialization pattern
    // NEDEN? İhtiyaç olduğunda oluşturulur, gereksiz yere memory kullanmaz

    public IRepository<Store> Stores
    {
        get { return _stores ??= new Repository<Store>(_context); }
    }

    public IRepository<Department> Departments
    {
        get { return _departments ??= new Repository<Department>(_context); }
    }

    public IRepository<Employee> Employees
    {
        get { return _employees ??= new Repository<Employee>(_context); }
    }

    public IRepository<Category> Categories
    {
        get { return _categories ??= new Repository<Category>(_context); }
    }

    public IRepository<Product> Products
    {
        get { return _products ??= new Repository<Product>(_context); }
    }

    public IRepository<Supplier> Suppliers
    {
        get { return _suppliers ??= new Repository<Supplier>(_context); }
    }

    public IRepository<Customer> Customers
    {
        get { return _customers ??= new Repository<Customer>(_context); }
    }

    public IRepository<Sale> Sales
    {
        get { return _sales ??= new Repository<Sale>(_context); }
    }

    public IRepository<SaleDetail> SaleDetails
    {
        get { return _saleDetails ??= new Repository<SaleDetail>(_context); }
    }

    public IRepository<Expense> Expenses
    {
        get { return _expenses ??= new Repository<Expense>(_context); }
    }

    public IRepository<TechnicalService> TechnicalServices
    {
        get { return _technicalServices ??= new Repository<TechnicalService>(_context); }
    }


    // ====== TRANSACTION METHODS ======

    /// <summary>
    /// Tüm değişiklikleri veritabanına kaydeder
    /// Başarılı ise true, hata varsa false döner
    ///
    /// KULLANIM:
    /// if (await _unitOfWork.SaveChangesAsync())
    ///     return Ok("Başarılı");
    /// else
    ///     return BadRequest("Hata");
    /// </summary>
    public async Task<bool> SaveChangesAsync()
    {
        try
        {
            // SaveChangesAsync, etkilenen satır sayısını döner
            // 0'dan büyükse başarılıdır
            return await _context.SaveChangesAsync() > 0;
        }
        catch
        {
            // Hata durumunda false döner
            // Loglama burada yapılabilir
            return false;
        }
    }

    /// <summary>
    /// Tüm değişiklikleri veritabanına kaydeder ve etkilenen satır sayısını döner
    ///
    /// KULLANIM:
    /// int affectedRows = await _unitOfWork.CommitAsync();
    /// </summary>
    public async Task<int> CommitAsync()
    {
        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Transaction başlatır
    ///
    /// ÖRNEK KULLANIM:
    /// // Satış işlemi: Satış ekle + Stok azalt + Gider kaydet
    /// await _unitOfWork.BeginTransactionAsync();
    /// try
    /// {
    ///     // 1. Satış ekle
    ///     await _unitOfWork.Sales.AddAsync(sale);
    ///     await _unitOfWork.SaveChangesAsync();
    ///
    ///     // 2. Satış detaylarını ekle ve stokları azalt
    ///     foreach (var detail in saleDetails)
    ///     {
    ///         await _unitOfWork.SaleDetails.AddAsync(detail);
    ///         var product = await _unitOfWork.Products.GetByIdAsync(detail.ProductId);
    ///         product.Stock -= detail.Quantity;
    ///         _unitOfWork.Products.Update(product);
    ///     }
    ///
    ///     // 3. Tümünü commit et
    ///     await _unitOfWork.CommitTransactionAsync();
    /// }
    /// catch
    /// {
    ///     // Hata olursa tümünü geri al
    ///     await _unitOfWork.RollbackTransactionAsync();
    ///     throw;
    /// }
    /// </summary>
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    /// <summary>
    /// Transaction'ı commit eder (onaylar)
    /// Tüm işlemler başarılı olduysa çağrılır
    ///
    /// NEDEN?
    /// - Atomicity sağlar (Ya hepsi başarılı, ya hiçbiri)
    /// - Consistency garantisi (Veritabanı tutarlı kalır)
    /// </summary>
    public async Task CommitTransactionAsync()
    {
        try
        {
            // Önce değişiklikleri kaydet
            await _context.SaveChangesAsync();

            // Sonra transaction'ı commit et
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        catch
        {
            // Hata durumunda rollback yap
            await RollbackTransactionAsync();
            throw; // Exception'ı üst katmana fırlat
        }
    }

    /// <summary>
    /// Transaction'ı rollback eder (geri alır)
    /// Hata durumunda tüm işlemler geri alınır
    ///
    /// NEDEN?
    /// - Hata durumunda veritabanını tutarlı tutmak için
    /// - Örnek: Satış eklendi ama stok güncellenemedi → Satışı da geri al
    /// </summary>
    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// Dispose pattern implementation
    /// Resources'ları temizler
    ///
    /// NEDEN?
    /// - Memory leak'leri önler
    /// - Database bağlantılarını kapatır
    /// - IDisposable pattern gereği
    /// </summary>
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
