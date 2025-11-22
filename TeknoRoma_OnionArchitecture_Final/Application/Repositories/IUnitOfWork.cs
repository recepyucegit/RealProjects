using Domain.Entities;

namespace Application.Repositories
{
    /// <summary>
    /// Unit of Work Pattern Interface
    /// Tüm repository'leri tek bir transaction içinde yönetir
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // Basit Repository'ler
        ISimpleRepository<Category> Categories { get; }
        ISimpleRepository<Supplier> Suppliers { get; }
        ISimpleRepository<Store> Stores { get; }
        ISimpleRepository<Department> Departments { get; }

        // Özel Repository'ler
        IProductRepository Products { get; }
        ICustomerRepository Customers { get; }
        IEmployeeRepository Employees { get; }
        ISaleRepository Sales { get; }
        ISaleDetailRepository SaleDetails { get; }
        IExpenseRepository Expenses { get; }
        ITechnicalServiceRepository TechnicalServices { get; }
        ISupplierTransactionRepository SupplierTransactions { get; }

        // Transaction yönetimi
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        int SaveChanges();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
