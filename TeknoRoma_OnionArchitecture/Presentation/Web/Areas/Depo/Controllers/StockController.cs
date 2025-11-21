using Application.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers;

namespace Web.Areas.Depo.Controllers
{
    [Area("Depo")]
    [Authorize(Roles = "Depo")]
    public class StockController : BaseController
    {
        public StockController(IUnitOfWork unitOfWork, ILogger<StockController> logger)
            : base(unitOfWork, logger) { }

        // GET: /Depo/Stock/Critical
        public async Task<IActionResult> Critical()
        {
            var products = await _unitOfWork.Products.GetCriticalStockProductsAsync();
            return View(products);
        }

        // GET: /Depo/Stock/OutOfStock
        public async Task<IActionResult> OutOfStock()
        {
            var products = await _unitOfWork.Products.GetOutOfStockProductsAsync();
            return View(products);
        }
    }
}
