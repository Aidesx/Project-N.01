using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_385.Repositery;

namespace Project_385.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;

        public ProductController(DataContext context)
        {
            _dataContext = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int? Id)
        {
            if (Id == null)
            {
                return RedirectToAction("Index");
            }
            var productsByID = await _dataContext.Products.Include(p => p.Category).Include(p => p.Brand).Where(b => b.Id == Id).FirstOrDefaultAsync();

            if (productsByID == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.RelatedByBrand = await _dataContext.Products.Where(p => p.BrandID == productsByID.BrandID && p.Id != productsByID.Id).Take(4).ToListAsync();

            ViewBag.RelatedByCategory = await _dataContext.Products.Where(p => p.CategoryID == productsByID.CategoryID && p.Id != productsByID.Id).Take(6).ToListAsync();

            return View(productsByID);
        }
        public async Task<IActionResult> Search(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return RedirectToAction("Index", "Home");
            }
            var products = await _dataContext.Products.Include(p => p.Category).Include(p => p.Brand).Where(p => p.Name.Contains(search) || p.Description.Contains(search)).ToListAsync(); 
            ViewBag.Keyword = search;
            return View(products);
        }
    }
}
