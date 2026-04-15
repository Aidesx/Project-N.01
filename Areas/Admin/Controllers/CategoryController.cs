using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project_385.Models;
using Project_385.Repositery;

namespace Project_385.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;
        public CategoryController(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Categories.OrderByDescending(p => p.Id).ToListAsync());
        }
        [HttpGet]

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(category.Slug))
                {
                    category.Slug = category.Name.Replace(" ", "-");

                    var slug = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Slug == category.Slug);
                    if (slug != null)
                    {
                        ModelState.AddModelError("", "Danh mục đã có");
                        return View(category);
                    }
                }

                _dataContext.Add(category);
                await _dataContext.SaveChangesAsync();
                TempData["succes"] = "Thêm danh mục thành công";
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
        CategoryModel category = await _dataContext.Categories.FindAsync(Id);
        if (category == null) return NotFound();
        return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(category.Slug))
                {
                    category.Slug = category.Name.Replace(" ", "-");

                    var slug = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Slug == category.Slug && p.Id != category.Id);
                    if (slug != null)
                    {
                        ModelState.AddModelError("", "Danh mục đã có");
                        return View(category);
                    }
                }

                _dataContext.Update(category);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật danh mục thành công";
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

            CategoryModel category = await _dataContext.Categories.FindAsync(Id);

            if (category != null)
            {
                _dataContext.Categories.Remove(category);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Xóa danh mục thành công";
            }
            else
            {
                TempData["error"] = "Danh mục không tồn tại";
            }
            return RedirectToAction("Index");

        }
    }
}
