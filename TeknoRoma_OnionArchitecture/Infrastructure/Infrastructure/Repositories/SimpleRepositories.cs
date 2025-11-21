using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistance;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Simple Repository Implementations
    ///
    /// AMAÇ:
    /// - Basit entity'ler için özel iş mantığı gerektirmeyen repository'ler
    /// - Generic Repository metodlarını kullanır
    /// - Sadece interface implement eder, ek metod yok
    ///
    /// NEDEN AYRI SINIFLAR?
    /// - Dependency Injection için ayrı interface gerekli
    /// - Gelecekte özel metodlar eklenebilir
    /// - Type safety sağlar
    ///
    /// HANGİ ENTITY'LER SIMPLE?
    /// - Category: Sadece CRUD yeterli
    /// - Supplier: Sadece CRUD yeterli
    /// - Store: Sadece CRUD yeterli
    /// - Department: Sadece CRUD yeterli
    /// </summary>

    // ====== CATEGORY REPOSITORY ======

    /// <summary>
    /// Category Repository Implementation
    /// Kategori işlemleri - basit CRUD yeterli
    /// </summary>
    public class CategoryRepository : Repository<Category>, ISimpleRepository<Category>
    {
        public CategoryRepository(TeknoromaDbContext context) : base(context)
        {
        }

        // Generic Repository metodları yeterli
        // Özel iş mantığı gerektirmiyor
    }


    // ====== SUPPLIER REPOSITORY ======

    /// <summary>
    /// Supplier Repository Implementation
    /// Tedarikçi işlemleri - basit CRUD yeterli
    /// </summary>
    public class SupplierRepository : Repository<Supplier>, ISimpleRepository<Supplier>
    {
        public SupplierRepository(TeknoromaDbContext context) : base(context)
        {
        }

        // Generic Repository metodları yeterli
        // Tedarikçi analizi SupplierTransactionRepository'de yapılır
    }


    // ====== STORE REPOSITORY ======

    /// <summary>
    /// Store Repository Implementation
    /// Mağaza işlemleri - basit CRUD yeterli
    /// </summary>
    public class StoreRepository : Repository<Store>, ISimpleRepository<Store>
    {
        public StoreRepository(TeknoromaDbContext context) : base(context)
        {
        }

        // Generic Repository metodları yeterli
        // Mağaza analizi SaleRepository'de yapılır
    }


    // ====== DEPARTMENT REPOSITORY ======

    /// <summary>
    /// Department Repository Implementation
    /// Departman işlemleri - basit CRUD yeterli
    /// </summary>
    public class DepartmentRepository : Repository<Department>, ISimpleRepository<Department>
    {
        public DepartmentRepository(TeknoromaDbContext context) : base(context)
        {
        }

        // Generic Repository metodları yeterli
        // Departman analizi EmployeeRepository'de yapılır
    }
}
