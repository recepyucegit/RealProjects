// ============================================================================
// IUnitOfWork.cs - Unit of Work Pattern Interface
// ============================================================================
// AÇIKLAMA:
// Tüm repository'leri tek bir transaction içinde yönetir.
// Veritabanı işlemlerinin tutarlılığını (consistency) sağlar.
//
// UNIT OF WORK PATTERN NEDİR?
// - Birden fazla repository işlemini tek bir transaction'da gruplar
// - "Ya hep ya hiç" (All or Nothing) prensibi
// - İş birimi (business transaction) mantığı
//
// NEDEN UNIT OF WORK?
// - Transaction yönetimi merkezileşir
// - Repository'ler arası tutarlılık sağlanır
// - Connection/Context paylaşımı yapılır
// - Tek SaveChanges ile tüm değişiklikler kaydedilir
//
// ÖRNEK SENARYO (Satış İşlemi):
// 1. Sale kaydı oluştur
// 2. SaleDetail kayıtları oluştur
// 3. Product stoklarını güncelle
// 4. Customer sadakat puanı güncelle
// -> Herhangi biri başarısız olursa HEPSİ geri alınır!
//
// IDISPOSABLE:
// - DbContext'i temizlemek için
// - using bloğu ile kullanılabilir
// - Memory leak önlenir
// ============================================================================

using Domain.Entities;

namespace Application.Repositories
{
    /// <summary>
    /// Unit of Work Pattern Interface
    ///
    /// TEK NOKTADAN VERİ ERİŞİMİ:
    /// - Service'ler UnitOfWork üzerinden repository'lere erişir
    /// - Tüm repository'ler aynı DbContext'i paylaşır
    ///
    /// DEPENDENCY INJECTION:
    /// <code>
    /// // Startup.cs
    /// services.AddScoped&lt;IUnitOfWork, UnitOfWork&gt;();
    ///
    /// // Service'te kullanım
    /// public SaleService(IUnitOfWork unitOfWork)
    /// {
    ///     _unitOfWork = unitOfWork;
    /// }
    /// </code>
    ///
    /// KULLANIM ÖRNEĞİ:
    /// <code>
    /// await _unitOfWork.Sales.AddAsync(sale);
    /// await _unitOfWork.Products.UpdateAsync(product);
    /// await _unitOfWork.SaveChangesAsync(); // Tek seferde kaydet
    /// </code>
    ///
    /// SCOPED LIFETIME:
    /// - Request başına bir instance
    /// - Request sonunda Dispose edilir
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // ====================================================================
        // BASİT REPOSITORY'LER
        // ====================================================================
        // Özel metod gerektirmeyen entity'ler için ISimpleRepository
        // CRUD işlemleri yeterli olan entity'ler
        // ====================================================================

        /// <summary>
        /// Kategori Repository
        /// Basit lookup tablosu, nadiren değişir
        /// </summary>
        ISimpleRepository<Category> Categories { get; }

        /// <summary>
        /// Tedarikçi Repository
        /// Ürün tedarikçisi ve borç takibi
        /// </summary>
        ISimpleRepository<Supplier> Suppliers { get; }

        /// <summary>
        /// Mağaza Repository
        /// Şube/mağaza yönetimi
        /// </summary>
        ISimpleRepository<Store> Stores { get; }

        /// <summary>
        /// Departman Repository
        /// Mağaza departmanları yönetimi
        /// </summary>
        ISimpleRepository<Department> Departments { get; }

        // ====================================================================
        // ÖZEL REPOSITORY'LER
        // ====================================================================
        // Domain-specific metodlara ihtiyaç duyan entity'ler
        // IRepository<T>'yi extend ederek ek metodlar ekler
        // ====================================================================

        /// <summary>
        /// Ürün Repository
        /// Özel: GetByBarcode, GetLowStock, UpdateStock
        /// </summary>
        IProductRepository Products { get; }

        /// <summary>
        /// Müşteri Repository
        /// Özel: GetByIdentityNumber, GetTopCustomers
        /// </summary>
        ICustomerRepository Customers { get; }

