using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TeknoRoma.Business.Abstract;
using TeknoRoma.Business.DTOs;
using TeknoRoma.DataAccess.Abstract;
using TeknoRoma.Entities;

namespace TeknoRoma.Business.Concrete;

/// <summary>
/// Category Service Implementation
/// Kategori ile ilgili business logic'i içerir
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync()
    {
        var categories = await _unitOfWork.Categories
            .FindAsync(c => c.IsActive);
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
    {
        // Business Rule: Aynı isimde kategori var mı kontrol et
        var existingCategory = await _unitOfWork.Categories
            .FirstOrDefaultAsync(c => c.Name == createCategoryDto.Name);

        if (existingCategory != null)
            throw new Exception("Bu isimde bir kategori zaten mevcut!");

        var category = _mapper.Map<Category>(createCategoryDto);
        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto)
    {
        var existingCategory = await _unitOfWork.Categories.GetByIdAsync(updateCategoryDto.Id);
        if (existingCategory == null)
            throw new Exception("Kategori bulunamadı!");

        // Business Rule: Başka bir kategoride aynı isim var mı kontrol et
        var duplicateCategory = await _unitOfWork.Categories
            .FirstOrDefaultAsync(c => c.Name == updateCategoryDto.Name && c.Id != updateCategoryDto.Id);

        if (duplicateCategory != null)
            throw new Exception("Bu isimde bir kategori zaten mevcut!");

        _mapper.Map(updateCategoryDto, existingCategory);
        _unitOfWork.Categories.Update(existingCategory);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CategoryDto>(existingCategory);
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
            return false;

        // Business Rule: Kategoriye ait ürün var mı kontrol et
        var productCount = await _unitOfWork.Products
            .CountAsync(p => p.CategoryId == id);

        if (productCount > 0)
            throw new Exception("Bu kategoriye ait ürünler bulunmaktadır! Önce ürünleri silmelisiniz.");

        _unitOfWork.Categories.SoftDelete(category);
        return await _unitOfWork.SaveChangesAsync();
    }

    public async Task<int> GetProductCountByCategoryAsync(int categoryId)
    {
        return await _unitOfWork.Products
            .CountAsync(p => p.CategoryId == categoryId);
    }
}
