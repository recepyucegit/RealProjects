using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers;

namespace Web.Areas.Muhasebe.Controllers
{
    [Area("Muhasebe")]
    [Authorize(Roles = "Muhasebe")]
    public class ExpenseController : BaseController
    {
        public ExpenseController(IUnitOfWork unitOfWork, ILogger<ExpenseController> logger)
            : base(unitOfWork, logger) { }

        // GET: /Muhasebe/Expense/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Muhasebe/Expense/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expense model)
        {
            try
            {
                var userId = GetCurrentUserId();
                var employee = await _unitOfWork.Employees.GetByIdentityUserIdAsync(userId);

                model.ExpenseNumber = await _unitOfWork.Expenses.GenerateExpenseNumberAsync();
                model.StoreId = employee!.StoreId;

                // TRY ise kur 1
                if (model.Currency == Currency.TRY)
                {
                    model.ExchangeRate = 1;
                    model.AmountInTRY = model.Amount;
                }
                else
                {
                    model.AmountInTRY = model.Amount * (model.ExchangeRate ?? 1);
                }

                await _unitOfWork.Expenses.AddAsync(model);
                await _unitOfWork.SaveChangesAsync();

                ShowSuccessMessage($"Gider kaydedildi. Gider No: {model.ExpenseNumber}");
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gider kaydedilirken hata");
                ShowErrorMessage("Hata: " + ex.Message);
                return View(model);
            }
        }

        // GET: /Muhasebe/Expense/Unpaid
        public async Task<IActionResult> Unpaid()
        {
            var expenses = await _unitOfWork.Expenses.GetUnpaidExpensesAsync();
            return View(expenses);
        }
    }
}
