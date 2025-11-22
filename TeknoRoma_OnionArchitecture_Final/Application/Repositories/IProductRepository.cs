using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product?> GetByBarcodeAsync(string barcode);
        Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId);
        Task<IReadOnlyList<Product>> GetBySupplierAsync(int supplierId);
        Task<IReadOnlyList<Product>> GetByStockStatusAsync(StockStatus status);
        Task<IReadOnlyList<Product>> GetLowStockProductsAsync();
        Task<IReadOnlyList<Product>> GetActiveProductsAsync();
        Task<IReadOnlyList<Product>> SearchByNameAsync(string searchTerm);
        Task<bool> UpdateStockAsync(int productId, int quantity);
    }
}
