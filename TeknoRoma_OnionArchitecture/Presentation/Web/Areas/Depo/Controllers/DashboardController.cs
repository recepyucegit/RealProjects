using Application.Repositories;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers;
using Web.Models.Report;

namespace Web.Areas.Depo.Controllers
{
    [Area("Depo")]
    [Authorize(Roles = "Depo")]
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

            var model = new DashboardViewModel
            {
                UserName = $"{employee.FirstName} {employee.LastName}",
                Role = "Depo Temsilcisi",
                StoreName = employee.Store?.Name ?? "Bilinmeyen",
                DepartmentName = employee.Department?.Name ?? "Bilinmeyen",
                PendingOrdersCount = (await _unitOfWork.Sales.GetByStatusAsync(SaleStatus.Hazirlaniyor)).Count,
                CriticalStockCount = (await _unitOfWork.Products.GetCriticalStockProductsAsync()).Count,
                OutOfStockProductsCount = (await _unitOfWork.Products.GetOutOfStockProductsAsync()).Count
            };

            return View(model);
        }
    }
}
