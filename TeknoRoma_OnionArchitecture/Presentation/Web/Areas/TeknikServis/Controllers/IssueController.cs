using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers;

namespace Web.Areas.TeknikServis.Controllers
{
    [Area("TeknikServis")]
    [Authorize(Roles = "TeknikServis")]
    public class IssueController : BaseController
    {
        public IssueController(IUnitOfWork unitOfWork, ILogger<IssueController> logger)
            : base(unitOfWork, logger) { }

        // GET: /TeknikServis/Issue/Open
        public async Task<IActionResult> Open()
        {
            var issues = await _unitOfWork.TechnicalServices.GetOpenIssuesByPriorityAsync();
            return View(issues);
        }

        // GET: /TeknikServis/Issue/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /TeknikServis/Issue/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TechnicalService model)
        {
            try
            {
                var userId = GetCurrentUserId();
                var employee = await _unitOfWork.Employees.GetByIdentityUserIdAsync(userId);

                model.ServiceNumber = await _unitOfWork.TechnicalServices.GenerateServiceNumberAsync();
                model.StoreId = employee!.StoreId;
                model.ReportedByEmployeeId = employee.ID;
                model.ReportedDate = DateTime.Now;

                await _unitOfWork.TechnicalServices.AddAsync(model);
                await _unitOfWork.SaveChangesAsync();

                ShowSuccessMessage($"Sorun kaydedildi. Servis No: {model.ServiceNumber}");
                return RedirectToAction("Open");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sorun kaydedilirken hata");
                ShowErrorMessage("Hata: " + ex.Message);
                return View(model);
            }
        }
    }
}
