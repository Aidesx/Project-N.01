using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Project_385.Services;

namespace Project_385.Areas.Admin.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string _permission;

        public AdminAuthorizeAttribute(string permission = null)
        {
            _permission = permission;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;

            // 1. Kiểm tra đã đăng nhập chưa
            if (!(user.Identity?.IsAuthenticated ?? false))
            {
                context.Result = new ChallengeResult();
                return;
            }

            var permissionService = context.HttpContext.RequestServices.GetRequiredService<PermissionService>();

            // 2. Kiểm tra quyền truy cập cơ bản vào khu vực Admin
            if (!await permissionService.CanAccessAdminAsync(user))
            {
                context.Result = new ForbidResult();
                return;
            }

            // 3. Nếu có yêu cầu quyền cụ thể, kiểm tra quyền đó
            if (!string.IsNullOrEmpty(_permission))
            {
                if (!await permissionService.HasPermissionAsync(user, _permission))
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            await next();
        }
    }
}
