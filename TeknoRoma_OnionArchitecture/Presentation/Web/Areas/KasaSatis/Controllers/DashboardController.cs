using Application.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers;
using Web.Models.Report;

namespace Web.Areas.KasaSatis.Controllers
{
    [Area("KasaSatis")]
    [Authorize(Roles = "KasaSatis")]
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

            var model = new DashboardViewModel
            {
                UserName = $"{employee.FirstName} {employee.LastName}",
                Role = "Kasa Satış Temsilcisi",
                StoreName = employee.Store?.Name ?? "Bilinmeyen",
                DepartmentName = employee.Department?.Name ?? "Bilinmeyen",

                // Aylık satışlar ve prim
                MonthlySales = await _unitOfWork.Employees.GetEmployeeSalesPerformanceAsync(employee.ID, today.Year, today.Month),
                SalesQuota = employee.SalesQuota,
                CommissionAmount = await _unitOfWork.Employees.CalculateEmployeeCommissionAsync(employee.ID, today.Year, today.Month)
            };

            return View(model);
        }
    }
}
