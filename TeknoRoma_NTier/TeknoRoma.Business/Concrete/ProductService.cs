using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TeknoRoma.Business.Abstract;
using TeknoRoma.Business.DTOs;
using TeknoRoma.DataAccess.Abstract;
using TeknoRoma.Entities;

namespace TeknoRoma.Business.Concrete;

/// <summary>
/// Product Service Implementation
/// Ürün ile ilgili business logic'i içerir
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// Constructor - Dependency Injection ile bağımlılıkları alır
    /// </summary>
    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        // Repository'den tüm ürünleri al
        var products = await _unitOfWork.Products
            .GetQueryable()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.IsActive)
            .ToListAsync();

        // Entity'leri DTO'ya dönüştür
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _unitOfWork.Products
            .GetQueryable()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
    {
        var products = await _unitOfWork.Products
            .GetQueryable()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync()
    {
        var products = await _unitOfWork.Products
            .GetQueryable()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.IsFeatured && p.IsActive)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        var products = await _unitOfWork.Products
            .GetQueryable()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice && p.IsActive)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
    {
        var products = await _unitOfWork.Products
            .GetQueryable()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.Name.Contains(searchTerm) && p.IsActive)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<IEnumerable<ProductDto>> GetInStockProductsAsync()
    {
        var products = await _unitOfWork.Products
            .GetQueryable()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.Stock > 0 && p.IsActive)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
    {
        // Business Rule: Kategori ve Tedarikçi var mı kontrol et
        var categoryExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == createProductDto.CategoryId);
        if (!categoryExists)
            throw new Exception("Kategori bulunamadı!");

        var supplierExists = await _unitOfWork.Suppliers.AnyAsync(s => s.Id == createProductDto.SupplierId);
        if (!supplierExists)
            throw new Exception("Tedarikçi bulunamadı!");

        // Business Rule: İndirimli fiyat normal fiyattan yüksek olamaz
        if (createProductDto.DiscountPrice.HasValue && createProductDto.DiscountPrice >= createProductDto.Price)
            throw new Exception("İndirimli fiyat, normal fiyattan düşük olmalıdır!");

        // DTO'dan Entity'ye dönüşüm
        var product = _mapper.Map<Product>(createProductDto);

        // Repository'ye ekle
        await _unitOfWork.Products.AddAsync(product);

        // Değişiklikleri kaydet
        await _unitOfWork.SaveChangesAsync();

        // Eklenen entity'yi DTO'ya dönüştür ve döndür
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateProductAsync(UpdateProductDto updateProductDto)
    {
        // Ürün var mı kontrol et
        var existingProduct = await _unitOfWork.Products.GetByIdAsync(updateProductDto.Id);
        if (existingProduct == null)
            throw new Exception("Ürün bulunamadı!");

        // Business Rule validasyonları
        var categoryExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == updateProductDto.CategoryId);
        if (!categoryExists)
            throw new Exception("Kategori bulunamadı!");

        var supplierExists = await _unitOfWork.Suppliers.AnyAsync(s => s.Id == updateProductDto.SupplierId);
        if (!supplierExists)
            throw new Exception("Tedarikçi bulunamadı!");

        if (updateProductDto.DiscountPrice.HasValue && updateProductDto.DiscountPrice >= updateProductDto.Price)
            throw new Exception("İndirimli fiyat, normal fiyattan düşük olmalıdır!");

        // DTO'dan Entity'ye mapping
        _mapper.Map(updateProductDto, existingProduct);

        // Repository'yi güncelle
        _unitOfWork.Products.Update(existingProduct);

        // Değişiklikleri kaydet
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductDto>(existingProduct);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            return false;

        // Soft Delete
        _unitOfWork.Products.SoftDelete(product);

        return await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> UpdateStockAsync(int id, int quantity)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            return false;

        // Stok güncellemesi
        product.Stock = quantity;
        _unitOfWork.Products.Update(product);

        return await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> ToggleActiveStatusAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            return false;

        // Aktiflik durumunu değiştir
        product.IsActive = !product.IsActive;
        _unitOfWork.Products.Update(product);

        return await _unitOfWork.SaveChangesAsync();
    }
}
