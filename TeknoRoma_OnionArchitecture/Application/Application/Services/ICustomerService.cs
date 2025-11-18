using Application.DTOs.CategoryDTO;
using Application.DTOs.CustomerDTO;
using Application.DTOs.EmployeeDTO;
using Application.DTOs.EmployeeDTO;
using Application.DTOs.SupplierDTO;
using Application.DTOs.SupplierDTO;

namespace Application.Services
{
    /// <summary>
    /// Customer Service Interface
    /// Müşteri iş mantığı
    /// </summary>
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDTO>> GetAllCustomersAsync();
        Task<CustomerDTO> GetCustomerByIdAsync(int id);

        /// <summary>
        /// TC ile müşteri bulur
        /// NEDEN?
        /// - Gül Satar: "TC girdiğimde otomatik bilgiler gelmeli"
        /// </summary>
        Task<CustomerDTO> GetCustomerByIdentityNumberAsync(string identityNumber);

        Task<CustomerDTO> CreateCustomerAsync(CreateCustomerDTO dto);
        Task<CustomerDTO> UpdateCustomerAsync(UpdateCustomerDTO dto);
        Task<bool> DeleteCustomerAsync(int id);

        /// <summary>
        /// Müşterinin toplam alışveriş tutarını hesaplar
        /// VIP müşteri analizi için
        /// </summary>
        Task<decimal> GetCustomerTotalPurchaseAsync(int customerId);
    }

    /// <summary>
    /// Employee Service Interface
    /// Çalışan iş mantığı
    /// 
    /// ÖZEL İŞ MANTIĞI:
    /// - Identity User ile senkronizasyon
    /// - Prim hesaplama
    /// - Performans analizi
    /// </summary>
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeDTO>> GetAllEmployeesAsync();
        Task<EmployeeDTO> GetEmployeeByIdAsync(int id);
        Task<EmployeeDTO> GetEmployeeByEmailAsync(string email);

        /// <summary>
        /// Identity User ID ile çalışan bulur
        /// NEDEN?
        /// - Login sonrası kullanıcının Employee kaydını bulmak için
        /// </summary>
        Task<EmployeeDTO> GetEmployeeByIdentityUserIdAsync(string identityUserId);

        Task<IEnumerable<EmployeeDTO>> GetEmployeesByRoleAsync(Domain.Enums.UserRole role);
        Task<IEnumerable<EmployeeDTO>> GetEmployeesByStoreAsync(int storeId);

        Task<EmployeeDTO> CreateEmployeeAsync(CreateEmployeeDTO dto);
        Task<EmployeeDTO> UpdateEmployeeAsync(UpdateEmployeeDTO dto);
        Task<bool> DeleteEmployeeAsync(int id);

        /// <summary>
        /// Çalışanın satış performansını hesaplar
        /// </summary>
        Task<decimal> GetEmployeeSalesPerformanceAsync(int employeeId, int year, int month);

        /// <summary>
        /// Çalışanın prim tutarını hesaplar
        /// </summary>
        Task<decimal> CalculateCommissionAsync(int employeeId, int year, int month);
    }

    /// <summary>
    /// Category Service Interface
    /// Kategori iş mantığı
    /// </summary>
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync();
        Task<IEnumerable<CategoryDTO>> GetActiveCategoriesAsync();
        Task<CategoryDTO> GetCategoryByIdAsync(int id);

        Task<CategoryDTO> CreateCategoryAsync(CreateCategoryDTO dto);
        Task<CategoryDTO> UpdateCategoryAsync(UpdateCategoryDTO dto);

        /// <summary>
        /// Kategori siler
        /// 
        /// İŞ MANTIĞI:
        /// - Kategoride ürün varsa silinemez
        /// - Önce ürünleri başka kategoriye taşınmalı
        /// </summary>
        Task<bool> DeleteCategoryAsync(int id);

        /// <summary>
        /// Kategorideki ürün sayısını döndürür
        /// </summary>
        Task<int> GetProductCountAsync(int categoryId);
    }

    /// <summary>
    /// Supplier Service Interface
    /// Tedarikçi iş mantığı
    /// </summary>
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierDTO>> GetAllSuppliersAsync();
        Task<IEnumerable<SupplierDTO>> GetActiveSuppliersAsync();
        Task<SupplierDTO> GetSupplierByIdAsync(int id);

        Task<SupplierDTO> CreateSupplierAsync(CreateSupplierDTO dto);
        Task<SupplierDTO> UpdateSupplierAsync(UpdateSupplierDTO dto);
        Task<bool> DeleteSupplierAsync(int id);

        /// <summary>
        /// Tedarikçinin toplam alım tutarını hesaplar
        /// NEDEN?
        /// - Haluk Bey'in raporu: "Hangi tedarikçiden ne kadar almışız"
        /// </summary>
        Task<decimal> GetTotalPurchaseAsync(int supplierId, DateTime? startDate = null, DateTime? endDate = null);
    }
}