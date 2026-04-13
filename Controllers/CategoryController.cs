using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_385.Models;
using Project_385.Repositery;

namespace Project_385.Controllers
{
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;

        public CategoryController(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IActionResult> Index(string Slug = "")
        {
            if (string.IsNullOrEmpty(Slug))
            {
                return RedirectToAction("Index", "Home");
            }

            var category = await _dataContext.Categories
                .FirstOrDefaultAsync(c => c.Slug == Slug);

            if (category == null)
            {
                return NotFound($"Lỗi: Không tìm thấy danh mục có Slug là: {Slug}");
            }

            var productsByCategory = _dataContext.Products
                .Where(p => p.CategoryID == category.Id)
                .Include(p => p.Category)
                .Include(p => p.Brand);

            ViewBag.CategoryName = category.Name;

            return View(await productsByCategory.OrderByDescending(p => p.Id).ToListAsync());
        }
    }
}
