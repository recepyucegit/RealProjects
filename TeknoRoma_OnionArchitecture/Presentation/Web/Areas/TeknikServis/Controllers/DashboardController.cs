using Application.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers;
using Web.Models.Report;

namespace Web.Areas.TeknikServis.Controllers
{
    [Area("TeknikServis")]
    [Authorize(Roles = "TeknikServis")]
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

            var openIssues = await _unitOfWork.TechnicalServices.GetOpenIssuesByPriorityAsync();
            var assignedToMe = await _unitOfWork.TechnicalServices.GetByAssignedEmployeeAsync(employee.ID);

            var model = new DashboardViewModel
            {
                UserName = $"{employee.FirstName} {employee.LastName}",
                Role = "Teknik Servis Temsilcisi",
                StoreName = employee.Store?.Name ?? "Bilinmeyen",
                DepartmentName = employee.Department?.Name ?? "Bilinmeyen",
                OpenIssuesCount = openIssues.Count,
                AssignedToMeCount = assignedToMe.Count,
                CriticalIssuesCount = openIssues.Count(i => i.Priority >= 3)
            };

            return View(model);
        }
    }
}
