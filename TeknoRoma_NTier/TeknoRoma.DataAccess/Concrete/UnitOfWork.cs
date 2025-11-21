using Microsoft.EntityFrameworkCore.Storage;
using TeknoRoma.DataAccess.Abstract;
using TeknoRoma.DataAccess.Context;
using TeknoRoma.Entities;

namespace TeknoRoma.DataAccess.Concrete;

/// <summary>
/// Unit of Work Implementation
/// Transaction yönetimi ve repository koordinasyonu yapar
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly TeknoRomaDbContext _context;
    private IDbContextTransaction? _transaction;

    // Repository instance'ları - Lazy initialization için nullable
    private IRepository<Category>? _categories;
    private IRepository<Product>? _products;
    private IRepository<Supplier>? _suppliers;
    private IRepository<Customer>? _customers;
    private IRepository<Order>? _orders;
    private IRepository<OrderDetail>? _orderDetails;

    /// <summary>
    /// Constructor - DbContext'i dependency injection ile alır
    /// </summary>
    public UnitOfWork(TeknoRomaDbContext context)
    {
        _context = context;
    }

    // Repository Properties - Lazy initialization pattern
    // İlk erişimde oluşturulur, sonraki erişimlerde aynı instance kullanılır

    public IRepository<Category> Categories
    {
        get
        {
            // Null ise yeni instance oluştur, değilse mevcut instance'ı döndür
            return _categories ??= new Repository<Category>(_context);
        }
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

    public IRepository<Order> Orders
    {
        get { return _orders ??= new Repository<Order>(_context); }
    }

    public IRepository<OrderDetail> OrderDetails
    {
        get { return _orderDetails ??= new Repository<OrderDetail>(_context); }
    }

    /// <summary>
    /// Tüm değişiklikleri veritabanına kaydeder
    /// Başarılı ise true, hata varsa false döner
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
    /// </summary>
    public async Task<int> CommitAsync()
    {
        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Transaction başlatır
    /// Örnek kullanım: Sipariş + Sipariş Detayları ekleme işlemini tek transaction'da yapmak
    /// </summary>
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    /// <summary>
    /// Transaction'ı commit eder (onaylar)
    /// Tüm işlemler başarılı olduysa çağrılır
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
    /// </summary>
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
