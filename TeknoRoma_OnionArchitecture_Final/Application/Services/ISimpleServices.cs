using Domain.Entities;

namespace Application.Services
{
    public interface ICategoryService
    {
        Task<Category?> GetByIdAsync(int id);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<Category> CreateAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
        Task<int> GetProductCountAsync(int categoryId);
    }

    public interface ISupplierService
    {
        Task<Supplier?> GetByIdAsync(int id);
        Task<IEnumerable<Supplier>> GetAllAsync();
        Task<IEnumerable<Supplier>> GetActiveSuppliersAsync();
        Task<Supplier> CreateAsync(Supplier supplier);
        Task UpdateAsync(Supplier supplier);
        Task DeleteAsync(int id);
        Task<decimal> GetTotalPurchaseAsync(int supplierId, DateTime? startDate = null, DateTime? endDate = null);
    }

    public interface IStoreService
    {
        Task<Store?> GetByIdAsync(int id);
        Task<IEnumerable<Store>> GetAllAsync();
        Task<IEnumerable<Store>> GetActiveStoresAsync();
        Task<IEnumerable<Store>> GetByCityAsync(string city);
        Task<Store> CreateAsync(Store store);
        Task UpdateAsync(Store store);
        Task DeleteAsync(int id);
        Task<decimal> GetTotalSalesAsync(int storeId, DateTime? startDate = null, DateTime? endDate = null);
    }

    public interface IDepartmentService
    {
        Task<Department?> GetByIdAsync(int id);
        Task<IEnumerable<Department>> GetAllAsync();
        Task<IEnumerable<Department>> GetByStoreAsync(int storeId);
        Task<Department> CreateAsync(Department department);
        Task UpdateAsync(Department department);
        Task DeleteAsync(int id);
        Task<int> GetEmployeeCountAsync(int departmentId);
    }
}
