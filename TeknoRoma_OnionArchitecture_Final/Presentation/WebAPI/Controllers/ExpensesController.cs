// ===================================================================================
// TEKNOROMA - EXPENSES CONTROLLER
// ===================================================================================
//
// Gider yonetimi icin API endpoint'leri.
// Finansal islemler ve odeme takibi.
//
// TEKNOROMA GEREKSINIMLERI:
// - Gider kategorileri: Calisan odemeleri, Teknik altyapi, Faturalar, Diger
// - Odeme takibi (Odendi/Odenmedi)
// - Aylik gider raporlari
// - Doviz kuru ile gecmis tarihleri goruntuleme
//
// SENARYO (Feyza - Muhasebe):
// "Subeyle ilgili tum para giris-cikis islemlerini ben takip ediyorum.
// Windows Uygulamasini kullanarak tum odemeleri yapabilmeliyim.
// Aylik olarak parasal anlamda tum giris cikislari rapor olarak gorebilmeliyim."
//
// ===================================================================================

using Application.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Gider Yonetimi API Controller
    ///
    /// Finansal islemler, odeme takibi, gider raporlari
    /// </summary>
    [Authorize(Policy = "FinansYonetimi")]
    public class ExpensesController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ILogger<ExpensesController> _logger;

        public ExpensesController(
            IUnitOfWork unitOfWork,
            IExchangeRateService exchangeRateService,
            ILogger<ExpensesController> logger)
        {
            _unitOfWork = unitOfWork;
            _exchangeRateService = exchangeRateService;
            _logger = logger;
        }

        /// <summary>
        /// Tum giderleri listele
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Expense>>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var expenses = await _unitOfWork.Expenses.GetAllAsync();
            return Success(expenses);
        }

        /// <summary>
        /// ID ile gider getir
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<Expense>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
            if (expense == null)
                return NotFoundResponse($"ID: {id} olan gider bulunamadi");

            return Success(expense);
        }

        /// <summary>
        /// Gider numarasi ile getir
        /// </summary>
        /// <param name="expenseNumber">Gider numarasi (G-YYYY-NNNNN)</param>
        [HttpGet("number/{expenseNumber}")]
        [ProducesResponseType(typeof(ApiResponse<Expense>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByExpenseNumber(string expenseNumber)
        {
            var expense = await _unitOfWork.Expenses.GetByExpenseNumberAsync(expenseNumber);
            if (expense == null)
                return NotFoundResponse($"Gider no: {expenseNumber} bulunamadi");

            return Success(expense);
        }

        /// <summary>
        /// Gider turune gore listele
        /// </summary>
        /// <remarks>
        /// GIDER TURLERI (Feyza):
        /// - CalisanOdemesi: Maas, prim
        /// - TeknikAltyapi: IT giderleri
        /// - Fatura: Elektrik, su, dogalgaz
        /// - Diger: Kategori disi giderler
        /// </remarks>
        /// <param name="expenseType">Gider turu</param>
        [HttpGet("type/{expenseType}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Expense>>), 200)]
        public async Task<IActionResult> GetByExpenseType(ExpenseType expenseType)
        {
            var expenses = await _unitOfWork.Expenses.GetByExpenseTypeAsync(expenseType);
            return Success(expenses);
        }

        /// <summary>
        /// Magaza giderleri
        /// </summary>
        /// <param name="storeId">Magaza ID</param>
        [HttpGet("store/{storeId:int}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Expense>>), 200)]
        public async Task<IActionResult> GetByStore(int storeId)
        {
            var expenses = await _unitOfWork.Expenses.GetByStoreAsync(storeId);
            return Success(expenses);
        }

        /// <summary>
        /// Tarih araligina gore giderler
        /// </summary>
        /// <param name="startDate">Baslangic tarihi</param>
        /// <param name="endDate">Bitis tarihi</param>
        [HttpGet("date-range")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Expense>>), 200)]
        public async Task<IActionResult> GetByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var expenses = await _unitOfWork.Expenses.GetByDateRangeAsync(startDate, endDate);
            return Success(expenses);
        }

        /// <summary>
        /// Calisan giderleri (maas, prim)
        /// </summary>
        /// <param name="employeeId">Calisan ID</param>
        [HttpGet("employee/{employeeId:int}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Expense>>), 200)]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var expenses = await _unitOfWork.Expenses.GetByEmployeeAsync(employeeId);
            return Success(expenses);
        }

        /// <summary>
        /// Odenmemis giderler
        /// </summary>
        /// <remarks>
        /// Nakit akis yonetimi icin kritik.
        /// Vadesi gelen odemelerin takibi.
        /// </remarks>
        [HttpGet("unpaid")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Expense>>), 200)]
        public async Task<IActionResult> GetUnpaidExpenses()
        {
            var expenses = await _unitOfWork.Expenses.GetUnpaidExpensesAsync();
            return Success(expenses, $"{expenses.Count} odenmemis gider var");
        }

        /// <summary>
        /// Aylik toplam gider
        /// </summary>
        /// <remarks>
        /// SENARYO (Feyza - Muhasebe):
        /// "Aylik olarak parasal anlamda tum giris cikislari
        /// rapor olarak gorebilmeliyim"
        /// </remarks>
        /// <param name="year">Yil</param>
        /// <param name="month">Ay</param>
        /// <param name="storeId">Magaza ID (opsiyonel)</param>
        [HttpGet("monthly-total")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> GetMonthlyTotal(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] int? storeId = null)
        {
            var total = await _unitOfWork.Expenses.GetMonthlyTotalAsync(year, month, storeId);

            // Doviz kuru bilgisi ekle
            var usdRate = await _exchangeRateService.GetUsdRateAsync();
            var eurRate = await _exchangeRateService.GetEurRateAsync();

            return Success(new
            {
                Year = year,
                Month = month,
                StoreId = storeId,
                TotalAmount = total,
                Currency = "TRY",
                TotalInUSD = Math.Round(total / usdRate, 2),
                TotalInEUR = Math.Round(total / eurRate, 2),
                ExchangeRates = new
                {
                    USD = usdRate,
                    EUR = eurRate
                }
            });
        }

        /// <summary>
        /// Yeni gider olustur
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Expense>), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] Expense expense)
        {
            // Gider numarasi olustur
            expense.ExpenseNumber = await _unitOfWork.Expenses.GenerateExpenseNumberAsync();
            expense.ExpenseDate = DateTime.Now;

            var created = await _unitOfWork.Expenses.AddAsync(expense);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Yeni gider kaydedildi: {ExpenseNumber} - {Amount} TL - {Type}",
                expense.ExpenseNumber, expense.Amount, expense.ExpenseType);

            return Created(created);
        }

        /// <summary>
        /// Gider guncelle
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<Expense>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] Expense expense)
        {
            var existing = await _unitOfWork.Expenses.GetByIdAsync(id);
            if (existing == null)
                return NotFoundResponse($"ID: {id} olan gider bulunamadi");

            existing.Description = expense.Description;
            existing.Amount = expense.Amount;
            existing.ExpenseType = expense.ExpenseType;
            existing.DueDate = expense.DueDate;

            await _unitOfWork.Expenses.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            return Success(existing, "Gider guncellendi");
        }

        /// <summary>
        /// Gideri odendi olarak isaretle
        /// </summary>
        /// <param name="id">Gider ID</param>
        [HttpPut("{id:int}/pay")]
        [ProducesResponseType(typeof(ApiResponse<Expense>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
            if (expense == null)
                return NotFoundResponse($"ID: {id} olan gider bulunamadi");

            expense.IsPaid = true;
            expense.PaymentDate = DateTime.Now;

            await _unitOfWork.Expenses.UpdateAsync(expense);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Gider odendi: {ExpenseNumber} - {Amount} TL",
                expense.ExpenseNumber, expense.Amount);

            return Success(expense, "Gider odendi olarak isaretlendi");
        }

        /// <summary>
        /// Gider sil (Soft Delete)
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
            if (expense == null)
                return NotFoundResponse($"ID: {id} olan gider bulunamadi");

            await _unitOfWork.Expenses.SoftDeleteAsync(expense);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    }
}
