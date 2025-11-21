using Microsoft.AspNetCore.Mvc;
using TeknoRoma.Business.Abstract;
using TeknoRoma.Business.DTOs;

namespace TeknoRoma.WebAPI.Controllers;

/// <summary>
/// Categories Controller
/// Kategori işlemleri için RESTful API endpoint'leri sağlar
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Tüm kategorileri getirir
    /// GET: api/Categories
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Sunucu hatası!", error = ex.Message });
        }
    }

    /// <summary>
    /// Aktif kategorileri getirir
    /// GET: api/Categories/active
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetActiveCategories()
    {
        try
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Sunucu hatası!", error = ex.Message });
        }
    }

    /// <summary>
    /// ID'ye göre kategori getirir
    /// GET: api/Categories/5
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
                return NotFound(new { message = "Kategori bulunamadı!" });

            return Ok(category);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Sunucu hatası!", error = ex.Message });
        }
    }

    /// <summary>
    /// Yeni kategori ekler
    /// POST: api/Categories
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _categoryService.CreateCategoryAsync(createCategoryDto);
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Sunucu hatası!", error = ex.Message });
        }
    }

    /// <summary>
    /// Kategori günceller
    /// PUT: api/Categories/5
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
    {
        try
        {
            if (id != updateCategoryDto.Id)
                return BadRequest(new { message = "ID uyuşmazlığı!" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _categoryService.UpdateCategoryAsync(updateCategoryDto);
            return Ok(category);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Sunucu hatası!", error = ex.Message });
        }
    }

    /// <summary>
    /// Kategoriyi siler (Soft Delete)
    /// DELETE: api/Categories/5
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCategory(int id)
    {
        try
        {
            var result = await _categoryService.DeleteCategoryAsync(id);

            if (!result)
                return NotFound(new { message = "Kategori bulunamadı!" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Sunucu hatası!", error = ex.Message });
        }
    }
}
