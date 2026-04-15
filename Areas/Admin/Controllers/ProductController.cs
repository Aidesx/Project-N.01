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
        private readonly IWebHostEnvironment _webHostEnvironment;

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
                }

                if (product.ImageUpLoad != null)
                {
                    string[] allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    string fileExtension = Path.GetExtension(product.ImageUpLoad.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("", "Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png, .webp)");
                        return View(product);
                    }

                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpLoad.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageUpLoad.CopyToAsync(fs);
                    }

                    product.Image = imageName;
                }

                _dataContext.Add(product);
                await _dataContext.SaveChangesAsync();
                TempData["succes"] = "Thêm sản phẩm thành công";
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
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? Id)
        {
            if (Id == null) return NotFound();

            ProductModels product = await _dataContext.Products.FindAsync(Id);
            if (product == null) return NotFound();

            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryID);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandID);

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductModels product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryID);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandID);

            var existingProduct = await _dataContext.Products.FindAsync(product.Id);
            if (existingProduct == null) return NotFound();

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(product.Slug))
                {
                    product.Slug = product.Name.Replace(" ", "-").ToLower();
                }

                var slugExists = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug && p.Id != product.Id);
                if (slugExists != null)
                {
                    ModelState.AddModelError("", "Slug đã tồn tại trong một sản phẩm khác.");
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

                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageUpLoad.CopyToAsync(fs);
                    }

                    if (!string.Equals(existingProduct.Image, "default.png"))
                    {
                        string oldfilePath = Path.Combine(uploadDir, existingProduct.Image);
                        if (System.IO.File.Exists(oldfilePath))
                        {
                            System.IO.File.Delete(oldfilePath);
                        }
                    }

                    existingProduct.Image = imageName;
                }

                existingProduct.Name = product.Name;
                existingProduct.Slug = product.Slug;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.BrandID = product.BrandID;
                existingProduct.CategoryID = product.CategoryID;

                _dataContext.Update(existingProduct);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật sản phẩm thành công";
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
        }

        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return NotFound();

            ProductModels product = await _dataContext.Products.FindAsync(Id);

            if (product != null)
            {
                if (!string.Equals(product.Image, "default.png"))
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string oldfilePath = Path.Combine(uploadDir, product.Image);

                    if (System.IO.File.Exists(oldfilePath))
                    {
                        System.IO.File.Delete(oldfilePath);
                    }
                }

                _dataContext.Products.Remove(product);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Xóa sản phẩm thành công";
            }
            else
            {
                TempData["error"] = "Sản phẩm không tồn tại";
            }
            return RedirectToAction("Index");
        }
    }
}