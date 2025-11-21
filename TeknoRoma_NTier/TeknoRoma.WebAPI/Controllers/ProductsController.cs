using Microsoft.AspNetCore.Mvc;
using TeknoRoma.Business.Abstract;
using TeknoRoma.Business.DTOs;

namespace TeknoRoma.WebAPI.Controllers;

/// <summary>
/// Products Controller
/// Ürün işlemleri için RESTful API endpoint'leri sağlar
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    /// <summary>
    /// Constructor - Dependency Injection ile service alır
    /// </summary>
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Tüm aktif ürünleri getirir
    /// GET: api/Products
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products); // 200 OK + data
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Sunucu hatası!", error = ex.Message });
        }
    }

    /// <summary>
    /// ID'ye göre ürün getirir
    /// GET: api/Products/5
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
                return NotFound(new { message = "Ürün bulunamadı!" }); // 404 Not Found

            return Ok(product); // 200 OK + data
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Sunucu hatası!", error = ex.Message });
        }
    }

    /// <summary>
    /// Kategoriye göre ürünleri getirir
    /// GET: api/Products/category/5
    /// </summary>
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int categoryId)
    {
        try
        {
            var products = await _productService.GetProductsByCategoryAsync(categoryId);
            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Sunucu hatası!", error = ex.Message });
        }
    }

    /// <summary>
    /// Öne çıkan ürünleri getirir
    /// GET: api/Products/featured
    /// </summary>
    [HttpGet("featured")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetFeaturedProducts()
    {
        try
        {
            var products = await _productService.GetFeaturedProductsAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Sunucu hatası!", error = ex.Message });
        }
    }

    /// <summary>
    /// Ürün arama
    /// GET: api/Products/search?term=iphone
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts([FromQuery] string term)
    {
        try
        {
            var products = await _productService.SearchProductsAsync(term);
            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Sunucu hatası!", error = ex.Message });
        }
    }

    /// <summary>
    /// Yeni ürün ekler
    /// POST: api/Products
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto createProductDto)
    {
        try
        {
            // Model validation kontrolü
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // 400 Bad Request

            var product = await _productService.CreateProductAsync(createProductDto);

            // 201 Created + Location header + data
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Sunucu hatası!", error = ex.Message });
        }
    }

    /// <summary>
    /// Ürün günceller
    /// PUT: api/Products/5
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
    {
        try
        {
            if (id != updateProductDto.Id)
                return BadRequest(new { message = "ID uyuşmazlığı!" }); // 400 Bad Request

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _productService.UpdateProductAsync(updateProductDto);
            return Ok(product); // 200 OK + data
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Sunucu hatası!", error = ex.Message });
        }
    }

    /// <summary>
    /// Ürünü siler (Soft Delete)
    /// DELETE: api/Products/5
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        try
        {
            var result = await _productService.DeleteProductAsync(id);

            if (!result)
                return NotFound(new { message = "Ürün bulunamadı!" });

            return NoContent(); // 204 No Content (başarılı silme)
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Sunucu hatası!", error = ex.Message });
        }
    }

    /// <summary>
    /// Ürün stoğunu günceller
    /// PATCH: api/Products/5/stock?quantity=100
    /// </summary>
    [HttpPatch("{id}/stock")]
    public async Task<ActionResult> UpdateStock(int id, [FromQuery] int quantity)
    {
        try
        {
            var result = await _productService.UpdateStockAsync(id, quantity);

            if (!result)
                return NotFound(new { message = "Ürün bulunamadı!" });

            return Ok(new { message = "Stok başarıyla güncellendi!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Sunucu hatası!", error = ex.Message });
        }
    }
}
