using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project_385.Models;
using Project_385.Repositery;
using System.Security.Claims;

namespace Project_385.Services
{
    public class PermissionService
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<AppUserModels> _userManager;

        public PermissionService(DataContext dataContext, UserManager<AppUserModels> userManager)
        {
            _dataContext = dataContext;
            _userManager = userManager;
        }

        public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permission)
        {
            if (user.IsInRole("Admin"))
                return true;

            var appUser = await _userManager.GetUserAsync(user);
            if (appUser == null) return false;
            var roles = await _userManager.GetRolesAsync(appUser);

            foreach (var role in roles)
            {
                var rolePermission = await _dataContext.RolePermissions
                    .FirstOrDefaultAsync(r => r.RoleName == role && r.Status == 1);

                if (rolePermission == null) continue;

                return permission switch
                {
                    "ManageProduct" => rolePermission.CanManageProduct == 1,
                    "ManageOrder" => rolePermission.CanManageOrder == 1,
                    "ManageUser" => rolePermission.CanManageUser == 1,
                    "ManageBrand" => rolePermission.CanManageBrand == 1,
                    "ManageCategory" => rolePermission.CanManageCategory == 1,
                    _ => false
                };
            }
            return false;
        }
    }
}