        /// <summary>
        /// Çalışan Repository
        /// Özel: GetByIdentityUserId, GetByRole, GetTopSellers
        /// </summary>
        IEmployeeRepository Employees { get; }

        /// <summary>
        /// Satış Repository
        /// Özel: GetBySaleNumber, GetDailyTotal, GenerateSaleNumber
        /// </summary>
        ISaleRepository Sales { get; }

        /// <summary>
        /// Satış Detay Repository
        /// Özel: GetBySale, GetTopSellingProducts
        /// </summary>
        ISaleDetailRepository SaleDetails { get; }

        /// <summary>
        /// Gider Repository
        /// Özel: GetByExpenseType, GetUnpaid, GetMonthlyTotal
        /// </summary>
        IExpenseRepository Expenses { get; }

        /// <summary>
        /// Teknik Servis Repository
        /// Özel: GetByStatus, GetOpenIssues, GetUnassigned
        /// </summary>
        ITechnicalServiceRepository TechnicalServices { get; }

        /// <summary>
        /// Tedarikçi İşlem Repository
        /// Özel: GetBySupplier, GetUnpaid, GetMonthlyTotal
        /// </summary>
        ISupplierTransactionRepository SupplierTransactions { get; }

        // ====================================================================
        // TRANSACTION YÖNETİMİ
        // ====================================================================
        // Veritabanı işlemlerinin atomik olmasını sağlar
        // Commit/Rollback kontrolü
        // ====================================================================

        /// <summary>
        /// Değişiklikleri Kaydet (Async)
        ///
        /// Tüm Add/Update/Delete işlemlerini tek seferde kaydeder
        ///
        /// ÖRNEK:
        /// <code>
        /// await _unitOfWork.Sales.AddAsync(sale);
        /// await _unitOfWork.Products.UpdateAsync(product);
        /// var affectedRows = await _unitOfWork.SaveChangesAsync();
        /// </code>
        ///
        /// DÖNÜŞ: Etkilenen satır sayısı
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Değişiklikleri Kaydet (Sync)
        /// NOT: Mümkünse async versiyon tercih edilmeli
        /// </summary>
        int SaveChanges();

        /// <summary>
        /// Transaction Başlat
        ///
        /// Explicit transaction yönetimi için.
        /// Birden fazla SaveChanges veya harici servis entegrasyonunda kullanılır.
        ///
        /// ÖRNEK:
        /// <code>
        /// await _unitOfWork.BeginTransactionAsync();
        /// try
        /// {
        ///     await _unitOfWork.Sales.AddAsync(sale);
        ///     await _unitOfWork.SaveChangesAsync();
        ///     await _paymentService.ProcessAsync(); // Harici servis
        ///     await _unitOfWork.CommitTransactionAsync();
        /// }
        /// catch
        /// {
        ///     await _unitOfWork.RollbackTransactionAsync();
        ///     throw;
        /// }
        /// </code>
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Transaction'ı Onayla
        /// Tüm değişiklikler kalıcı olur
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Transaction'ı Geri Al
        /// Tüm değişiklikler geri alınır
        /// Hata/iş kuralı ihlali durumunda kullanılır
        /// </summary>
        Task RollbackTransactionAsync();
    }
}

// ============================================================================
// EK BİLGİLER
// ============================================================================
//
// IMPLEMENTATION (Infrastructure):
// public class UnitOfWork : IUnitOfWork
// {
//     private readonly AppDbContext _context;
//     private IDbContextTransaction? _transaction;
//
//     // Lazy initialization
//     private IProductRepository? _products;
//     public IProductRepository Products =>
//         _products ??= new ProductRepository(_context);
//
//     public async Task<int> SaveChangesAsync(CancellationToken ct)
//         => await _context.SaveChangesAsync(ct);
//
//     public async Task BeginTransactionAsync()
//         => _transaction = await _context.Database.BeginTransactionAsync();
//
//     public async Task CommitTransactionAsync()
//         => await _transaction?.CommitAsync()!;
//
//     public async Task RollbackTransactionAsync()
//         => await _transaction?.RollbackAsync()!;
//
//     public void Dispose()
//     {
//         _transaction?.Dispose();
//         _context.Dispose();
//     }
// }
// ============================================================================
