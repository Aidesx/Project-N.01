using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project_385.Areas.Admin.Filters;
using Project_385.Models;
using Project_385.Models.ViewModels;
using Project_385.Services;

namespace Project_385.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize("ManageUser")]
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

        public async Task<IActionResult> Index(int pg = 1)
        {

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
                    Role = roles.FirstOrDefault()
                });
            }

            const int pageSize = 10;
            if (pg < 1) pg = 1;

            int recsCount = userWithRoles.Count();
            var pager = new Paginate(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = userWithRoles.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;
            return View(data);
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
            if (string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase) && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "Chỉ quản trị viên cấp cao (Admin) mới có quyền cấp quyền Admin.";
                ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
                return View(user);
            }

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

                    TempData["Success"] = "Tạo user thành công";
                    return RedirectToAction("Index");
                }

                TempData["Error"] = string.Join("\n", result.Errors.Select(e => e.Description));
            }
            else
            {
                TempData["Error"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            }

            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
            return View(user); 
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Không tìm thấy ID của người dùng.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Index");
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Admin") && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa tài khoản quản trị viên Admin.";
                return RedirectToAction("Index");
            }

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
            if (string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase) && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "Chỉ quản trị viên cấp cao (Admin) mới có quyền cấp vai trò Admin.";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng.";
                    return RedirectToAction("Index");
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Contains("Admin") && !User.IsInRole("Admin"))
                {
                    TempData["Error"] = "Bạn không có quyền thay đổi tài khoản quản trị viên Admin.";
                    return RedirectToAction("Index");
                }

                // Bảo vệ tài khoản admin chính không bị hạ quyền
                if (user.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase) && roleName != "Admin")
                {
                    TempData["Error"] = "Không được phép gỡ quyền Admin của tài khoản quản trị viên chính (admin).";
                    return RedirectToAction("Index");
                }

                user.UserName = model.UserName;
                user.Email = model.Email;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    if (!string.IsNullOrEmpty(roleName))
                        await _userManager.AddToRoleAsync(user, roleName);

                    TempData["Success"] = "Cập nhật user thành công";
                    return RedirectToAction("Index");
                }
                TempData["Error"] = string.Join("\n", result.Errors.Select(e => e.Description));
            }
            else
            {
                TempData["Error"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            }

            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                TempData["Error"] = "Không tìm thấy ID người dùng.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                if (user.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Error"] = "Không được phép xóa tài khoản quản trị viên chính (admin).";
                    return RedirectToAction("Index");
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                if (userRoles.Contains("Admin") && !User.IsInRole("Admin"))
                {
                    TempData["Error"] = "Bạn không có quyền xóa tài khoản quản trị viên Admin.";
                    return RedirectToAction("Index");
                }

                var currentUserId = _userManager.GetUserId(User);
                if (user.Id == currentUserId)
                {
                    TempData["Error"] = "Bạn không thể tự xóa tài khoản của chính mình.";
                    return RedirectToAction("Index");
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                    TempData["Success"] = "Xóa user thành công";
                else
                    TempData["Error"] = "Có lỗi khi xóa user";
            }
            else
            {
                TempData["Error"] = "User không tồn tại";
            }

            return RedirectToAction("Index");
        }
    }
}