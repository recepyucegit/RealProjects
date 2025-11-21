using Application.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers;

namespace Web.Areas.SubeYoneticisi.Controllers
{
    [Area("SubeYoneticisi")]
    [Authorize(Roles = "SubeYoneticisi")]
    public class ReportController : BaseController
    {
        public ReportController(IUnitOfWork unitOfWork, ILogger<ReportController> logger)
            : base(unitOfWork, logger) { }

        // GET: /SubeYoneticisi/Report/Sales
        public async Task<IActionResult> Sales(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.Today.AddMonths(-1);
            endDate ??= DateTime.Today;

            var sales = await _unitOfWork.Sales.GetByDateRangeAsync(startDate.Value, endDate.Value);

            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            ViewBag.TotalAmount = sales.Sum(s => s.TotalAmount);

            return View(sales);
        }

        // GET: /SubeYoneticisi/Report/TopProducts
        public async Task<IActionResult> TopProducts(int count = 10)
        {
            var products = await _unitOfWork.Products.GetTopSellingProductsAsync(count);
            return View(products);
        }

        // GET: /SubeYoneticisi/Report/EmployeePerformance
        public async Task<IActionResult> EmployeePerformance()
        {
            var employees = await _unitOfWork.Employees.GetEmployeesWithSalesQuotaAsync();
            return View(employees);
        }
    }
}
