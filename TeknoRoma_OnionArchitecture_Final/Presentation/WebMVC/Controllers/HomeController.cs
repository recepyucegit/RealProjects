// ===================================================================================
// TEKNOROMA MVC - HOME CONTROLLER
// ===================================================================================
//
// Ana sayfa ve dashboard icin controller.
//
// DASHBOARD OZELLIKLERI:
// - Gunluk satis ozeti
// - Kritik stok uyarilari
// - Bekleyen teknik servis talepleri
// - Odenmemis giderler
// - Hizli erisim linkleri
//
// ===================================================================================

using Application.Repositories;
using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using WebMVC.Models;

namespace WebMVC.Controllers
{
    /// <summary>
    /// Ana Sayfa ve Dashboard Controller
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IUnitOfWork unitOfWork,
            IExchangeRateService exchangeRateService,
            ILogger<HomeController> logger)
        {
            _unitOfWork = unitOfWork;
            _exchangeRateService = exchangeRateService;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard - Ana sayfa
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();

            try
            {
                // Gunluk satis toplami
                model.TodaySalesTotal = await _unitOfWork.Sales.GetDailyTotalAsync(DateTime.Today);

                // Aylik satis toplami
                model.MonthlySalesTotal = await _unitOfWork.Sales.GetMonthlyTotalAsync(
                    DateTime.Today.Year, DateTime.Today.Month);

                // Kritik stok urun sayisi
                var lowStockProducts = await _unitOfWork.Products.GetLowStockProductsAsync();
                model.LowStockProductCount = lowStockProducts.Count;

                // Bekleyen satis sayisi (Depo icin)
                var pendingSales = await _unitOfWork.Sales.GetByStatusAsync(SaleStatus.Beklemede);
                model.PendingSalesCount = pendingSales.Count;

                // Acik teknik servis talep sayisi
                model.OpenTicketsCount = await _unitOfWork.TechnicalServices.GetOpenIssuesCountAsync();

                // Odenmemis gider sayisi
                var unpaidExpenses = await _unitOfWork.Expenses.GetUnpaidExpensesAsync();
                model.UnpaidExpensesCount = unpaidExpenses.Count;

                // Doviz kurlari
                model.UsdRate = await _exchangeRateService.GetUsdRateAsync();
                model.EurRate = await _exchangeRateService.GetEurRateAsync();

                model.LastUpdated = DateTime.Now;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard verileri yuklenirken hata");
                TempData["Error"] = "Dashboard verileri yuklenirken bir hata olustu.";
            }

            return View(model);
        }

        /// <summary>
        /// Hakkinda sayfasi
        /// </summary>
        public IActionResult About()
        {
            return View();
        }

        /// <summary>
        /// Hata sayfasi
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
