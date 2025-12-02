// ===================================================================================
// TEKNOROMA MVC - EXPENSES CONTROLLER
// ===================================================================================

using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebMVC.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ExpensesController> _logger;

        public ExpensesController(IUnitOfWork unitOfWork, ILogger<ExpensesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var expenses = await _unitOfWork.Expenses.GetAllAsync();
                return View(expenses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Giderler listelenirken hata");
                TempData["Error"] = "Giderler yuklenirken hata olustu.";
                return View(new List<Expense>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
                if (expense == null)
                {
                    TempData["Error"] = "Gider bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }
                return View(expense);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gider detay yuklenirken hata. ExpenseId: {Id}", id);
                TempData["Error"] = "Detay yuklenirken hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expense expense)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(expense);

                expense.ExpenseDate = DateTime.Now;
                expense.IsPaid = false;

                await _unitOfWork.Expenses.AddAsync(expense);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Gider basariyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gider eklenirken hata");
                TempData["Error"] = "Gider eklenirken hata olustu.";
                return View(expense);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
                if (expense == null)
                {
                    TempData["Error"] = "Gider bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }
                return View(expense);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gider duzenleme formu yuklenirken hata. ExpenseId: {Id}", id);
                TempData["Error"] = "Form yuklenirken hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Expense expense)
        {
            try
            {
                if (id != expense.Id)
                {
                    TempData["Error"] = "ID uyusmazligi.";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                    return View(expense);

                await _unitOfWork.Expenses.UpdateAsync(expense);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Gider basariyla guncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gider guncellenirken hata. ExpenseId: {Id}", id);
                TempData["Error"] = "Gider guncellenirken hata olustu.";
                return View(expense);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
                if (expense == null)
                {
                    TempData["Error"] = "Gider bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }

                await _unitOfWork.Expenses.DeleteAsync(expense);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Gider basariyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gider silinirken hata. ExpenseId: {Id}", id);
                TempData["Error"] = "Gider silinirken hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
