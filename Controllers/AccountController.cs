using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project_385.Models;
using Project_385.Models.ViewModels;
using Project_385.Repositery;
using Project_385.Areas.Admin.Repositery;
using System.Linq;

namespace Project_385.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUserModels> _userManager;
        private readonly SignInManager<AppUserModels> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateRenderer _emailRenderer;

        public AccountController(
            UserManager<AppUserModels> userManager,
            SignInManager<AppUserModels> signInManager,
            IEmailSender emailSender,
            IEmailTemplateRenderer emailRenderer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _emailRenderer = emailRenderer;
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

                var result = await _signInManager.PasswordSignInAsync(login.UserName, login.Password, false, false);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Đăng nhập thành công!";
                    return Redirect(login.ReturnUrl ?? "/");
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
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
                var confirmLink = Url.Action("ConfirmEmail", "Account", new { userId = appUser.Id, token = token }, Request.Scheme);

                var body = await _emailRenderer.RenderAsync("ConfirmEmail", new Dictionary<string, string>
                {
                    { "UserName", appUser.UserName },
                    { "ConfirmLink", confirmLink },
                    { "ExpireTime", "24 giờ" }
                });

                await _emailSender.SendEmailAsync(appUser.Email, "Xác nhận tài khoản của bạn", body);

                TempData["Success"] = "Đăng ký thành công! Vui lòng kiểm tra Email để kích hoạt tài khoản.";
                return RedirectToAction("Login");
            }

            TempData["Error"] = string.Join("\n", result.Errors.Select(e => e.Description));
            return View(user);
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
            {
                TempData["Success"] = "Xác nhận Email thành công! Bạn có thể đăng nhập ngay.";
                return RedirectToAction("Login");
            }

            TempData["Error"] = "Xác nhận Email thất bại hoặc link đã hết hạn.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Vui lòng nhập Email.";
                return View();
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

                await _emailSender.SendEmailAsync(email, "Đặt lại mật khẩu của bạn", body);
            }

            TempData["Success"] = "Nếu Email tồn tại, một liên kết đặt lại mật khẩu đã được gửi đi.";
            return View();
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
            return Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
        }
    }
}