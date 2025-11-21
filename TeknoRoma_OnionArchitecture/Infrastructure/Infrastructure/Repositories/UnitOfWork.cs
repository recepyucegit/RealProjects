using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Unit of Work Implementation
    ///
    /// AMAÇ:
    /// - IUnitOfWork interface'ini implement eder
    /// - Tüm repository'leri bir arada tutar
    /// - Transaction yönetimi sağlar
    /// - SaveChanges() merkezileştirir
    ///
    /// NASIL ÇALIŞIR?
    /// 1. Constructor'da tüm repository'ler oluşturulur (Lazy Loading)
    /// 2. İlk erişimde repository instance'ı oluşturulur
    /// 3. SaveChangesAsync() çağrıldığında tüm değişiklikler kaydedilir
    /// 4. Dispose() ile kaynaklar temizlenir
    ///
    /// LAZY LOADING PATTERN:
    /// - Repository'ler ilk erişimde oluşturulur
    /// - Kullanılmayan repository'ler memory'de yer kaplamaz
    /// - Performans optimizasyonu
    ///
    /// ÖRNEK:
    /// <code>
    /// using (var unitOfWork = new UnitOfWork(context))
    /// {
    ///     var sale = await unitOfWork.Sales.AddAsync(newSale);
    ///     await unitOfWork.SaveChangesAsync();
    /// } // Dispose otomatik çağrılır
    /// </code>
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TeknoromaDbContext _context;
        private IDbContextTransaction? _transaction;

        // Repository field'ları - Lazy Loading için nullable
        private ISimpleRepository<Category>? _categories;
        private ISimpleRepository<Supplier>? _suppliers;
        private ISimpleRepository<Store>? _stores;
        private ISimpleRepository<Department>? _departments;
        private IProductRepository? _products;
        private ICustomerRepository? _customers;
        private IEmployeeRepository? _employees;
        private ISaleRepository? _sales;
        private ISaleDetailRepository? _saleDetails;
        private IExpenseRepository? _expenses;
        private ITechnicalServiceRepository? _technicalServices;
        private ISupplierTransactionRepository? _supplierTransactions;

        /// <summary>
        /// Constructor - DbContext Dependency Injection
        /// </summary>
        public UnitOfWork(TeknoromaDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }


        // ====== REPOSITORY PROPERTIES (LAZY LOADING) ======

        /// <summary>
        /// Categories Repository
        /// Lazy Loading: İlk erişimde oluşturulur
        /// </summary>
        public ISimpleRepository<Category> Categories
        {
            get
            {
                // Null ise oluştur, değilse mevcut instance'ı döndür
                return _categories ??= new CategoryRepository(_context);
            }
        }

        public ISimpleRepository<Supplier> Suppliers
        {
            get
            {
                return _suppliers ??= new SupplierRepository(_context);
            }
        }

        public ISimpleRepository<Store> Stores
        {
            get
            {
                return _stores ??= new StoreRepository(_context);
            }
        }

        public ISimpleRepository<Department> Departments
        {
            get
            {
                return _departments ??= new DepartmentRepository(_context);
            }
        }

        public IProductRepository Products
        {
            get
            {
                return _products ??= new ProductRepository(_context);
            }
        }

        public ICustomerRepository Customers
        {
            get
            {
                return _customers ??= new CustomerRepository(_context);
            }
        }

        public IEmployeeRepository Employees
        {
            get
            {
                return _employees ??= new EmployeeRepository(_context);
            }
        }

        public ISaleRepository Sales
        {
            get
            {
                return _sales ??= new SaleRepository(_context);
            }
        }

        public ISaleDetailRepository SaleDetails
        {
            get
            {
                return _saleDetails ??= new SaleDetailRepository(_context);
            }
        }

        public IExpenseRepository Expenses
        {
            get
            {
                return _expenses ??= new ExpenseRepository(_context);
            }
        }

        public ITechnicalServiceRepository TechnicalServices
        {
            get
            {
                return _technicalServices ??= new TechnicalServiceRepository(_context);
            }
        }

        public ISupplierTransactionRepository SupplierTransactions
        {
            get
            {
                return _supplierTransactions ??= new SupplierTransactionRepository(_context);
            }
        }


        // ====== SAVE CHANGES ======

        /// <summary>
        /// Tüm değişiklikleri veritabanına kaydeder (Async)
        ///
        /// NEDEN ASYNC?
        /// - Database I/O işlemi
        /// - Thread'i bloklamaz
        /// - Web uygulamalarında performans kritik
        ///
        /// NE ZAMAN ÇAĞRILIR?
        /// - Repository işlemlerinden sonra
        /// - Tek seferde tüm değişiklikler kaydedilir
        ///
        /// ÖRNEK:
        /// <code>
        /// await _unitOfWork.Sales.AddAsync(sale);
        /// await _unitOfWork.SaleDetails.AddRangeAsync(details);
        /// int savedCount = await _unitOfWork.SaveChangesAsync(); // Tek seferde
        /// </code>
        /// </summary>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // DbContext.SaveChangesAsync() tüm değişiklikleri kaydeder
            // BaseEntity.ModifiedDate, CreatedDate vb. otomatik güncellenir
            return await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Tüm değişiklikleri veritabanına kaydeder (Sync)
        /// NOT: Mümkün oldukça SaveChangesAsync kullanılmalı
        /// </summary>
        public int SaveChanges()
        {
            return _context.SaveChanges();
        }


        // ====== TRANSACTION YÖNETİMİ ======

        /// <summary>
        /// Transaction başlatır
        ///
        /// NEDEN TRANSACTION?
        /// - Birden fazla işlemi atomik yapar
        /// - Ya hepsi başarılı ya hiçbiri (ACID)
        /// - Hata durumunda rollback
        ///
        /// KULLANIM:
        /// <code>
        /// await _unitOfWork.BeginTransactionAsync();
        /// try
        /// {
        ///     // İşlemler...
        ///     await _unitOfWork.SaveChangesAsync();
        ///     await _unitOfWork.CommitTransactionAsync(); // Başarılı
        /// }
        /// catch
        /// {
        ///     await _unitOfWork.RollbackTransactionAsync(); // Hata, geri al
        ///     throw;
        /// }
        /// </code>
        ///
        /// NOT:
        /// - Normal durumlarda SaveChangesAsync yeterli
        /// - Karmaşık senaryolar için manuel transaction
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
                throw new InvalidOperationException("Transaction zaten başlatılmış!");

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Transaction'ı commit eder (onaylar)
        /// Tüm değişiklikler kalıcı olur
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("Aktif transaction yok!");

            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// Transaction'ı rollback eder (geri alır)
        /// Tüm değişiklikler iptal edilir
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("Aktif transaction yok!");

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }


        // ====== DISPOSE PATTERN ======

        /// <summary>
        /// Kaynakları temizler
        ///
        /// NEDEN DISPOSE?
        /// - DbContext dispose edilmeli
        /// - Memory leak önlenir
        /// - Database bağlantısı serbest bırakılır
        ///
        /// KULLANIM:
        /// <code>
        /// using (var unitOfWork = new UnitOfWork(context))
        /// {
        ///     // İşlemler...
        /// } // Dispose otomatik çağrılır
        /// </code>
        ///
        /// VEYA:
        /// <code>
        /// var unitOfWork = new UnitOfWork(context);
        /// try
        /// {
        ///     // İşlemler...
        /// }
        /// finally
        /// {
        ///     unitOfWork.Dispose(); // Manuel
        /// }
        /// </code>
        /// </summary>
        public void Dispose()
        {
            // Transaction varsa dispose et
            _transaction?.Dispose();

            // DbContext dispose et
            _context?.Dispose();
        }
    }
}
