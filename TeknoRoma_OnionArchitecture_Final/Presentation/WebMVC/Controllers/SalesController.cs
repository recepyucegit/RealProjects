// ===================================================================================
// TEKNOROMA MVC - SALES CONTROLLER
// ===================================================================================
//
// Satis islemleri icin controller.
// POS sistemi ve satis yonetimi.
//
// ROLLER:
// - Kasa: Yeni satis yapma
// - Sube Muduru: Tum islemler
// - Depo: Sadece goruntuleme
//
// ===================================================================================

using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebMVC.Controllers
{
    /// <summary>
    /// Satis Yonetimi Controller
    /// </summary>
    public class SalesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SalesController> _logger;

        public SalesController(
            IUnitOfWork unitOfWork,
            ILogger<SalesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // =========================================================================
        // INDEX - SATIS LISTESI
        // =========================================================================

        /// <summary>
        /// Tum satislari listeler
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var sales = await _unitOfWork.Sales.GetAllAsync();
                return View(sales);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Satislar listelenirken hata olustu");
                TempData["Error"] = "Satislar yuklenirken bir hata olustu.";
                return View(new List<Sale>());
            }
        }

        /// <summary>
        /// Bekleyen satislar
        /// </summary>
        public async Task<IActionResult> Pending()
        {
            try
            {
                var sales = await _unitOfWork.Sales.GetByStatusAsync(SaleStatus.Beklemede);
                ViewData["Title"] = "Bekleyen Satislar";
                ViewData["Status"] = "Beklemede";
                return View("Index", sales);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bekleyen satislar listelenirken hata olustu");
                TempData["Error"] = "Bekleyen satislar yuklenirken bir hata olustu.";
                return View("Index", new List<Sale>());
            }
        }

        // =========================================================================
        // DETAILS - SATIS DETAY
        // =========================================================================

        /// <summary>
        /// Satis detaylarini goster (Fis goruntuleme)
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var sale = await _unitOfWork.Sales.GetByIdAsync(id);
                if (sale == null)
                {
                    TempData["Error"] = "Satis bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }

                // Sale details'i de yukle
                var saleDetails = await _unitOfWork.SaleDetails
                    .GetAllAsync(sd => sd.SaleId == id);

                ViewBag.SaleDetails = saleDetails;
                return View(sale);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Satis detaylari yuklenirken hata olustu. SaleId: {Id}", id);
                TempData["Error"] = "Satis detaylari yuklenirken bir hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =========================================================================
        // CREATE - YENI SATIS
        // =========================================================================

        /// <summary>
        /// Yeni satis formu
        /// </summary>
        public async Task<IActionResult> Create()
        {
            try
            {
                // Aktif urunleri yukle
                var products = await _unitOfWork.Products
                    .GetAllAsync(p => p.IsActive && p.UnitsInStock > 0);
                ViewBag.Products = products;

                // Aktif musterileri yukle
                var customers = await _unitOfWork.Customers
                    .GetAllAsync(c => c.IsActive);
                ViewBag.Customers = customers;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yeni satis formu yuklenirken hata olustu");
                TempData["Error"] = "Form yuklenirken bir hata olustu.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Yeni satis olustur (POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sale sale, List<SaleDetail> details)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Lutfen tum alanlari doldurun.";
                    return View(sale);
                }

                // Transaction baslat
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // 1. Sale kaydini olustur
                    sale.SaleDate = DateTime.Now;
                    sale.Status = SaleStatus.Tamamlandi;

                    // Kullanici bilgisini session'dan al
                    var employeeId = HttpContext.Session.GetInt32("UserId");
                    if (employeeId.HasValue)
                    {
                        sale.EmployeeId = employeeId.Value;
                    }

                    await _unitOfWork.Sales.AddAsync(sale);
                    await _unitOfWork.SaveChangesAsync();

                    // 2. Sale details kayitlarini olustur ve stok dusur
                    decimal totalAmount = 0;
                    foreach (var detail in details)
                    {
                        detail.SaleId = sale.Id;

                        // Urun bilgisini al
                        var product = await _unitOfWork.Products.GetByIdAsync(detail.ProductId);
                        if (product == null)
                        {
                            throw new Exception($"Urun bulunamadi: {detail.ProductId}");
                        }

                        // Stok kontrolu
                        if (product.UnitsInStock < detail.Quantity)
                        {
                            throw new Exception($"Yetersiz stok: {product.Name}");
                        }

                        // Stok dusur
                        product.UnitsInStock -= detail.Quantity;
                        await _unitOfWork.Products.UpdateAsync(product);

                        // Detail kaydet
                        detail.UnitPrice = product.UnitPrice;
                        detail.Subtotal = detail.Quantity * detail.UnitPrice;
                        totalAmount += detail.Subtotal;

                        await _unitOfWork.SaleDetails.AddAsync(detail);
                    }

                    // 3. Toplam tutari guncelle
                    sale.TotalAmount = totalAmount;
                    await _unitOfWork.Sales.UpdateAsync(sale);
                    await _unitOfWork.SaveChangesAsync();

                    // Transaction'i onayla
                    await _unitOfWork.CommitTransactionAsync();

                    TempData["Success"] = $"Satis basariyla olusturuldu. Fis No: {sale.SaleNumber}";
                    return RedirectToAction(nameof(Details), new { id = sale.Id });
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Satis olusturulurken hata olustu");
                TempData["Error"] = $"Satis olusturulurken hata: {ex.Message}";
                return View(sale);
            }
        }

        // =========================================================================
        // STATUS UPDATE
        // =========================================================================

        /// <summary>
        /// Satis durumu guncelle
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, SaleStatus status)
        {
            try
            {
                var sale = await _unitOfWork.Sales.GetByIdAsync(id);
                if (sale == null)
                {
                    TempData["Error"] = "Satis bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }

                sale.Status = status;
                await _unitOfWork.Sales.UpdateAsync(sale);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Satis durumu guncellendi.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Satis durumu guncellenirken hata olustu. SaleId: {Id}", id);
                TempData["Error"] = "Durum guncellenirken bir hata olustu.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // =========================================================================
        // CANCEL - SATIS IPTAL
        // =========================================================================

        /// <summary>
        /// Satisi iptal et ve stoklari geri ekle
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var sale = await _unitOfWork.Sales.GetByIdAsync(id);
                if (sale == null)
                {
                    TempData["Error"] = "Satis bulunamadi.";
                    return RedirectToAction(nameof(Index));
                }

                if (sale.Status == SaleStatus.Iptal)
                {
                    TempData["Warning"] = "Satis zaten iptal edilmis.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Transaction baslat
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // 1. Satisi iptal et
                    sale.Status = SaleStatus.Iptal;
                    await _unitOfWork.Sales.UpdateAsync(sale);

                    // 2. Stoklari geri ekle
                    var details = await _unitOfWork.SaleDetails
                        .GetAllAsync(sd => sd.SaleId == id);

                    foreach (var detail in details)
                    {
                        var product = await _unitOfWork.Products.GetByIdAsync(detail.ProductId);
                        if (product != null)
                        {
                            product.UnitsInStock += detail.Quantity;
                            await _unitOfWork.Products.UpdateAsync(product);
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    TempData["Success"] = "Satis iptal edildi ve stoklar geri eklendi.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Satis iptal edilirken hata olustu. SaleId: {Id}", id);
                TempData["Error"] = "Satis iptal edilirken bir hata olustu.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
