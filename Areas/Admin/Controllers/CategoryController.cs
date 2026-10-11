using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_385.Areas.Admin.Filters;
using Project_385.Models;
using Project_385.Repository;
using Project_385.Services;

namespace Project_385.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize("ManageCategory")]
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly PermissionService _permissionService;

        public CategoryController(DataContext context, PermissionService permissionService)
        {
            _dataContext = context;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index(int pg = 1)
        {

            const int pageSize = 10; 
            if (pg < 1) pg = 1;

            int recsCount = await _dataContext.Categories.CountAsync(); 
            var pager = new Paginate(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize; 

            var data = await _dataContext.Categories
                .AsNoTracking()
                .OrderByDescending(c => c.Id)
                .Skip(recSkip)
                .Take(pager.PageSize)
                .ToListAsync();

            ViewBag.Pager = pager;
            return View(data);
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
                    category.Slug = category.Name.Replace(" ", "-").ToLower();
                }

                var slugExists = await _dataContext.Categories
                    .FirstOrDefaultAsync(p => p.Slug == category.Slug);
                if (slugExists != null)
                {
                    TempData["Error"] = "Danh mục đã có";
                    return View(category);
                }

                _dataContext.Add(category);
                await _dataContext.SaveChangesAsync();
                TempData["Success"] = "Thêm danh mục thành công";
                return RedirectToAction("Index");
            }

            List<string> errors = new List<string>();
            foreach (var value in ModelState.Values)
                foreach (var error in value.Errors)
                    errors.Add(error.ErrorMessage);

            TempData["Error"] = string.Join("\n", errors);
            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? Id)
        {
            if (Id == null) return NotFound();

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
                    category.Slug = category.Name.Replace(" ", "-").ToLower();
                }

                var slugExists = await _dataContext.Categories
                    .FirstOrDefaultAsync(p => p.Slug == category.Slug && p.Id != category.Id);
                if (slugExists != null)
                {
                    TempData["Error"] = "Danh mục đã có";
                    return View(category);
                }

                _dataContext.Update(category);
                await _dataContext.SaveChangesAsync();
                TempData["Success"] = "Cập nhật danh mục thành công";
                return RedirectToAction("Index");
            }

            List<string> errors = new List<string>();
            foreach (var value in ModelState.Values)
                foreach (var error in value.Errors)
                    errors.Add(error.ErrorMessage);

            TempData["Error"] = string.Join("\n", errors);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return NotFound();

            CategoryModel category = await _dataContext.Categories.FindAsync(Id);
            if (category != null)
            {
                _dataContext.Categories.Remove(category);
                await _dataContext.SaveChangesAsync();
                TempData["Success"] = "Xóa danh mục thành công";
            }
            else
            {
                TempData["Error"] = "Danh mục không tồn tại";
            }

            return RedirectToAction("Index");
        }
    }
}
