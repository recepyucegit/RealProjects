// ===================================================================================
// TEKNOROMA - PRODUCTS CONTROLLER
// ===================================================================================
//
// Urun ve stok yonetimi icin API endpoint'leri.
//
// TEKNOROMA GEREKSINIMLERI:
// - Urun listeleme ve arama
// - Barkod ile urun sorgulama (Mobil Satis icin kritik)
// - Stok durumu kontrolu
// - Kritik stok uyarilari
// - Urun CRUD islemleri (Sube Muduru yetkisi)
//
// KULLANICI SENARYOLARI:
// - Fahri (Mobil Satis): Barkod okutup stok ve fiyat bilgisi alir
// - Gul (Kasa Satis): Urun fiyati ve stok kontrolu
// - Kerim (Depo): Kritik stok listesi
// - Haluk (Sube Muduru): Urun tanimlama ve fiyat guncelleme
//
// ===================================================================================

using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Urun Yonetimi API Controller
    ///
    /// Stok takibi, fiyat sorgulama, urun CRUD
    /// </summary>
    [Authorize]
    public class ProductsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IUnitOfWork unitOfWork, ILogger<ProductsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // =====================================================================
        // GET ENDPOINTS
        // =====================================================================

        /// <summary>
        /// Tum urunleri listele
        /// </summary>
        /// <remarks>
        /// Tum aktif urunleri getirir (soft delete olmayanlar)
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Product>>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            return Success(products);
        }

        /// <summary>
        /// ID ile urun getir
        /// </summary>
        /// <param name="id">Urun ID</param>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<Product>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);

            if (product == null)
                return NotFoundResponse($"ID: {id} olan urun bulunamadi");

            return Success(product);
        }

        /// <summary>
        /// Barkod ile urun getir
        /// </summary>
        /// <remarks>
        /// Mobil satis icin kritik endpoint.
        /// Cep bilgisayari ile barkod okutuldiginda bu endpoint cagirilir.
        ///
        /// SENARYO (Fahri - Mobil Satis):
        /// 1. Musteri urun sorar
        /// 2. Fahri barkodu okuttur
        /// 3. Bu endpoint'ten stok ve fiyat bilgisi gelir
        /// 4. Musteriye bilgi verilir
        /// </remarks>
        /// <param name="barcode">Urun barkod numarasi</param>
        [HttpGet("barcode/{barcode}")]
        [ProducesResponseType(typeof(ApiResponse<Product>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByBarcode(string barcode)
        {
            var product = await _unitOfWork.Products.GetByBarcodeAsync(barcode);

            if (product == null)
                return NotFoundResponse($"Barkod: {barcode} olan urun bulunamadi");

            _logger.LogInformation("Barkod sorgusu: {Barcode} - {ProductName}",
                barcode, product.Name);

            return Success(product);
        }

        /// <summary>
        /// Kategoriye gore urunler
        /// </summary>
        /// <param name="categoryId">Kategori ID</param>
        [HttpGet("category/{categoryId:int}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Product>>), 200)]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var products = await _unitOfWork.Products.GetByCategoryAsync(categoryId);
            return Success(products);
        }

        /// <summary>
        /// Tedarikci urunleri
        /// </summary>
        /// <param name="supplierId">Tedarikci ID</param>
        [HttpGet("supplier/{supplierId:int}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Product>>), 200)]
        public async Task<IActionResult> GetBySupplier(int supplierId)
        {
            var products = await _unitOfWork.Products.GetBySupplierAsync(supplierId);
            return Success(products);
        }

        /// <summary>
        /// Stok durumuna gore urunler
        /// </summary>
        /// <remarks>
        /// StockStatus degerleri:
        /// - InStock (0): Stokta var
        /// - LowStock (1): Dusuk stok (kritik seviyeye yakin)
        /// - OutOfStock (2): Stokta yok
        /// </remarks>
        /// <param name="status">Stok durumu</param>
        [HttpGet("stock-status/{status}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Product>>), 200)]
        public async Task<IActionResult> GetByStockStatus(StockStatus status)
        {
            var products = await _unitOfWork.Products.GetByStockStatusAsync(status);
            return Success(products);
        }

        /// <summary>
        /// Kritik stok seviyesindeki urunler
        /// </summary>
        /// <remarks>
        /// KRITIK STOK UYARISI:
        /// Bu endpoint kritik seviyenin altina dusen urunleri getirir.
        ///
        /// SENARYO (Kerim - Depo):
        /// "Kritik seviyenin altina dusen urunleri ben de gorebilmeliyim,
        /// Haluk Bey subede olmadiginda ve acil durumlarda urun siparisini biz veriyoruz."
        ///
        /// SENARYO (Gul - Kasa):
        /// "Urunler kritik seviyenin altina dustugu anda Haluk Bey'in
        /// uygulamasi uyari vermeli"
        /// </remarks>
        [HttpGet("low-stock")]
        [Authorize(Policy = "StokYonetimi")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Product>>), 200)]
        public async Task<IActionResult> GetLowStockProducts()
        {
            var products = await _unitOfWork.Products.GetLowStockProductsAsync();

            _logger.LogWarning("Kritik stok sorgusu: {Count} urun kritik seviyede",
                products.Count);

            return Success(products, $"{products.Count} urun kritik stok seviyesinde");
        }

        // =====================================================================
        // POST ENDPOINTS
        // =====================================================================

        /// <summary>
        /// Yeni urun olustur
        /// </summary>
        /// <remarks>
        /// Sadece Sube Muduru yetkisiyle urun tanimlanabilir.
        ///
        /// SENARYO (Haluk - Sube Muduru):
        /// "Urun tanimlamalarini sadece ben yapabilmeliyim"
        /// </remarks>
        [HttpPost]
        [Authorize(Policy = "SubeYonetimi")]
        [ProducesResponseType(typeof(ApiResponse<Product>), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            // Barkod benzersizlik kontrolu
            var existingProduct = await _unitOfWork.Products.GetByBarcodeAsync(product.Barcode);
            if (existingProduct != null)
                return BadRequestResponse($"Bu barkod zaten kullaniliyor: {product.Barcode}");

            var created = await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Yeni urun olusturuldu: {ProductName} ({Barcode})",
                product.Name, product.Barcode);

            return Created(created);
        }

        // =====================================================================
        // PUT ENDPOINTS
        // =====================================================================

        /// <summary>
        /// Urun guncelle
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "SubeYonetimi")]
        [ProducesResponseType(typeof(ApiResponse<Product>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            var existing = await _unitOfWork.Products.GetByIdAsync(id);
            if (existing == null)
                return NotFoundResponse($"ID: {id} olan urun bulunamadi");

            // Entity guncelle
            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.UnitPrice = product.UnitPrice;
            existing.CategoryId = product.CategoryId;
            existing.SupplierId = product.SupplierId;
            existing.CriticalStockLevel = product.CriticalStockLevel;

            await _unitOfWork.Products.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Urun guncellendi: {ProductId} - {ProductName}",
                id, existing.Name);

            return Success(existing, "Urun basariyla guncellendi");
        }

        /// <summary>
        /// Stok guncelle
        /// </summary>
        /// <remarks>
        /// Stok miktarini arttirir veya azaltir.
        /// Negatif deger stok dusumu, pozitif deger stok artisi.
        /// </remarks>
        /// <param name="id">Urun ID</param>
        /// <param name="quantityChange">Miktar degisimi (+/-)</param>
        [HttpPut("{id:int}/stock")]
        [Authorize(Policy = "StokYonetimi")]
        [ProducesResponseType(typeof(ApiResponse<Product>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] int quantityChange)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return NotFoundResponse($"ID: {id} olan urun bulunamadi");

            // Stok negatife dusemez kontrolu
            if (product.UnitsInStock + quantityChange < 0)
                return BadRequestResponse("Yetersiz stok. Mevcut: " + product.UnitsInStock);

            await _unitOfWork.Products.UpdateStockAsync(id, quantityChange);
            await _unitOfWork.SaveChangesAsync();

            // Guncel urunu getir
            var updated = await _unitOfWork.Products.GetByIdAsync(id);

            _logger.LogInformation("Stok guncellendi: {ProductId} - Degisim: {Change}, Yeni stok: {Stock}",
                id, quantityChange, updated?.UnitsInStock);

            return Success(updated, $"Stok guncellendi. Yeni miktar: {updated?.UnitsInStock}");
        }

        // =====================================================================
        // DELETE ENDPOINTS
        // =====================================================================

        /// <summary>
        /// Urun sil (Soft Delete)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "SubeYonetimi")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return NotFoundResponse($"ID: {id} olan urun bulunamadi");

            await _unitOfWork.Products.SoftDeleteAsync(product);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Urun silindi (soft delete): {ProductId} - {ProductName}",
                id, product.Name);

            return NoContent();
        }
    }
}
