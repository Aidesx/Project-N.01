using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_385.Models;
using Project_385.Models.ViewModels;
using Project_385.Services;

namespace Project_385.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUserModels> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly PermissionService _permissionService;

        public UserController(UserManager<AppUserModels> userManager, RoleManager<IdentityRole> roleManager, PermissionService permissionService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index()
        {
            if (!await _permissionService.HasPermissionAsync(User, "ManageUser"))
                return Forbid();

            var users = _userManager.Users.ToList();
            var userWithRoles = new List<UserWithRoleViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userWithRoles.Add(new UserWithRoleViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "User"
                });
            }
            return View(userWithRoles);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserModels user, string roleName)
        {
            if (ModelState.IsValid)
            {
                AppUserModels appUser = new AppUserModels
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    EmailConfirmed = true
                };

                IdentityResult result = await _userManager.CreateAsync(appUser, user.Password);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(roleName))
                        await _userManager.AddToRoleAsync(appUser, roleName);
                    else
                        await _userManager.AddToRoleAsync(appUser, "User");

                    TempData["success"] = "Tạo user thành công";
                    return RedirectToAction("Index");
                }

                foreach (IdentityError error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }

            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            ViewBag.CurrentRole = roles.FirstOrDefault() ?? "User";

            var viewModel = new UserWithRoleViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = roles.FirstOrDefault() ?? "User"
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserWithRoleViewModel model, string roleName)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null) return NotFound();

                user.UserName = model.UserName;
                user.Email = model.Email;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    if (!string.IsNullOrEmpty(roleName))
                        await _userManager.AddToRoleAsync(user, roleName);

                    TempData["success"] = "Cập nhật user thành công";
                    return RedirectToAction("Index");
                }

                foreach (IdentityError error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }

            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                    TempData["success"] = "Xóa user thành công";
                else
                    TempData["error"] = "Có lỗi khi xóa user";
            }
            else
            {
                TempData["error"] = "User không tồn tại";
            }

            return RedirectToAction("Index");
        }
    }
}