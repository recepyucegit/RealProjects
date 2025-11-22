using Microsoft.AspNetCore.Mvc;
using TeknoRoma.Business.Abstract;
using TeknoRoma.Business.DTOs;

namespace TeknoRoma.WebMVC.Controllers;

/// <summary>
/// Products Controller (MVC)
/// Ürün listeleme, detay ve yönetim sayfalarını yönetir
/// </summary>
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public ProductsController(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }

    /// <summary>
    /// Ürün listesi sayfası
    /// GET: /Products
    /// </summary>
    public async Task<IActionResult> Index(int? categoryId, string? searchTerm)
    {
        try
        {
            IEnumerable<ProductDto> products;

            if (categoryId.HasValue)
            {
                // Kategoriye göre filtrele
                products = await _productService.GetProductsByCategoryAsync(categoryId.Value);
                ViewBag.SelectedCategory = categoryId.Value;
            }
            else if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Arama terimine göre filtrele
                products = await _productService.SearchProductsAsync(searchTerm);
                ViewBag.SearchTerm = searchTerm;
            }
            else
            {
                // Tüm ürünleri getir
                products = await _productService.GetAllProductsAsync();
            }

            // Kategorileri dropdown için ViewBag'e ekle
            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();

            return View(products);
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            return View();
        }
    }

    /// <summary>
    /// Ürün detay sayfası
    /// GET: /Products/Details/5
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                TempData["Error"] = "Ürün bulunamadı!";
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Yeni ürün ekleme sayfası (GET)
    /// GET: /Products/Create
    /// </summary>
    public async Task<IActionResult> Create()
    {
        // Kategorileri ve tedarikçileri dropdown için ViewBag'e ekle
        ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
        return View();
    }

    /// <summary>
    /// Yeni ürün ekleme işlemi (POST)
    /// POST: /Products/Create
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductDto createProductDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
                return View(createProductDto);
            }

            await _productService.CreateProductAsync(createProductDto);

            TempData["Success"] = "Ürün başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
            return View(createProductDto);
        }
    }

    /// <summary>
    /// Ürün düzenleme sayfası (GET)
    /// GET: /Products/Edit/5
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                TempData["Error"] = "Ürün bulunamadı!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();

            // ProductDto'yu UpdateProductDto'ya dönüştür
            var updateDto = new UpdateProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                IsFeatured = product.IsFeatured,
                CategoryId = product.CategoryId,
                SupplierId = product.SupplierId
            };

            return View(updateDto);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Ürün düzenleme işlemi (POST)
    /// POST: /Products/Edit/5
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateProductDto updateProductDto)
    {
        try
        {
            if (id != updateProductDto.Id)
            {
                TempData["Error"] = "ID uyuşmazlığı!";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
                return View(updateProductDto);
            }

            await _productService.UpdateProductAsync(updateProductDto);

            TempData["Success"] = "Ürün başarıyla güncellendi!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
            return View(updateProductDto);
        }
    }

    /// <summary>
    /// Ürün silme işlemi
    /// POST: /Products/Delete/5
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _productService.DeleteProductAsync(id);

            if (result)
                TempData["Success"] = "Ürün başarıyla silindi!";
            else
                TempData["Error"] = "Ürün silinirken bir hata oluştu!";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
