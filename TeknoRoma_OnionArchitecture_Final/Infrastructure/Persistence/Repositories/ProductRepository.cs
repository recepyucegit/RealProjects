// ===================================================================================
// TEKNOROMA - URUN REPOSITORY IMPLEMENTASYONU (ProductRepository.cs)
// ===================================================================================

using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Urun Repository Implementasyonu
    /// IProductRepository interface'ini implement eder
    /// </summary>
    public class ProductRepository : EfRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Barkod ile urun getir
        /// </summary>
        public async Task<Product?> GetByBarcodeAsync(string barcode)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.Barcode == barcode);
        }

        /// <summary>
        /// Kategoriye gore urunler
        /// </summary>
        public async Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }

        /// <summary>
        /// Tedarikciye gore urunler
        /// </summary>
        public async Task<IReadOnlyList<Product>> GetBySupplierAsync(int supplierId)
        {
            return await _dbSet
                .Where(p => p.SupplierId == supplierId)
                .ToListAsync();
        }

        /// <summary>
        /// Stok durumuna gore urunler
        /// </summary>
        public async Task<IReadOnlyList<Product>> GetByStockStatusAsync(StockStatus status)
        {
            return await _dbSet
                .Where(p => p.StockStatus == status)
                .ToListAsync();
        }

        /// <summary>
        /// Dusuk stoklu urunler
        /// </summary>
        public async Task<IReadOnlyList<Product>> GetLowStockProductsAsync()
        {
            return await _dbSet
                .Where(p => p.UnitsInStock <= p.ReorderLevel)
                .ToListAsync();
        }

        /// <summary>
        /// Aktif urunler
        /// </summary>
        public async Task<IReadOnlyList<Product>> GetActiveProductsAsync()
        {
            return await _dbSet
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        /// <summary>
        /// Isme gore arama
        /// </summary>
        public async Task<IReadOnlyList<Product>> SearchByNameAsync(string searchTerm)
        {
            return await _dbSet
                .Where(p => p.Name.Contains(searchTerm))
                .ToListAsync();
        }

        /// <summary>
        /// Stok guncelle
        /// </summary>
        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            var product = await _dbSet.FindAsync(productId);
            if (product == null)
                return false;

            var newStock = product.UnitsInStock + quantity;
            if (newStock < 0)
                return false;

            product.UnitsInStock = newStock;
            return true;
        }
    }
}
