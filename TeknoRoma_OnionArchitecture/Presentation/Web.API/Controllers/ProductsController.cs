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
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IUnitOfWork unitOfWork, ILogger<ProductsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Tüm ürünleri getir
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            return Ok(products);
        }

        /// <summary>
        /// ID'ye göre ürün getir
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return NotFound(new { message = "Ürün bulunamadı" });

            return Ok(product);
        }

        /// <summary>
        /// Barkod ile ürün ara (Fahri Cepçi için)
        /// </summary>
        [HttpGet("barcode/{barcode}")]
        public async Task<IActionResult> GetByBarcode(string barcode)
        {
            var product = await _unitOfWork.Products.GetByBarcodeAsync(barcode);
            if (product == null)
                return NotFound(new { message = "Ürün bulunamadı" });

            return Ok(product);
        }

        /// <summary>
        /// Kategoriye göre ürünler
        /// </summary>
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var products = await _unitOfWork.Products.GetByCategoryAsync(categoryId);
            return Ok(products);
        }

        /// <summary>
        /// Kritik stok seviyesindeki ürünler (Kerim Zulacı için)
        /// </summary>
        [HttpGet("critical-stock")]
        [Authorize(Roles = "Depo,SubeYoneticisi")]
        public async Task<IActionResult> GetCriticalStock()
        {
            var products = await _unitOfWork.Products.GetCriticalStockProductsAsync();
            return Ok(products);
        }

        /// <summary>
        /// Stokta olmayan ürünler
        /// </summary>
        [HttpGet("out-of-stock")]
        [Authorize(Roles = "Depo,SubeYoneticisi")]
        public async Task<IActionResult> GetOutOfStock()
        {
            var products = await _unitOfWork.Products.GetOutOfStockProductsAsync();
            return Ok(products);
        }

        /// <summary>
        /// En çok satılan ürünler (Haluk Bey için)
        /// </summary>
        [HttpGet("top-selling")]
        [Authorize(Roles = "SubeYoneticisi")]
        public async Task<IActionResult> GetTopSelling([FromQuery] int count = 10)
        {
            var products = await _unitOfWork.Products.GetTopSellingProductsAsync(count);
            return Ok(products);
        }

        /// <summary>
        /// Yeni ürün ekle
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SubeYoneticisi")]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            try
            {
                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = product.ID }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün eklenirken hata");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Ürün güncelle
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "SubeYoneticisi")]
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            if (id != product.ID)
                return BadRequest(new { message = "ID uyuşmuyor" });

            try
            {
                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün güncellenirken hata");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Ürün sil (Soft Delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SubeYoneticisi")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                    return NotFound(new { message = "Ürün bulunamadı" });

                await _unitOfWork.Products.SoftDeleteAsync(product);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Ürün silindi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün silinirken hata");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
