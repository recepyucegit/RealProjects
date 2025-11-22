using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public interface IProductService
    {
        Task<Product?> GetByIdAsync(int id);
        Task<Product?> GetByBarcodeAsync(string barcode);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> GetBySupplierAsync(int supplierId);
        Task<IEnumerable<Product>> GetLowStockProductsAsync();
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<IEnumerable<Product>> SearchAsync(string searchTerm);
        Task<Product> CreateAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<bool> UpdateStockAsync(int productId, int quantity);
        Task<bool> IsBarcodeTakenAsync(string barcode, int? excludeId = null);
    }
}
