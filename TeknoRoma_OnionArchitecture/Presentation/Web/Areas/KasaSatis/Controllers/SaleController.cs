using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers;
using Web.Models.Sale;

namespace Web.Areas.KasaSatis.Controllers
{
    [Area("KasaSatis")]
    [Authorize(Roles = "KasaSatis")]
    public class SaleController : BaseController
    {
        public SaleController(IUnitOfWork unitOfWork, ILogger<SaleController> logger)
            : base(unitOfWork, logger) { }

        // GET: /KasaSatis/Sale/Create
        public IActionResult Create()
        {
            var model = new SaleCreateViewModel();
            return View(model);
        }

        // POST: /KasaSatis/Sale/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaleCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = GetCurrentUserId();
                var employee = await _unitOfWork.Employees.GetByIdentityUserIdAsync(userId);

                // Sale oluştur
                var sale = new Sale
                {
                    SaleNumber = await _unitOfWork.Sales.GenerateSaleNumberAsync(),
                    SaleDate = DateTime.Now,
                    CustomerId = model.CustomerId,
                    EmployeeId = employee!.ID,
                    StoreId = employee.StoreId,
                    Status = SaleStatus.Tamamlandi,
                    PaymentType = model.PaymentType,
                    Subtotal = model.Subtotal,
                    TaxAmount = model.TaxAmount,
                    DiscountAmount = model.DiscountAmount,
                    TotalAmount = model.TotalAmount,
                    CashRegisterNumber = model.CashRegisterNumber,
                    Notes = model.Notes
                };

                await _unitOfWork.Sales.AddAsync(sale);
                await _unitOfWork.SaveChangesAsync();

                // SaleDetails oluştur ve stok azalt
                foreach (var item in model.Items)
                {
                    var saleDetail = new SaleDetail
                    {
                        SaleId = sale.ID,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity,
                        DiscountPercentage = item.DiscountPercentage,
                        Subtotal = item.Subtotal,
                        DiscountAmount = item.DiscountAmount,
                        TotalAmount = item.TotalAmount
                    };

                    await _unitOfWork.SaleDetails.AddAsync(saleDetail);

                    // Stok azalt
                    await _unitOfWork.Products.DecreaseStockAsync(item.ProductId, item.Quantity);
                }

                await _unitOfWork.SaveChangesAsync();

                ShowSuccessMessage($"Satış başarıyla tamamlandı. Satış No: {sale.SaleNumber}");
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Satış oluşturulurken hata oluştu");
                ShowErrorMessage("Satış oluşturulurken bir hata oluştu: " + ex.Message);
                return View(model);
            }
        }

        // GET: /KasaSatis/Sale/SearchProduct
        public async Task<IActionResult> SearchProduct(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return Json(new { success = false, message = "Barkod giriniz" });
            }

            var product = await _unitOfWork.Products.GetByBarcodeAsync(barcode);

            if (product == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı" });
            }

            if (!product.IsAvailable)
            {
                return Json(new { success = false, message = "Ürün satışa uygun değil" });
            }

            return Json(new
            {
                success = true,
                product = new
                {
                    id = product.ID,
                    name = product.Name,
                    barcode = product.Barcode,
                    unitPrice = product.UnitPrice,
                    unitsInStock = product.UnitsInStock
                }
            });
        }
    }
}
