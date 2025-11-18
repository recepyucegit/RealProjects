using Domain.Entities;

namespace Application.Repositories
{
    /// <summary>
    /// Category Repository Interface
    /// Kategori için basit işlemler - Generic repository yeterli olabilir
    /// Ama özel metod eklemek için interface oluşturduk
    /// </summary>
    public interface ICategoryRepository : IRepository<Category>
    {
        /// <summary>
        /// Aktif kategorileri getirir
        /// Kullanıcıya sadece aktif kategoriler gösterilir
        /// </summary>
        Task<IReadOnlyList<Category>> GetActiveCategoriesAsync();

        /// <summary>
        /// Kategorideki ürün sayısını döndürür
        /// Kategori silinirken kontrol için
        /// </summary>
        Task<int> GetProductCountInCategoryAsync(int categoryId);
    }

    /// <summary>
    /// Supplier Repository Interface
    /// </summary>
    public interface ISupplierRepository : IRepository<Supplier>
    {
        /// <summary>
        /// Aktif tedarikçileri getirir
        /// </summary>
        Task<IReadOnlyList<Supplier>> GetActiveSuppliersAsync();

        /// <summary>
        /// Tedarikçinin toplam alım tutarını hesaplar
        /// NEDEN?
        /// - Haluk Bey'in raporu: "Hangi tedarikçiden ne kadar almışız"
        /// </summary>
        Task<decimal> GetSupplierTotalPurchaseAsync(int supplierId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// En çok alım yapılan tedarikçileri getirir
        /// </summary>
        Task<IReadOnlyList<Supplier>> GetTopSuppliersAsync(int count);
    }

    /// <summary>
    /// Store Repository Interface
    /// </summary>
    public interface IStoreRepository : IRepository<Store>
    {
        /// <summary>
        /// Aktif mağazaları getirir
        /// </summary>
        Task<IReadOnlyList<Store>> GetActiveStoresAsync();

        /// <summary>
        /// Şehre göre mağazaları getirir
        /// NEDEN?
        /// - İstanbul: 20, İzmir: 13, Ankara: 13, Bursa: 9 mağaza
        /// - Şehir bazında raporlar için
        /// </summary>
        Task<IReadOnlyList<Store>> GetByCityAsync(string city);

        /// <summary>
        /// Mağazanın toplam satışını hesaplar
        /// Performans analizi için
        /// </summary>
        Task<decimal> GetStoreTotalSalesAsync(int storeId, DateTime? startDate = null, DateTime? endDate = null);
    }

    /// <summary>
    /// Department Repository Interface
    /// </summary>
    public interface IDepartmentRepository : IRepository<Department>
    {
        /// <summary>
        /// Mağazaya göre departmanları getirir
        /// </summary>
        Task<IReadOnlyList<Department>> GetByStoreAsync(int storeId);

        /// <summary>
        /// Role göre departmanları getirir
        /// NEDEN?
        /// - Kullanıcıya sadece kendi rolüne uygun departmanları göstermek için
        /// </summary>
        Task<IReadOnlyList<Department>> GetByRoleAsync(Domain.Enums.UserRole role);

        /// <summary>
        /// Departmandaki çalışan sayısını döndürür
        /// </summary>
        Task<int> GetEmployeeCountInDepartmentAsync(int departmentId);
    }
}