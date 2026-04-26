using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_385.Models;
using Project_385.Models.ViewModels;
using Project_385.Repositery;
using Project_385.Services;

namespace Project_385.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;
        private readonly PermissionService _permissionService;

        public RoleController(RoleManager<IdentityRole> roleManager, DataContext dataContext, PermissionService permissionService)
        {
            _roleManager = roleManager;
            _dataContext = dataContext;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index(int pg =1)
        {
            if (!await _permissionService.HasPermissionAsync(User, "ManageUser"))
                return Forbid();

            var roles = await _roleManager.Roles.ToListAsync();
            var permissions = await _dataContext.RolePermissions.ToListAsync();

            var viewModel = roles.Select(role => new RolePermissionViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name,
                Permission = permissions.FirstOrDefault(p => p.RoleName == role.Name)
            }).ToList();

            const int pageSize = 10;
            if (pg < 1) pg = 1;

            int recsCount = viewModel.Count();
            var pager = new Paginate(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = viewModel.Skip(recSkip).Take(pager.PageSize).ToList();

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
        public async Task<IActionResult> Create(RoleModels model)
        {
            if (ModelState.IsValid)
            {
                var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
                if (roleExists)
                {
                    ModelState.AddModelError("", "Role này đã tồn tại");
                    return View(model);
                }

                var identityRole = new IdentityRole(model.RoleName);
                await _roleManager.CreateAsync(identityRole);

                model.Id = identityRole.Id;
                _dataContext.RolePermissions.Add(model);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Tạo role thành công";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            var permission = await _dataContext.RolePermissions
                .FirstOrDefaultAsync(p => p.RoleName == role.Name);

            if (permission == null)
            {
                permission = new RoleModels
                {
                    Id = role.Id,
                    RoleName = role.Name,
                    Status = 1
                };
            }

            return View(permission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleModels model)
        {
            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(model.Id);
                if (role == null) return NotFound();

                if (role.Name != model.RoleName)
                {
                    role.Name = model.RoleName;
                    await _roleManager.UpdateAsync(role);
                }

                var existing = await _dataContext.RolePermissions
                    .FirstOrDefaultAsync(p => p.Id == model.Id);

                if (existing != null)
                {
                    existing.RoleName = model.RoleName;
                    existing.CanManageProduct = model.CanManageProduct;
                    existing.CanManageOrder = model.CanManageOrder;
                    existing.CanManageUser = model.CanManageUser;
                    existing.CanManageBrand = model.CanManageBrand;
                    existing.CanManageCategory = model.CanManageCategory;
                    existing.Status = model.Status;
                    _dataContext.Update(existing);
                }
                else
                {
                    _dataContext.RolePermissions.Add(model);
                }

                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật role thành công";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                var permission = await _dataContext.RolePermissions
                    .FirstOrDefaultAsync(p => p.Id == id);
                if (permission != null)
                {
                    _dataContext.RolePermissions.Remove(permission);
                    await _dataContext.SaveChangesAsync();
                }

                await _roleManager.DeleteAsync(role);
                TempData["success"] = "Xóa role thành công";
            }
            else
            {
                TempData["error"] = "Role không tồn tại";
            }

            return RedirectToAction("Index");
        }
    }
}