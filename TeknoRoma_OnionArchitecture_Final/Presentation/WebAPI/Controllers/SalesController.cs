// ===================================================================================
// TEKNOROMA - SALES CONTROLLER
// ===================================================================================
//
// Satis islemleri icin API endpoint'leri.
// POS (Point of Sale) sisteminin kalbi.
//
// TEKNOROMA GEREKSINIMLERI:
// - Hizli satis yapabilme (yogun donemlerde kritik)
// - Musteri TC ile otomatik bilgi getirme
// - Satis onayinda depoya bildirim
// - Mobil satislari gorebilme
// - Fis numarasi ile sorgulama
// - Guncel doviz kuru ile fiyat gosterme
//
// KULLANICI SENARYOLARI:
// - Gul (Kasa Satis): Yogun donemlerde hizli satis
// - Fahri (Mobil Satis): Cep bilgisayari ile satis
// - Kerim (Depo): Satislari goruntuleme, urun hazirlama
// - Haluk (Sube Muduru): Satis raporlari
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
    /// Satis Islemleri API Controller
    ///
    /// POS sistemi, satis raporlari, fis yonetimi
    /// </summary>
    [Authorize]
    public class SalesController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ILogger<SalesController> _logger;

        public SalesController(
            IUnitOfWork unitOfWork,
            IExchangeRateService exchangeRateService,
            ILogger<SalesController> logger)
        {
            _unitOfWork = unitOfWork;
            _exchangeRateService = exchangeRateService;
            _logger = logger;
        }

        // =====================================================================
        // GET ENDPOINTS
        // =====================================================================

        /// <summary>
        /// Tum satislari listele
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Sale>>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var sales = await _unitOfWork.Sales.GetAllAsync();
            return Success(sales);
        }

        /// <summary>
        /// ID ile satis getir
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<Sale>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var sale = await _unitOfWork.Sales.GetByIdAsync(id);

            if (sale == null)
                return NotFoundResponse($"ID: {id} olan satis bulunamadi");

            return Success(sale);
        }

        /// <summary>
        /// Fis numarasi ile satis getir
        /// </summary>
        /// <remarks>
        /// SENARYO (Gul - Kasa):
        /// "Cep bilgisayariyla satis yapan arkadas, musteriye bir satis numarasi verse
        /// ben o satis numarasini ekrana girdigimde hemen urunleri gorup fatura kesebilsem"
        ///
        /// Fis numarasi formati: S-2024-00001
        /// </remarks>
        /// <param name="saleNumber">Fis numarasi (S-YYYY-NNNNN)</param>
        [HttpGet("number/{saleNumber}")]
        [ProducesResponseType(typeof(ApiResponse<Sale>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetBySaleNumber(string saleNumber)
        {
            var sale = await _unitOfWork.Sales.GetBySaleNumberAsync(saleNumber);

            if (sale == null)
                return NotFoundResponse($"Fis no: {saleNumber} bulunamadi");

            return Success(sale);
        }

        /// <summary>
        /// Musterinin satislari
        /// </summary>
        /// <param name="customerId">Musteri ID</param>
        [HttpGet("customer/{customerId:int}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Sale>>), 200)]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            var sales = await _unitOfWork.Sales.GetByCustomerAsync(customerId);
            return Success(sales);
        }

        /// <summary>
        /// Calisanin satislari
        /// </summary>
        /// <remarks>
        /// SENARYO (Gul, Fahri - Satis Ekibi):
        /// "Biz de Satis ekibi olarak yaptigimiz satislari,
        /// prime ne kadar yaklastigimizi rapor olarak gormek istiyoruz."
        /// </remarks>
        /// <param name="employeeId">Calisan ID</param>
        [HttpGet("employee/{employeeId:int}")]
        [Authorize(Policy = "SatisYapabilir")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Sale>>), 200)]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var sales = await _unitOfWork.Sales.GetByEmployeeAsync(employeeId);
            return Success(sales);
        }

        /// <summary>
        /// Magazanin satislari
        /// </summary>
        [HttpGet("store/{storeId:int}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Sale>>), 200)]
        public async Task<IActionResult> GetByStore(int storeId)
        {
            var sales = await _unitOfWork.Sales.GetByStoreAsync(storeId);
            return Success(sales);
        }

        /// <summary>
        /// Duruma gore satislar
        /// </summary>
        /// <remarks>
        /// SENARYO (Kerim - Depo):
        /// "Bizim gorevimiz satis yapildigi anda ilgili urunleri
        /// ilgili satisi yapan kasaya birakmak"
        ///
        /// Status = Beklemede olanlari filtrele
        /// </remarks>
        /// <param name="status">Satis durumu</param>
        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Sale>>), 200)]
        public async Task<IActionResult> GetByStatus(SaleStatus status)
        {
            var sales = await _unitOfWork.Sales.GetByStatusAsync(status);
            return Success(sales);
        }

        /// <summary>
        /// Tarih araligina gore satislar
        /// </summary>
        /// <param name="startDate">Baslangic tarihi</param>
        /// <param name="endDate">Bitis tarihi</param>
        [HttpGet("date-range")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Sale>>), 200)]
        public async Task<IActionResult> GetByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var sales = await _unitOfWork.Sales.GetByDateRangeAsync(startDate, endDate);
            return Success(sales);
        }

        /// <summary>
        /// Gunluk toplam ciro
        /// </summary>
        /// <remarks>
        /// Dashboard "Bugunun cirosu" widget'i icin
        /// </remarks>
        /// <param name="date">Tarih (default: bugun)</param>
        /// <param name="storeId">Magaza ID (opsiyonel)</param>
        [HttpGet("daily-total")]
        [Authorize(Policy = "RaporGorebilir")]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 200)]
        public async Task<IActionResult> GetDailyTotal(
            [FromQuery] DateTime? date = null,
            [FromQuery] int? storeId = null)
        {
            var targetDate = date ?? DateTime.Today;
            var total = await _unitOfWork.Sales.GetDailyTotalAsync(targetDate, storeId);

            return Success(new
            {
                Date = targetDate.ToString("yyyy-MM-dd"),
                StoreId = storeId,
                TotalAmount = total,
                Currency = "TRY"
            });
        }

        /// <summary>
        /// Aylik toplam ciro
        /// </summary>
        /// <param name="year">Yil</param>
        /// <param name="month">Ay</param>
        /// <param name="storeId">Magaza ID (opsiyonel)</param>
        [HttpGet("monthly-total")]
        [Authorize(Policy = "RaporGorebilir")]
        [ProducesResponseType(typeof(ApiResponse<decimal>), 200)]
        public async Task<IActionResult> GetMonthlyTotal(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] int? storeId = null)
        {
            var total = await _unitOfWork.Sales.GetMonthlyTotalAsync(year, month, storeId);

            return Success(new
            {
                Year = year,
                Month = month,
                StoreId = storeId,
                TotalAmount = total,
                Currency = "TRY"
            });
        }

        /// <summary>
        /// Bekleyen satislar (Depo icin)
        /// </summary>
        /// <remarks>
        /// SENARYO (Kerim - Depo):
        /// "Ornegin 1.Nolu kasa satis yaptiysa ben Windows Uygulamasinda
        /// aninda bunu gorup urunleri kasaya goturmeliyim.
        /// Ihtiyacim olan bilgiler Satis Numarasi, Urun Bilgisi
        /// ve talebin hangi kasadan geldigi"
        /// </remarks>
        [HttpGet("pending")]
        [Authorize(Policy = "StokYonetimi")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Sale>>), 200)]
        public async Task<IActionResult> GetPendingSales()
        {
            var sales = await _unitOfWork.Sales.GetByStatusAsync(SaleStatus.Beklemede);

            return Success(sales, $"{sales.Count} adet bekleyen satis var");
        }

        // =====================================================================
        // POST ENDPOINTS
        // =====================================================================

        /// <summary>
        /// Yeni satis olustur
        /// </summary>
        /// <remarks>
        /// SATIS AKISI:
        /// 1. Satis kaydedilir
        /// 2. Fis numarasi otomatik olusturulur
        /// 3. Stok dusumu yapilir
        /// 4. Depo bilgilendirilir (Status: Beklemede)
        ///
        /// SENARYO (Gul - Kasa):
        /// "Satisi onayladigim anda onaylanan satisa ait bilgiler
        /// depo bolumune iletilmeli"
        /// </remarks>
        [HttpPost]
        [Authorize(Policy = "SatisYapabilir")]
        [ProducesResponseType(typeof(ApiResponse<Sale>), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateSaleRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Fis numarasi olustur
                var saleNumber = await _unitOfWork.Sales.GenerateSaleNumberAsync();

                // Satis entity'si olustur
                var sale = new Sale
                {
                    SaleNumber = saleNumber,
                    CustomerId = request.CustomerId,
                    EmployeeId = request.EmployeeId,
                    StoreId = request.StoreId,
                    SaleDate = DateTime.Now,
                    Status = SaleStatus.Beklemede,
                    PaymentType = request.PaymentType,
                    TotalAmount = 0
                };

                // Satis kaydet
                var createdSale = await _unitOfWork.Sales.AddAsync(sale);
                await _unitOfWork.SaveChangesAsync();

                decimal totalAmount = 0;

                // Satis detaylarini ekle ve stok dusumu yap
                foreach (var item in request.Items)
                {
                    // Urun kontrolu
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    if (product == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return BadRequestResponse($"Urun bulunamadi: ID {item.ProductId}");
                    }

                    // Stok kontrolu
                    if (product.UnitsInStock < item.Quantity)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return BadRequestResponse(
                            $"Yetersiz stok: {product.Name}. Mevcut: {product.UnitsInStock}, Istenen: {item.Quantity}");
                    }

                    // Satis detayi olustur
                    var saleDetail = new SaleDetail
                    {
                        SaleId = createdSale.Id,
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = product.UnitPrice,
                        Subtotal = product.UnitPrice * item.Quantity,
                        TotalAmount = product.UnitPrice * item.Quantity
                    };

                    await _unitOfWork.SaleDetails.AddAsync(saleDetail);
                    totalAmount += saleDetail.TotalAmount;

                    // Stok dusumu
                    await _unitOfWork.Products.UpdateStockAsync(item.ProductId, -item.Quantity);
                }

                // Toplam tutari guncelle
                createdSale.TotalAmount = totalAmount;
                await _unitOfWork.Sales.UpdateAsync(createdSale);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "Yeni satis olusturuldu: {SaleNumber} - Tutar: {Amount} TL - Calisan: {EmployeeId}",
                    saleNumber, totalAmount, request.EmployeeId);

                return Created(createdSale);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Satis olusturulurken hata");
                throw;
            }
        }

        // =====================================================================
        // PUT ENDPOINTS
        // =====================================================================

        /// <summary>
        /// Satis durumunu guncelle
        /// </summary>
        /// <remarks>
        /// Depo urunleri hazirladiginda durumu "Tamamlandi" olarak gunceller.
        /// </remarks>
        /// <param name="id">Satis ID</param>
        /// <param name="status">Yeni durum</param>
        [HttpPut("{id:int}/status")]
        [Authorize(Policy = "StokYonetimi")]
        [ProducesResponseType(typeof(ApiResponse<Sale>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] SaleStatus status)
        {
            var sale = await _unitOfWork.Sales.GetByIdAsync(id);
            if (sale == null)
                return NotFoundResponse($"ID: {id} olan satis bulunamadi");

            sale.Status = status;
            await _unitOfWork.Sales.UpdateAsync(sale);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Satis durumu guncellendi: {SaleNumber} -> {Status}",
                sale.SaleNumber, status);

            return Success(sale, $"Satis durumu guncellendi: {status}");
        }

        // =====================================================================
        // HELPER ENDPOINTS
        // =====================================================================

        /// <summary>
        /// Urun fiyatini doviz kuru ile goster
        /// </summary>
        /// <remarks>
        /// SENARYO (Gul - Kasa, Fahri - Mobil):
        /// "Musterilerime guncel doviz kuru uzerinden urunun
        /// dolar ve euro fiyatini soyleyebilmeliyim"
        /// </remarks>
        /// <param name="productId">Urun ID</param>
        [HttpGet("product-price/{productId:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> GetProductPriceWithExchange(int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return NotFoundResponse($"ID: {productId} olan urun bulunamadi");

            var usdRate = await _exchangeRateService.GetUsdRateAsync();
            var eurRate = await _exchangeRateService.GetEurRateAsync();

            return Success(new
            {
                ProductId = product.Id,
                ProductName = product.Name,
                PriceTRY = product.UnitPrice,
                PriceUSD = Math.Round(product.UnitPrice / usdRate, 2),
                PriceEUR = Math.Round(product.UnitPrice / eurRate, 2),
                ExchangeRates = new
                {
                    USD = usdRate,
                    EUR = eurRate,
                    UpdatedAt = DateTime.Now
                }
            });
        }
    }

    /// <summary>
    /// Satis olusturma request modeli
    /// </summary>
    public class CreateSaleRequest
    {
        public int? CustomerId { get; set; }
        public int EmployeeId { get; set; }
        public int StoreId { get; set; }
        public PaymentType PaymentType { get; set; }
        public List<SaleItemRequest> Items { get; set; } = new();
    }

    /// <summary>
    /// Satis kalemi request modeli
    /// </summary>
    public class SaleItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
