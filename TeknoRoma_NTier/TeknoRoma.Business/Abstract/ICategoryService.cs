using TeknoRoma.Business.DTOs;

namespace TeknoRoma.Business.Abstract;

/// <summary>
/// Category Service Interface
/// Kategori işlemleri için business logic metodlarını tanımlar
/// </summary>
public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
    Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto);
    Task<bool> DeleteCategoryAsync(int id);
    Task<int> GetProductCountByCategoryAsync(int categoryId);
}
