
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_385.Models;
using Project_385.Repositery;
using Project_385.Services;

namespace Project_385.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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
        public async Task<IActionResult> Index()
        {
            if (!await _permissionService.HasPermissionAsync(User, "ManageBrand"))
                return Forbid();
            return View(await _dataContext.Brands.OrderByDescending(p => p.Id).ToListAsync());
        }

        [HttpGet]
        [Route("Create")] 
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create(BrandModels brand)
        {
            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.Replace(" ", "-").ToLower();

                var slugExists = await _dataContext.Brands
                    .FirstOrDefaultAsync(p => p.Slug == brand.Slug);
                if (slugExists != null)
                {
                    ModelState.AddModelError("", "Thương hiệu đã có trong hệ thống");
                    return View(brand);
                }

                _dataContext.Add(brand);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm thương hiệu thành công";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Có lỗi xảy ra, vui lòng kiểm tra lại";
            return View(brand);
        }

        [HttpGet]
        [Route("Edit/{Id?}")]
        public async Task<IActionResult> Edit(int? Id)
        {
            if (Id == null) return NotFound();

            BrandModels brand = await _dataContext.Brands.FindAsync(Id);
            if (brand == null) return NotFound();

            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit/{Id?}")]
        public async Task<IActionResult> Edit(BrandModels brand)
        {
            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.Replace(" ", "-").ToLower();

                var slugExists = await _dataContext.Brands
                    .FirstOrDefaultAsync(p => p.Slug == brand.Slug && p.Id != brand.Id);
                if (slugExists != null)
                {
                    ModelState.AddModelError("", "Tên thương hiệu này đã tồn tại");
                    return View(brand);
                }

                _dataContext.Update(brand);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật thương hiệu thành công";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Có lỗi xảy ra";
            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return NotFound();

            BrandModels brand = await _dataContext.Brands.FindAsync(Id);
            if (brand != null)
            {
                _dataContext.Brands.Remove(brand);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Xóa thương hiệu thành công";
            }
            else
            {
                TempData["error"] = "Thương hiệu không tồn tại";
            }

            return RedirectToAction("Index");
        }
    }
}
