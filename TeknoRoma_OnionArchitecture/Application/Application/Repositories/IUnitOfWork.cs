namespace Application.Repositories
{
    /// <summary>
    /// Unit of Work Pattern Interface
    ///
    /// AMAÇ:
    /// - Birden fazla repository'yi tek bir transaction içinde yönetir
    /// - SaveChanges() çağrısını merkezileştirir
    /// - Repository'leri bir arada tutar (Dependency Injection için)
    ///
    /// NEDEN UNIT OF WORK?
    /// 1. TRANSACTION YÖNETİMİ:
    ///    - Birden fazla işlemi tek bir transaction'da toplar
    ///    - Ya hepsi başarılı olur ya hiçbiri (ACID prensibi)
    ///
    /// 2. PERFORMANS:
    ///    - SaveChanges() tek seferde çağrılır
    ///    - Database'e daha az round-trip
    ///
    /// 3. KOD TEMİZLİĞİ:
    ///    - Repository'leri tek bir yerden alırız
    ///    - Dependency Injection kolaylaşır
    ///
    /// ÖRNEK KULLANIM:
    /// <code>
    /// public class SaleService
    /// {
    ///     private readonly IUnitOfWork _unitOfWork;
    ///
    ///     public async Task CreateSaleAsync(SaleDTO saleDto)
    ///     {
    ///         // 1. Sale kaydı oluştur
    ///         var sale = await _unitOfWork.Sales.AddAsync(newSale);
    ///
    ///         // 2. SaleDetail kayıtları oluştur
    ///         await _unitOfWork.SaleDetails.AddRangeAsync(saleDetails);
    ///
    ///         // 3. Stok azalt
    ///         await _unitOfWork.Products.DecreaseStockAsync(productId, quantity);
    ///
    ///         // 4. Tek seferde kaydet (Transaction)
    ///         await _unitOfWork.SaveChangesAsync();
    ///         // Ya hepsi başarılı olur ya hiçbiri!
    ///     }
    /// }
    /// </code>
    ///
    /// MİMARİ:
    /// Application (Interface) → Infrastructure (Implementation)
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // ====== REPOSITORY'LER ======
        // Her repository bir property olarak erişilebilir

        // Basit Repository'ler
        ISimpleRepository<Domain.Entities.Category> Categories { get; }
        ISimpleRepository<Domain.Entities.Supplier> Suppliers { get; }
        ISimpleRepository<Domain.Entities.Store> Stores { get; }
        ISimpleRepository<Domain.Entities.Department> Departments { get; }

        // Özel Repository'ler
        IProductRepository Products { get; }
        ICustomerRepository Customers { get; }
        IEmployeeRepository Employees { get; }
        ISaleRepository Sales { get; }
        ISaleDetailRepository SaleDetails { get; }
        IExpenseRepository Expenses { get; }
        ITechnicalServiceRepository TechnicalServices { get; }
        ISupplierTransactionRepository SupplierTransactions { get; }


        // ====== TRANSACTION YÖNETİMİ ======

        /// <summary>
        /// Tüm değişiklikleri veritabanına kaydeder
        /// NEDEN ASYNC?
        /// - Database I/O işlemi
        /// - Thread'i bloklamaz
        ///
        /// NEDEN TEK SaveChanges?
        /// - Birden fazla repository işlemi tek transaction'da
        /// - ACID prensibi: Ya hepsi ya hiçbiri
        ///
        /// ÖRNEK:
        /// <code>
        /// await _unitOfWork.Sales.AddAsync(sale);
        /// await _unitOfWork.SaleDetails.AddAsync(saleDetail);
        /// await _unitOfWork.SaveChangesAsync(); // Tek seferde kaydet
        /// </code>
        /// </summary>
        /// <returns>Kaydedilen kayıt sayısı</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Senkron SaveChanges
        /// NOT: Mümkün oldukça SaveChangesAsync kullanılmalı
        /// </summary>
        int SaveChanges();

        /// <summary>
        /// Transaction başlatır
        /// İLERİ SEVİYE: Manuel transaction yönetimi için
        ///
        /// KULLANIM:
        /// <code>
        /// using var transaction = await _unitOfWork.BeginTransactionAsync();
        /// try
        /// {
        ///     await _unitOfWork.Sales.AddAsync(sale);
        ///     await _unitOfWork.SaveChangesAsync();
        ///
        ///     await _unitOfWork.Products.DecreaseStockAsync(productId, quantity);
        ///     await _unitOfWork.SaveChangesAsync();
        ///
        ///     await transaction.CommitAsync(); // Başarılı
        /// }
        /// catch
        /// {
        ///     await transaction.RollbackAsync(); // Hata olursa geri al
        ///     throw;
        /// }
        /// </code>
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Transaction'ı commit eder (onaylar)
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Transaction'ı rollback eder (geri alır)
        /// </summary>
        Task RollbackTransactionAsync();
    }
}
