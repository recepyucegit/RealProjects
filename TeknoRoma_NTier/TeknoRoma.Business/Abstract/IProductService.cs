using TeknoRoma.Business.DTOs;

namespace TeknoRoma.Business.Abstract;

/// <summary>
/// Product Service Interface
/// Ürün işlemleri için business logic metodlarını tanımlar
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Tüm aktif ürünleri getirir
    /// </summary>
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();

    /// <summary>
    /// ID'ye göre ürün getirir
    /// </summary>
    Task<ProductDto?> GetProductByIdAsync(int id);

    /// <summary>
    /// Kategoriye göre ürünleri getirir
    /// </summary>
    Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId);

    /// <summary>
    /// Öne çıkan ürünleri getirir
    /// </summary>
    Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync();

    /// <summary>
    /// Fiyat aralığına göre ürünleri getirir
    /// </summary>
    Task<IEnumerable<ProductDto>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);

    /// <summary>
    /// Ürün adına göre arama yapar
    /// </summary>
    Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm);

    /// <summary>
    /// Stokta bulunan ürünleri getirir
    /// </summary>
    Task<IEnumerable<ProductDto>> GetInStockProductsAsync();

    /// <summary>
    /// Yeni ürün ekler
    /// </summary>
    Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);

    /// <summary>
    /// Ürün günceller
    /// </summary>
    Task<ProductDto> UpdateProductAsync(UpdateProductDto updateProductDto);

    /// <summary>
    /// Ürünü siler (Soft Delete)
    /// </summary>
    Task<bool> DeleteProductAsync(int id);

    /// <summary>
    /// Ürün stoğunu günceller
    /// </summary>
    Task<bool> UpdateStockAsync(int id, int quantity);

    /// <summary>
    /// Ürün aktiflik durumunu değiştirir
    /// </summary>
    Task<bool> ToggleActiveStatusAsync(int id);
}
