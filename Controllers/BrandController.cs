using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_385.Models;
using Project_385.Repositery;

namespace Project_385.Controllers
{
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;

        public BrandController(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IActionResult> Index(string Slug = "")
        {
            BrandModels brand = _dataContext.Brands.Where(c => c.Slug == Slug).FirstOrDefault();
            if ( brand == null) return RedirectToAction("Index");

            var productsByBrand = _dataContext.Products.Where(b => b.BrandID == brand.Id);
            return View(await productsByBrand.OrderByDescending(p => p.Id).ToListAsync());
        }
    }
}
