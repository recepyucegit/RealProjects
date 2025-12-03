// ===================================================================================
// TEKNOROMA MVC - TECHNICAL SERVICES CONTROLLER
// ===================================================================================

using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace WebMVC.Controllers
{
    public class TechnicalServicesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TechnicalServicesController> _logger;

        public TechnicalServicesController(IUnitOfWork unitOfWork, ILogger<TechnicalServicesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var services = await _unitOfWork.TechnicalServices.GetAllAsync();
                return View(services);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Teknik servisler listelenirken hata");
                TempData["Error"] = "Teknik servisler yuklenirken hata olustu.";
                return View(new List<TechnicalService>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var service = await _unitOfWork.TechnicalServices.GetByIdAsync(id);
                if (service == null)
                {
                    TempData["Error"] = "Teknik servis bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }
                return View(service);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Teknik servis detay yuklenirken hata. ServiceId: {Id}", id);
                TempData["Error"] = "Detay yuklenirken hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var customers = await _unitOfWork.Customers.GetAllAsync();
                ViewBag.Customers = customers;

                var products = await _unitOfWork.Products.GetAllAsync();
                ViewBag.Products = products;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Teknik servis ekleme formu yuklenirken hata");
                TempData["Error"] = "Form yuklenirken hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TechnicalService service)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(service);

                service.ReportedDate = DateTime.Now;
                service.Status = TechnicalServiceStatus.Acik;

                await _unitOfWork.TechnicalServices.AddAsync(service);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Teknik servis talebi basariyla olusturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Teknik servis eklenirken hata");
                TempData["Error"] = "Teknik servis eklenirken hata olustu.";
                return View(service);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var service = await _unitOfWork.TechnicalServices.GetByIdAsync(id);
                if (service == null)
                {
                    TempData["Error"] = "Teknik servis bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }

                var customers = await _unitOfWork.Customers.GetAllAsync();
                ViewBag.Customers = customers;

                var products = await _unitOfWork.Products.GetAllAsync();
                ViewBag.Products = products;

                return View(service);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Teknik servis duzenleme formu yuklenirken hata. ServiceId: {Id}", id);
                TempData["Error"] = "Form yuklenirken hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TechnicalService service)
        {
            try
            {
                if (id != service.Id)
                {
                    TempData["Error"] = "ID uyusmazligi.";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                    return View(service);

                await _unitOfWork.TechnicalServices.UpdateAsync(service);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Teknik servis basariyla guncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Teknik servis guncellenirken hata. ServiceId: {Id}", id);
                TempData["Error"] = "Teknik servis guncellenirken hata olustu.";
                return View(service);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var service = await _unitOfWork.TechnicalServices.GetByIdAsync(id);
                if (service == null)
                {
                    TempData["Error"] = "Teknik servis bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }

                await _unitOfWork.TechnicalServices.DeleteAsync(service);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Teknik servis basariyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Teknik servis silinirken hata. ServiceId: {Id}", id);
                TempData["Error"] = "Teknik servis silinirken hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
