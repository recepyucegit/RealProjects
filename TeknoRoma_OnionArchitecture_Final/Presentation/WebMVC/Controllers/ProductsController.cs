// ===================================================================================
// TEKNOROMA MVC - PRODUCTS CONTROLLER
// ===================================================================================
//
// Urun yonetimi islemleri icin controller.
// CRUD (Create, Read, Update, Delete) islemleri saglar.
//
// ROLLER:
// - Sube Muduru: Tum islemler
// - Kasa: Sadece goruntuleme
// - Depo: Stok guncelleme
//
// ===================================================================================

using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebMVC.Models;

namespace WebMVC.Controllers
{
    /// <summary>
    /// Urun Yonetimi Controller
    /// </summary>
    public class ProductsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IUnitOfWork unitOfWork,
            ILogger<ProductsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // =========================================================================
        // INDEX - URUN LISTESI
        // =========================================================================

        /// <summary>
        /// Tum urunleri listeler (DataTables ile)
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var products = await _unitOfWork.Products.GetAllWithCategoryAndSupplierAsync();
            return View(products);
        }

        /// <summary>
        /// Kritik stok urunleri
        /// </summary>
        public async Task<IActionResult> LowStock()
        {
            var products = await _unitOfWork.Products.GetLowStockProductsAsync();
            ViewData["Title"] = "Kritik Stok Urunleri";
            return View("Index", products);
        }

        // =========================================================================
        // DETAILS - URUN DETAY
        // =========================================================================

        /// <summary>
        /// Urun detaylarini goster
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var product = await _unitOfWork.Products.GetByIdWithDetailsAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Urun bulunamadi.";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // =========================================================================
        // CREATE - YENI URUN
        // =========================================================================

        /// <summary>
        /// Yeni urun formu
        /// </summary>
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View(new ProductViewModel());
        }

        /// <summary>
        /// Yeni urun kaydet
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(model);
            }

            try
            {
                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description ?? string.Empty,
                    Barcode = model.Barcode,
                    UnitPrice = model.UnitPrice,
                    UnitsInStock = model.UnitsInStock,
                    CriticalStockLevel = model.CriticalStockLevel,
                    CategoryId = model.CategoryId,
                    SupplierId = model.SupplierId,
                    IsActive = model.IsActive,
                    ImageUrl = model.ImageUrl,
                    StockStatus = model.UnitsInStock <= 0 ? StockStatus.Tukendi
                        : model.UnitsInStock <= model.CriticalStockLevel ? StockStatus.Kritik
                        : StockStatus.Yeterli
                };

                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = $"'{product.Name}' urunu basariyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Urun eklenirken hata");
                TempData["Error"] = "Urun eklenirken bir hata olustu.";
                await PopulateDropdowns();
                return View(model);
            }
        }

        // =========================================================================
        // EDIT - URUN DUZENLEME
        // =========================================================================

        /// <summary>
        /// Urun duzenleme formu
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Urun bulunamadi.";
                return RedirectToAction(nameof(Index));
            }

            var model = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Barcode = product.Barcode,
                UnitPrice = product.UnitPrice,
                UnitsInStock = product.UnitsInStock,
                CriticalStockLevel = product.CriticalStockLevel,
                CategoryId = product.CategoryId,
                SupplierId = product.SupplierId,
                IsActive = product.IsActive,
                ImageUrl = product.ImageUrl
            };

            await PopulateDropdowns();
            return View(model);
        }

        /// <summary>
        /// Urun guncelle
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(model);
            }

            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Urun bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }

                product.Name = model.Name;
                product.Description = model.Description ?? string.Empty;
                product.Barcode = model.Barcode;
                product.UnitPrice = model.UnitPrice;
                product.UnitsInStock = model.UnitsInStock;
                product.CriticalStockLevel = model.CriticalStockLevel;
                product.CategoryId = model.CategoryId;
                product.SupplierId = model.SupplierId;
                product.IsActive = model.IsActive;
                product.ImageUrl = model.ImageUrl;
                product.StockStatus = model.UnitsInStock <= 0 ? StockStatus.Tukendi
                    : model.UnitsInStock <= model.CriticalStockLevel ? StockStatus.Kritik
                    : StockStatus.Yeterli;

                _unitOfWork.Products.Update(product);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = $"'{product.Name}' urunu basariyla guncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Urun guncellenirken hata: {ProductId}", id);
                TempData["Error"] = "Urun guncellenirken bir hata olustu.";
                await PopulateDropdowns();
                return View(model);
            }
        }

        // =========================================================================
        // DELETE - URUN SILME
        // =========================================================================

        /// <summary>
        /// Urun silme (Soft Delete)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                {
                    return Json(new { success = false, message = "Urun bulunamadi." });
                }

                _unitOfWork.Products.Delete(product);
                await _unitOfWork.SaveChangesAsync();

                return Json(new { success = true, message = $"'{product.Name}' urunu silindi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Urun silinirken hata: {ProductId}", id);
                return Json(new { success = false, message = "Urun silinirken bir hata olustu." });
            }
        }

        // =========================================================================
        // STOK GUNCELLEME (DEPO ICIN)
        // =========================================================================

        /// <summary>
        /// Stok guncelleme (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(int id, int quantity)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                {
                    return Json(new { success = false, message = "Urun bulunamadi." });
                }

                product.UnitsInStock = quantity;
                product.StockStatus = quantity <= 0 ? StockStatus.Tukendi
                    : quantity <= product.CriticalStockLevel ? StockStatus.Kritik
                    : StockStatus.Yeterli;

                _unitOfWork.Products.Update(product);
                await _unitOfWork.SaveChangesAsync();

                return Json(new {
                    success = true,
                    message = $"Stok guncellendi: {quantity} adet",
                    stockStatus = product.StockStatusText
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stok guncellenirken hata: {ProductId}", id);
                return Json(new { success = false, message = "Stok guncellenirken bir hata olustu." });
            }
        }

        // =========================================================================
        // YARDIMCI METODLAR
        // =========================================================================

        /// <summary>
        /// Dropdown listelerini doldur
        /// </summary>
        private async Task PopulateDropdowns()
        {
            var categories = await _unitOfWork.Categories.GetAllActiveAsync();
            var suppliers = await _unitOfWork.Suppliers.GetAllActiveAsync();

            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name");
        }
    }
}
