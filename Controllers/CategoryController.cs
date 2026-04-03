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

        public async Task<IActionResult> Index(string Slug = "" )
        {   
            CategoryModel category = _dataContext.Categories.Where(c=>c.Slug == Slug).FirstOrDefault();
            if(category == null)
            {
                return RedirectToAction("Index");
            }
            var productsByCategory = _dataContext.Products.Where(b => b.CategoryID == category.Id);
            return View(await productsByCategory.OrderByDescending(p => p.Id ).ToListAsync()); 
        }
    }
}
