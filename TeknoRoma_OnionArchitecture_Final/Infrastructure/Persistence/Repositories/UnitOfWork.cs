// ===================================================================================
// TEKNOROMA - UNIT OF WORK IMPLEMENTASYONU (UnitOfWork.cs)
// ===================================================================================
//
// BU DOSYANIN AMACI
// ===================================================================================
// IUnitOfWork interface'inin implementasyonu.
// Tum repository'leri tek bir DbContext uzerinden yonetir.
// Transaction yonetimi ve atomik islemler icin kullanilir.
//
// ===================================================================================

using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Unit of Work Implementasyonu
    ///
    /// TEK NOKTADAN VERI ERISIMI:
    /// - Tum repository'ler ayni DbContext'i paylasir
    /// - Transaction yonetimi merkezilestrilmistir
    /// - Lazy initialization ile performans optimizasyonu
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        // =================================================================
        // FIELD'LAR
        // =================================================================

        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        // Lazy initialization icin backing field'lar
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

        // =================================================================
        // CONSTRUCTOR
        // =================================================================

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        // =================================================================
        // BASIT REPOSITORY'LER (ISimpleRepository)
        // =================================================================

        /// <summary>
        /// Kategori Repository
        /// Lazy initialization: Ilk erisimde olusturulur
        /// </summary>
        public ISimpleRepository<Category> Categories =>
            _categories ??= new EfRepository<Category>(_context);

        /// <summary>
        /// Tedarikci Repository
        /// </summary>
        public ISimpleRepository<Supplier> Suppliers =>
            _suppliers ??= new EfRepository<Supplier>(_context);

        /// <summary>
        /// Magaza Repository
        /// </summary>
        public ISimpleRepository<Store> Stores =>
            _stores ??= new EfRepository<Store>(_context);

        /// <summary>
        /// Departman Repository
        /// </summary>
        public ISimpleRepository<Department> Departments =>
            _departments ??= new EfRepository<Department>(_context);

        // =================================================================
        // OZEL REPOSITORY'LER
        // =================================================================

        /// <summary>
        /// Urun Repository
        /// </summary>
        public IProductRepository Products =>
            _products ??= new ProductRepository(_context);

        /// <summary>
        /// Musteri Repository
        /// </summary>
        public ICustomerRepository Customers =>
            _customers ??= new CustomerRepository(_context);

        /// <summary>
        /// Calisan Repository
        /// </summary>
        public IEmployeeRepository Employees =>
            _employees ??= new EmployeeRepository(_context);

        /// <summary>
        /// Satis Repository
        /// </summary>
        public ISaleRepository Sales =>
            _sales ??= new SaleRepository(_context);

        /// <summary>
        /// Satis Detay Repository
        /// </summary>
        public ISaleDetailRepository SaleDetails =>
            _saleDetails ??= new SaleDetailRepository(_context);

        /// <summary>
        /// Gider Repository
        /// </summary>
        public IExpenseRepository Expenses =>
            _expenses ??= new ExpenseRepository(_context);

        /// <summary>
        /// Teknik Servis Repository
        /// </summary>
        public ITechnicalServiceRepository TechnicalServices =>
            _technicalServices ??= new TechnicalServiceRepository(_context);

        /// <summary>
        /// Tedarikci Islem Repository
        /// </summary>
        public ISupplierTransactionRepository SupplierTransactions =>
            _supplierTransactions ??= new SupplierTransactionRepository(_context);

        // =================================================================
        // TRANSACTION YONETIMI
        // =================================================================

        /// <summary>
        /// Degisiklikleri kaydet (Async)
        /// </summary>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Degisiklikleri kaydet (Sync)
        /// </summary>
        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        /// <summary>
        /// Transaction baslat
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Transaction'i onayla
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// Transaction'i geri al
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

        // =================================================================
        // DISPOSE
        // =================================================================

        /// <summary>
        /// Kaynaklari serbest birak
        /// </summary>
        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
