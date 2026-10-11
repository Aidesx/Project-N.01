using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_385.Models;
using Project_385.Repository;

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
            if (string.IsNullOrEmpty(Slug))
            {
                return RedirectToAction("Index", "Home");
            }

            var brands = await _dataContext.Brands.FirstOrDefaultAsync(c => c.Slug == Slug);

            if (brands == null)
            {
                return NotFound($"Lỗi: Không tìm thấy hãng là: {Slug}");
            }

            var productsByBrand = _dataContext.Products.Where(p => p.BrandID == brands.Id).Include(p => p.Category).Include(p => p.Brand);

            ViewBag.BrandName = brands.Name;

            return View(await productsByBrand.OrderByDescending(p => p.Id).ToListAsync());
        }
    }
}
