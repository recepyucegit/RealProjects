using Application.Repositories;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers;
using Web.Models.Report;

namespace Web.Areas.SubeYoneticisi.Controllers
{
    [Area("SubeYoneticisi")]
    [Authorize(Roles = "SubeYoneticisi")]
    public class DashboardController : BaseController
    {
        public DashboardController(IUnitOfWork unitOfWork, ILogger<DashboardController> logger)
            : base(unitOfWork, logger) { }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var employee = await _unitOfWork.Employees.GetByIdentityUserIdAsync(userId);

            if (employee == null)
            {
                ShowErrorMessage("Çalışan kaydınız bulunamadı.");
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            var model = new DashboardViewModel
            {
                UserName = $"{employee.FirstName} {employee.LastName}",
                Role = "Şube Müdürü",
                StoreName = employee.Store?.Name ?? "Bilinmeyen",
                DepartmentName = employee.Department?.Name ?? "Bilinmeyen",

                // Bugünkü satışlar
                TodaysTotalSales = await _unitOfWork.Sales.GetStoreDailySalesTotalAsync(employee.StoreId, today),

                // Aylık satışlar
                MonthlyTotalSales = await _unitOfWork.Sales.GetStoreMonthlySalesTotalAsync(employee.StoreId, today.Year, today.Month),

                // Çalışan sayısı
                TotalEmployees = await _unitOfWork.Employees.CountAsync(e => e.StoreId == employee.StoreId && e.IsActive),

                // Kritik stok
                CriticalStockCount = (await _unitOfWork.Products.GetCriticalStockProductsAsync()).Count
            };

            return View(model);
        }
    }
}
