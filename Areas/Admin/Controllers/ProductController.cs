using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project_385.Models;
using Project_385.Repositery;
using Project_385.Services;

namespace Project_385.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly PermissionService _permissionService;

        public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment, PermissionService permissionService)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index(int pg = 1)
        {
            if (!await _permissionService.HasPermissionAsync(User, "ManageProduct"))
                return Forbid();
            var products = await _dataContext.Products.Include(p => p.Category).Include(p => p.Brand).OrderByDescending(p => p.Id).ToListAsync();

            const int pageSize = 10;
            if (pg < 1) pg = 1;

            int recsCount = products.Count;
            var pager = new Paginate(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = products.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;
            return View(data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            PopulateViewBag();
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
                    product.Slug = product.Name.Replace(" ", "-").ToLower();
                }

                var existingSlug = await _dataContext.Products
                    .FirstOrDefaultAsync(p => p.Slug == product.Slug);
                if (existingSlug != null)
                {
                    TempData["Error"] = "Sản phẩm đã có";
                    PopulateViewBag();
                    return View(product);
                }

                if (product.ImageUpLoad != null)
                {
                    string[] allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    string fileExtension = Path.GetExtension(product.ImageUpLoad.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        TempData["Error"] = "Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png, .webp)";
                        PopulateViewBag();
                        return View(product);
                    }

                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    // Dùng Path.GetFileName() để tránh path traversal
                    string imageName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(product.ImageUpLoad.FileName);
                    string filePath = Path.Combine(uploadDir, imageName);
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageUpLoad.CopyToAsync(fs);
                    }
                    product.Image = imageName;
                }

                _dataContext.Add(product);
                await _dataContext.SaveChangesAsync();
                TempData["Success"] = "Thêm sản phẩm thành công";
                return RedirectToAction("Index");
            }

            List<string> errors = new List<string>();
            foreach (var value in ModelState.Values)
                foreach (var error in value.Errors)
                    errors.Add(error.ErrorMessage);

            TempData["Error"] = string.Join("\n", errors);
            PopulateViewBag();
            return View(product);
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

                var slugExists = await _dataContext.Products
                    .FirstOrDefaultAsync(p => p.Slug == product.Slug && p.Id != product.Id);
                if (slugExists != null)
                {
                    TempData["Error"] = "Slug đã tồn tại trong một sản phẩm khác.";
                    return View(product);
                }

                if (product.ImageUpLoad != null)
                {
                    string[] allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    string fileExtension = Path.GetExtension(product.ImageUpLoad.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        TempData["Error"] = "Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png, .webp)";
                        return View(product);
                    }

                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    string imageName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(product.ImageUpLoad.FileName);
                    string filePath = Path.Combine(uploadDir, imageName);
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageUpLoad.CopyToAsync(fs);
                    }

                    if (!string.Equals(existingProduct.Image, "default.png"))
                    {
                        string oldFilePath = Path.Combine(uploadDir, existingProduct.Image);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
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
                TempData["Success"] = "Cập nhật sản phẩm thành công";
                return RedirectToAction("Index");
            }

            List<string> errors = new List<string>();
            foreach (var value in ModelState.Values)
                foreach (var error in value.Errors)
                    errors.Add(error.ErrorMessage);

            TempData["Error"] = string.Join("\n", errors);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return NotFound();

            ProductModels product = await _dataContext.Products.FindAsync(Id);
            if (product != null)
            {
                if (!string.Equals(product.Image, "default.png"))
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string oldFilePath = Path.Combine(uploadDir, product.Image);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                _dataContext.Products.Remove(product);
                await _dataContext.SaveChangesAsync();
                TempData["Success"] = "Xóa sản phẩm thành công";
            }
            else
            {
                TempData["Error"] = "Sản phẩm không tồn tại";
            }

            return RedirectToAction("Index");
        }

        private void PopulateViewBag()
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
        }
    }
}
