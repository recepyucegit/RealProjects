using Microsoft.AspNetCore.Mvc;

namespace WebMVC.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return Content("TEST CONTROLLER WORKS!");
        }
    }
}
