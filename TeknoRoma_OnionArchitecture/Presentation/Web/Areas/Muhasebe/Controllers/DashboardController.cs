using Application.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers;
using Web.Models.Report;

namespace Web.Areas.Muhasebe.Controllers
{
    [Area("Muhasebe")]
    [Authorize(Roles = "Muhasebe")]
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
            var unpaidExpenses = await _unitOfWork.Expenses.GetUnpaidExpensesAsync();

            var model = new DashboardViewModel
            {
                UserName = $"{employee.FirstName} {employee.LastName}",
                Role = "Muhasebe Temsilcisi",
                StoreName = employee.Store?.Name ?? "Bilinmeyen",
                DepartmentName = employee.Department?.Name ?? "Bilinmeyen",
                UnpaidExpensesCount = unpaidExpenses.Count,
                UnpaidExpensesAmount = unpaidExpenses.Sum(e => e.AmountInTRY),
                MonthlyExpenses = await _unitOfWork.Expenses.GetMonthlyTotalExpenseAsync(today.Year, today.Month, employee.StoreId)
            };

            return View(model);
        }
    }
}
