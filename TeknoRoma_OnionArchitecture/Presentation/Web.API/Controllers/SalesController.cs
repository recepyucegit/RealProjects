using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SalesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SalesController> _logger;

        public SalesController(IUnitOfWork unitOfWork, ILogger<SalesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Tüm satışları getir
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "SubeYoneticisi")]
        public async Task<IActionResult> GetAll()
        {
            var sales = await _unitOfWork.Sales.GetAllAsync();
            return Ok(sales);
        }

        /// <summary>
        /// Satış detayıyla birlikte getir
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var sale = await _unitOfWork.Sales.GetWithDetailsAsync(id);
            if (sale == null)
                return NotFound(new { message = "Satış bulunamadı" });

            return Ok(sale);
        }

        /// <summary>
        /// Satış numarasına göre getir
        /// </summary>
        [HttpGet("number/{saleNumber}")]
        public async Task<IActionResult> GetBySaleNumber(string saleNumber)
        {
            var sale = await _unitOfWork.Sales.GetBySaleNumberAsync(saleNumber);
            if (sale == null)
                return NotFound(new { message = "Satış bulunamadı" });

            return Ok(sale);
        }

        /// <summary>
        /// Çalışan bazlı satışlar (Gül Satar kendi satışlarını görür)
        /// </summary>
        [HttpGet("employee/{employeeId}")]
        [Authorize(Roles = "KasaSatis,MobilSatis,SubeYoneticisi")]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var sales = await _unitOfWork.Sales.GetByEmployeeAsync(employeeId);
            return Ok(sales);
        }

        /// <summary>
        /// Mağaza bazlı satışlar
        /// </summary>
        [HttpGet("store/{storeId}")]
        [Authorize(Roles = "SubeYoneticisi")]
        public async Task<IActionResult> GetByStore(int storeId)
        {
            var sales = await _unitOfWork.Sales.GetByStoreAsync(storeId);
            return Ok(sales);
        }

        /// <summary>
        /// Bugünkü satışlar
        /// </summary>
        [HttpGet("today")]
        public async Task<IActionResult> GetTodays()
        {
            var sales = await _unitOfWork.Sales.GetTodaysSalesAsync();
            return Ok(sales);
        }

        /// <summary>
        /// Bekleyen siparişler (Kerim Zulacı için)
        /// </summary>
        [HttpGet("pending")]
        [Authorize(Roles = "Depo,SubeYoneticisi")]
        public async Task<IActionResult> GetPending()
        {
            var sales = await _unitOfWork.Sales.GetByStatusAsync(SaleStatus.Hazirlaniyor);
            return Ok(sales);
        }

        /// <summary>
        /// Tarih aralığı
        /// </summary>
        [HttpGet("date-range")]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var sales = await _unitOfWork.Sales.GetByDateRangeAsync(startDate, endDate);
            return Ok(sales);
        }

        /// <summary>
        /// Yeni satış
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "KasaSatis,MobilSatis")]
        public async Task<IActionResult> Create([FromBody] SaleCreateDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Sale oluştur
                var sale = new Sale
                {
                    SaleNumber = await _unitOfWork.Sales.GenerateSaleNumberAsync(),
                    SaleDate = DateTime.Now,
                    CustomerId = dto.CustomerId,
                    EmployeeId = dto.EmployeeId,
                    StoreId = dto.StoreId,
                    Status = SaleStatus.Tamamlandi,
                    PaymentType = dto.PaymentType,
                    Subtotal = dto.Items.Sum(i => i.UnitPrice * i.Quantity),
                    TaxAmount = dto.Items.Sum(i => i.UnitPrice * i.Quantity) * 0.20m,
                    DiscountAmount = 0,
                    TotalAmount = dto.Items.Sum(i => i.UnitPrice * i.Quantity) * 1.20m,
                    CashRegisterNumber = dto.CashRegisterNumber
                };

                await _unitOfWork.Sales.AddAsync(sale);
                await _unitOfWork.SaveChangesAsync();

                // SaleDetails ve stok azalt
                foreach (var item in dto.Items)
                {
                    var detail = new SaleDetail
                    {
                        SaleId = sale.ID,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity,
                        Subtotal = item.UnitPrice * item.Quantity,
                        TotalAmount = item.UnitPrice * item.Quantity
                    };

                    await _unitOfWork.SaleDetails.AddAsync(detail);
                    await _unitOfWork.Products.DecreaseStockAsync(item.ProductId, item.Quantity);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return CreatedAtAction(nameof(GetById), new { id = sale.ID }, sale);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Satış oluşturulurken hata");
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class SaleCreateDto
    {
        public int CustomerId { get; set; }
        public int EmployeeId { get; set; }
        public int StoreId { get; set; }
        public PaymentType PaymentType { get; set; }
        public string? CashRegisterNumber { get; set; }
        public List<SaleItemDto> Items { get; set; } = new();
    }

    public class SaleItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
