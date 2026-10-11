using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_385.Services;
using Project_385.Models;
using Project_385.Models.ViewModels;
using Project_385.Repository;
using System.Linq;
using System.Security.Claims;

namespace Project_385.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUserModels> _userManager;
        private readonly SignInManager<AppUserModels> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateRenderer _emailRenderer;
        private readonly DataContext _dataContext;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<AppUserModels> userManager,
            SignInManager<AppUserModels> signInManager,
            IEmailSender emailSender,
            IEmailTemplateRenderer emailRenderer,
            DataContext dataContext,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _emailRenderer = emailRenderer;
            _dataContext = dataContext;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index() => RedirectToAction("History");

        [HttpGet]
        public async Task<IActionResult> History()
        {
            if (!(User.Identity?.IsAuthenticated ?? false)) return RedirectToAction("Login");

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var orders = await _dataContext.Orders
                .Include(o => o.OrderDetails)
                .AsNoTracking()
                .Where(o => o.UserName == user.Email || o.UserName == user.UserName)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModels { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModels login)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin.";
                return View(login);
            }

            var user = await _userManager.FindByNameAsync(login.UserName);
            if (user != null)
            {
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    TempData["Error"] = "Tài khoản chưa được xác nhận email. Vui lòng kiểm tra hòm thư hoặc gửi lại mã xác nhận.";
                    return View(login);
                }

                var result = await _signInManager.PasswordSignInAsync(
                    login.UserName,
                    login.Password,
                    isPersistent: login.RememberMe, 
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    TempData["Success"] = "Đăng nhập thành công!";
                    return RedirectToLocal(login.ReturnUrl);
                }
            }

            TempData["Error"] = "Tên đăng nhập hoặc mật khẩu không đúng.";
            return View(login);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserModels user)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng nhập thông tin hợp lệ.";
                return View(user);
            }

            var appUser = new AppUserModels { UserName = user.UserName, Email = user.Email };
            var result = await _userManager.CreateAsync(appUser, user.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(appUser, "User");

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
                var confirmLink = Url.Action("ConfirmEmail", "Account", new { userId = appUser.Id, token = token }, Request.Scheme);

                var body = await _emailRenderer.RenderAsync("ConfirmEmail", new Dictionary<string, string>
                {
                    { "UserName", appUser.UserName },
                    { "ConfirmLink", confirmLink },
                    { "ExpireTime", "24 giờ" }
                });

                try
                {
                    await _emailSender.SendEmailAsync(appUser.Email, "Xác nhận tài khoản của bạn", body);
                    TempData["Success"] = "Đăng ký thành công! Vui lòng kiểm tra Email để kích hoạt tài khoản.";
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thể gửi email kích hoạt tài khoản cho {Email}", appUser.Email);
                    TempData["Success"] = "Đăng ký thành công! Hệ thống tạm thời chưa thể gửi email kích hoạt, vui lòng liên hệ hỗ trợ.";
                }

                return RedirectToAction("Login");
            }

            TempData["Error"] = string.Join("\n", result.Errors.Select(e => e.Description));
            return View(user);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            TempData["Error"] = "Bạn không có quyền truy cập vào chức năng hoặc trang này.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null) return RedirectToAction("Index", "Home");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Login");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
                TempData["Success"] = "Xác nhận Email thành công! Bạn có thể đăng nhập ngay.";
            else
                TempData["Error"] = "Xác nhận Email thất bại hoặc link đã hết hạn.";

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Vui lòng nhập Email.";
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null && await _userManager.IsEmailConfirmedAsync(user))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = Url.Action("ResetPassword", "Account", new { userId = user.Id, token = token }, Request.Scheme);

                var body = await _emailRenderer.RenderAsync("ResetPassword", new Dictionary<string, string>
                {
                    { "UserName", user.UserName },
                    { "ResetLink", resetLink },
                    { "ExpireTime", "2 giờ" }
                });

                try
                {
                    await _emailSender.SendEmailAsync(email, "Đặt lại mật khẩu của bạn", body);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thể gửi email đặt lại mật khẩu cho {Email}", email);
                }
            }

            TempData["Success"] = "Nếu Email tồn tại, một liên kết đặt lại mật khẩu đã được gửi đi.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (userId == null || token == null) return RedirectToAction("Index", "Home");
            return View(new ResetPasswordViewModel { UserId = userId, Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Login");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["Success"] = "Mật khẩu đã được cập nhật. Hãy đăng nhập lại.";
                return RedirectToAction("Login");
            }

            TempData["Error"] = string.Join("\n", result.Errors.Select(e => e.Description));
            return View(model);
        }

        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            TempData["Success"] = "Bạn đã đăng xuất thành công.";
            return RedirectToLocal(returnUrl);
        }

        [HttpGet]
        public IActionResult LoginGoogle(string returnUrl = "/")
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account",
                new { returnUrl }, Request.Scheme);

            var properties = _signInManager
                .ConfigureExternalAuthenticationProperties("Google", redirectUrl);

            return Challenge(properties, "Google");
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse(string returnUrl = "/")
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["Error"] = "Đăng nhập Google thất bại.";
                return RedirectToAction("Login");
            }

            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false);

            if (result.Succeeded)
                return RedirectToLocal(returnUrl);

            var email = info.Principal.FindFirstValue(ClaimTypes.Email)!;
            var baseUserName = email.Split('@')[0];
            var candidateUserName = baseUserName;

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                int suffix = 1;
                while (await _userManager.FindByNameAsync(candidateUserName) != null)
                {
                    candidateUserName = $"{baseUserName}_{suffix++}";
                }

                user = new AppUserModels
                {
                    UserName = candidateUserName,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    TempData["Error"] = "Không thể tạo tài khoản từ Google: " + string.Join("; ", createResult.Errors.Select(e => e.Description));
                    return RedirectToAction("Login");
                }

                await _userManager.AddToRoleAsync(user, "User");
            }

            await _userManager.AddLoginAsync(user, info);
            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToLocal(returnUrl);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
