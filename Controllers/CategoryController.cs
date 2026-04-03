using Microsoft.AspNetCore.Mvc;

namespace Project_385.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
