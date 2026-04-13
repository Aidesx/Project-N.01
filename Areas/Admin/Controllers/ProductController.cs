using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project_385.Models;
using Project_385.Repositery;

namespace Project_385.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment; // SỬA: Thêm dấu chấm phẩy ở đây

        public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Products.OrderByDescending(p => p.Id).Include(p => p.Category).Include(p => p.Brand).ToListAsync());
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModels product)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(product.Slug))
                {
                    product.Slug = product.Name.Replace(" ", "-");
                    
                    var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
                    if (slug != null)
                    {
                        ModelState.AddModelError("", "Sản phẩm đã có");
                        return View(product);
                    }
                    if (product.ImageUpLoad != null)
                    {
                        string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        if (!Directory.Exists(uploadDir))
                        {
                            Directory.CreateDirectory(uploadDir);
                        }
                        string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpLoad.FileName;
                        string filePath = Path.Combine(uploadDir, imageName);

                        FileStream fs = new FileStream(filePath, FileMode.Create);
                        await product.ImageUpLoad.CopyToAsync(fs);
                        fs.Close();

                        product.Image = imageName;
                    }
                }

                _dataContext.Add(product);
                await _dataContext.SaveChangesAsync();
                TempData["succes"] = "Thêm sản phầm thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {

                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }

            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryID);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandID);
            return View(product);
        }
    }
}