// ===================================================================================
// TEKNOROMA MVC - CATEGORIES CONTROLLER
// ===================================================================================

using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebMVC.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(IUnitOfWork unitOfWork, ILogger<CategoriesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetAllAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategoriler listelenirken hata");
                TempData["Error"] = "Kategoriler yuklenirken hata olustu.";
                return View(new List<Category>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                {
                    TempData["Error"] = "Kategori bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }
                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori detay yuklenirken hata. CategoryId: {Id}", id);
                TempData["Error"] = "Detay yuklenirken hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(category);

                category.IsActive = true;
                await _unitOfWork.Categories.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Kategori basariyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori eklenirken hata");
                TempData["Error"] = "Kategori eklenirken hata olustu.";
                return View(category);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                {
                    TempData["Error"] = "Kategori bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }
                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori duzenleme formu yuklenirken hata. CategoryId: {Id}", id);
                TempData["Error"] = "Form yuklenirken hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            try
            {
                if (id != category.Id)
                {
                    TempData["Error"] = "ID uyusmazligi.";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                    return View(category);

                await _unitOfWork.Categories.UpdateAsync(category);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Kategori basariyla guncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori guncellenirken hata. CategoryId: {Id}", id);
                TempData["Error"] = "Kategori guncellenirken hata olustu.";
                return View(category);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                {
                    TempData["Error"] = "Kategori bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }

                await _unitOfWork.Categories.DeleteAsync(category);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Kategori basariyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori silinirken hata. CategoryId: {Id}", id);
                TempData["Error"] = "Kategori silinirken hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
