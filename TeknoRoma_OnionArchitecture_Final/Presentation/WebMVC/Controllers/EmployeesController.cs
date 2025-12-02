// ===================================================================================
// TEKNOROMA MVC - EMPLOYEES CONTROLLER
// ===================================================================================

using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebMVC.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(IUnitOfWork unitOfWork, ILogger<EmployeesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var employees = await _unitOfWork.Employees.GetAllAsync();
                return View(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Calisanlar listelenirken hata");
                TempData["Error"] = "Calisanlar yuklenirken hata olustu.";
                return View(new List<Employee>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(id);
                if (employee == null)
                {
                    TempData["Error"] = "Calisan bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }
                return View(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Calisan detay yuklenirken hata. EmployeeId: {Id}", id);
                TempData["Error"] = "Detay yuklenirken hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var stores = await _unitOfWork.Stores.GetAllAsync();
                ViewBag.Stores = stores;

                var departments = await _unitOfWork.Departments.GetAllAsync();
                ViewBag.Departments = departments;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Calisan ekleme formu yuklenirken hata");
                TempData["Error"] = "Form yuklenirken hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(employee);

                employee.HireDate = DateTime.Now;
                employee.IsActive = true;

                await _unitOfWork.Employees.AddAsync(employee);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Calisan basariyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Calisan eklenirken hata");
                TempData["Error"] = "Calisan eklenirken hata olustu.";
                return View(employee);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(id);
                if (employee == null)
                {
                    TempData["Error"] = "Calisan bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }

                var stores = await _unitOfWork.Stores.GetAllAsync();
                ViewBag.Stores = stores;

                var departments = await _unitOfWork.Departments.GetAllAsync();
                ViewBag.Departments = departments;

                return View(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Calisan duzenleme formu yuklenirken hata. EmployeeId: {Id}", id);
                TempData["Error"] = "Form yuklenirken hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            try
            {
                if (id != employee.Id)
                {
                    TempData["Error"] = "ID uyusmazligi.";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                    return View(employee);

                await _unitOfWork.Employees.UpdateAsync(employee);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Calisan basariyla guncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Calisan guncellenirken hata. EmployeeId: {Id}", id);
                TempData["Error"] = "Calisan guncellenirken hata olustu.";
                return View(employee);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(id);
                if (employee == null)
                {
                    TempData["Error"] = "Calisan bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }

                await _unitOfWork.Employees.DeleteAsync(employee);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Calisan basariyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Calisan silinirken hata. EmployeeId: {Id}", id);
                TempData["Error"] = "Calisan silinirken hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
