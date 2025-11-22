using Microsoft.AspNetCore.Mvc;
using TeknoRoma.Business.Abstract;

namespace TeknoRoma.WebMVC.Controllers;

/// <summary>
/// Home Controller
/// Ana sayfa ve genel sayfaları yönetir
/// </summary>
public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public HomeController(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }

    /// <summary>
    /// Ana sayfa - Öne çıkan ürünleri gösterir
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            // Öne çıkan ürünleri getir
            var featuredProducts = await _productService.GetFeaturedProductsAsync();

            // ViewBag ile kategorileri view'a gönder
            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();

            return View(featuredProducts);
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            return View();
        }
    }

    /// <summary>
    /// Hakkımızda sayfası
    /// </summary>
    public IActionResult About()
    {
        return View();
    }

    /// <summary>
    /// İletişim sayfası
    /// </summary>
    public IActionResult Contact()
    {
        return View();
    }

    /// <summary>
    /// Hata sayfası
    /// </summary>
    public IActionResult Error()
    {
        return View();
    }
}
