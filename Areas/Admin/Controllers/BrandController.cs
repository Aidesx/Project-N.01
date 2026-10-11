
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
    [AdminAuthorize("ManageBrand")]
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly PermissionService _permissionService;

        public BrandController(DataContext context, PermissionService permissionService)
        {
            _dataContext = context;
            _permissionService = permissionService;
        }

        [Route("Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {

            const int pageSize = 10;
            if (pg < 1) pg = 1;

            int recsCount = await _dataContext.Brands.CountAsync();
            var pager = new Paginate(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;

            var data = await _dataContext.Brands
                .AsNoTracking()
                .OrderByDescending(p => p.Id)
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
        public async Task<IActionResult> Create(BrandModels brand)
        {
            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.Replace(" ", "-").ToLower();

                var slugExists = await _dataContext.Brands
                    .FirstOrDefaultAsync(p => p.Slug == brand.Slug);
                if (slugExists != null)
                {
                    TempData["Error"] = "Thương hiệu đã có trong hệ thống";
                    return View(brand);
                }

                _dataContext.Add(brand);
                await _dataContext.SaveChangesAsync();
                TempData["Success"] = "Thêm thương hiệu thành công";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Có lỗi xảy ra, vui lòng kiểm tra lại";
            return View(brand);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? Id)
        {
            if (Id == null) return NotFound();

            BrandModels brand = await _dataContext.Brands.FindAsync(Id);
            if (brand == null) return NotFound();

            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BrandModels brand)
        {
            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.Replace(" ", "-").ToLower();

                var slugExists = await _dataContext.Brands
                    .FirstOrDefaultAsync(p => p.Slug == brand.Slug && p.Id != brand.Id);
                if (slugExists != null)
                {
                    TempData["Error"] = "Tên thương hiệu này đã tồn tại";
                    return View(brand);
                }

                _dataContext.Update(brand);
                await _dataContext.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thương hiệu thành công";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Có lỗi xảy ra";
            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return NotFound();

            var hasProducts = await _dataContext.Products.AnyAsync(p => p.BrandID == Id);
            if (hasProducts)
            {
                TempData["Error"] = "Không thể xóa thương hiệu này vì vẫn còn sản phẩm liên kết. Vui lòng chuyển hoặc xóa sản phẩm trước.";
                return RedirectToAction("Index");
            }

            BrandModels brand = await _dataContext.Brands.FindAsync(Id);
            if (brand != null)
            {
                _dataContext.Brands.Remove(brand);
                await _dataContext.SaveChangesAsync();
                TempData["Success"] = "Xóa thương hiệu thành công";
            }
            else
            {
                TempData["Error"] = "Thương hiệu không tồn tại";
            }

            return RedirectToAction("Index");
        }
    }
}
