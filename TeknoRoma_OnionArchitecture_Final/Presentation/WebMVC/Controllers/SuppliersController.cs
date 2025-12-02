// ===================================================================================
// TEKNOROMA MVC - SUPPLIERS CONTROLLER
// ===================================================================================

using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebMVC.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SuppliersController> _logger;

        public SuppliersController(IUnitOfWork unitOfWork, ILogger<SuppliersController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var suppliers = await _unitOfWork.Suppliers.GetAllAsync();
                return View(suppliers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tedarikciler listelenirken hata");
                TempData["Error"] = "Tedarikciler yuklenirken hata olustu.";
                return View(new List<Supplier>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
                if (supplier == null)
                {
                    TempData["Error"] = "Tedarikci bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }
                return View(supplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tedarikci detay yuklenirken hata. SupplierId: {Id}", id);
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
        public async Task<IActionResult> Create(Supplier supplier)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(supplier);

                supplier.IsActive = true;
                await _unitOfWork.Suppliers.AddAsync(supplier);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Tedarikci basariyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tedarikci eklenirken hata");
                TempData["Error"] = "Tedarikci eklenirken hata olustu.";
                return View(supplier);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
                if (supplier == null)
                {
                    TempData["Error"] = "Tedarikci bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }
                return View(supplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tedarikci duzenleme formu yuklenirken hata. SupplierId: {Id}", id);
                TempData["Error"] = "Form yuklenirken hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Supplier supplier)
        {
            try
            {
                if (id != supplier.Id)
                {
                    TempData["Error"] = "ID uyusmazligi.";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                    return View(supplier);

                await _unitOfWork.Suppliers.UpdateAsync(supplier);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Tedarikci basariyla guncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tedarikci guncellenirken hata. SupplierId: {Id}", id);
                TempData["Error"] = "Tedarikci guncellenirken hata olustu.";
                return View(supplier);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
                if (supplier == null)
                {
                    TempData["Error"] = "Tedarikci bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }

                await _unitOfWork.Suppliers.DeleteAsync(supplier);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Tedarikci basariyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tedarikci silinirken hata. SupplierId: {Id}", id);
                TempData["Error"] = "Tedarikci silinirken hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
