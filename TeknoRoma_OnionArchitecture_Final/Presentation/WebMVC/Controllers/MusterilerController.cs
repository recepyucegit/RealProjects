// ===================================================================================
// TEKNOROMA MVC - CUSTOMERS CONTROLLER
// ===================================================================================

using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebMVC.Controllers
{
    /// <summary>
    /// Musteri Yonetimi Controller
    /// </summary>
    public class MusterilerController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MusterilerController> _logger;

        public MusterilerController(
            IUnitOfWork unitOfWork,
            ILogger<MusterilerController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // INDEX - MUSTERI LISTESI
        public async Task<IActionResult> Index()
        {
            try
            {
                var customers = await _unitOfWork.Customers.GetAllAsync();
                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Musteriler listelenirken hata olustu");
                TempData["Error"] = "Musteriler yuklenirken bir hata olustu.";
                return View(new List<Customer>());
            }
        }

        // DETAILS - MUSTERI DETAY
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(id);
                if (customer == null)
                {
                    TempData["Error"] = "Musteri bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }
                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Musteri detaylari yuklenirken hata. CustomerId: {Id}", id);
                TempData["Error"] = "Musteri detaylari yuklenirken hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // CREATE - GET
        public IActionResult Create()
        {
            return View();
        }

        // CREATE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(customer);
                }

                customer.RegistrationDate = DateTime.Now;
                customer.IsActive = true;

                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Musteri basariyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Musteri eklenirken hata");
                TempData["Error"] = "Musteri eklenirken hata olustu.";
                return View(customer);
            }
        }

        // EDIT - GET
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(id);
                if (customer == null)
                {
                    TempData["Error"] = "Musteri bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }
                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Musteri duzenleme formu yuklenirken hata. CustomerId: {Id}", id);
                TempData["Error"] = "Form yuklenirken hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // EDIT - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            try
            {
                if (id != customer.Id)
                {
                    TempData["Error"] = "ID uyusmazligi.";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    return View(customer);
                }

                await _unitOfWork.Customers.UpdateAsync(customer);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Musteri basariyla guncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Musteri guncellenirken hata. CustomerId: {Id}", id);
                TempData["Error"] = "Musteri guncellenirken hata olustu.";
                return View(customer);
            }
        }

        // DELETE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(id);
                if (customer == null)
                {
                    TempData["Error"] = "Musteri bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }

                await _unitOfWork.Customers.DeleteAsync(customer);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Musteri basariyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Musteri silinirken hata. CustomerId: {Id}", id);
                TempData["Error"] = "Musteri silinirken hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
