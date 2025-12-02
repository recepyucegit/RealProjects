using Microsoft.AspNetCore.Mvc;
using TeknoRoma.Business.Abstract;
using TeknoRoma.Business.DTOs;

namespace TeknoRoma.WebMVC.Controllers;

/// <summary>
/// Categories Controller (MVC)
/// Kategori yönetim sayfalarını yönetir
/// </summary>
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Kategori listesi sayfası
    /// GET: /Categories
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            return View();
        }
    }

    /// <summary>
    /// Kategori detay sayfası
    /// GET: /Categories/Details/5
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
            {
                TempData["Error"] = "Kategori bulunamadı!";
                return RedirectToAction(nameof(Index));
            }

            // Kategoriye ait ürün sayısını al
            ViewBag.ProductCount = await _categoryService.GetProductCountByCategoryAsync(id);

            return View(category);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Yeni kategori ekleme sayfası (GET)
    /// GET: /Categories/Create
    /// </summary>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Yeni kategori ekleme işlemi (POST)
    /// POST: /Categories/Create
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryDto createCategoryDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(createCategoryDto);
            }

            await _categoryService.CreateCategoryAsync(createCategoryDto);

            TempData["Success"] = "Kategori başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(createCategoryDto);
        }
    }

    /// <summary>
    /// Kategori düzenleme sayfası (GET)
    /// GET: /Categories/Edit/5
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
            {
                TempData["Error"] = "Kategori bulunamadı!";
                return RedirectToAction(nameof(Index));
            }

            // CategoryDto'yu UpdateCategoryDto'ya dönüştür
            var updateDto = new UpdateCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                IsActive = category.IsActive
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
    /// Kategori düzenleme işlemi (POST)
    /// POST: /Categories/Edit/5
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateCategoryDto updateCategoryDto)
    {
        try
        {
            if (id != updateCategoryDto.Id)
            {
                TempData["Error"] = "ID uyuşmazlığı!";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(updateCategoryDto);
            }

            await _categoryService.UpdateCategoryAsync(updateCategoryDto);

            TempData["Success"] = "Kategori başarıyla güncellendi!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return View(updateCategoryDto);
        }
    }

    /// <summary>
    /// Kategori silme işlemi
    /// POST: /Categories/Delete/5
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _categoryService.DeleteCategoryAsync(id);

            if (result)
                TempData["Success"] = "Kategori başarıyla silindi!";
            else
                TempData["Error"] = "Kategori silinirken bir hata oluştu!";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
