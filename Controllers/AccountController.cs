using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project_385.Models;
using Project_385.Models.ViewModels;
namespace Project_385.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUserModels> _userManager;
        private SignInManager<AppUserModels> _signInManager;

        public AccountController(UserManager<AppUserModels> userManager, SignInManager<AppUserModels> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Login");

        }


        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModels { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModels Login)
        {
            if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(Login.UserName, Login.Password, false, false);
                if (result.Succeeded)
                {
                    if (string.IsNullOrEmpty(Login.ReturnUrl))
                    {
                        var user = await _userManager.FindByNameAsync(Login.UserName);
                        if (await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            return RedirectToAction("Index", "Product", new { area = "Admin" });
                        }
                        return Redirect("/");
                    }
                    return Redirect(Login.ReturnUrl);
                }
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
            }
            return View(Login);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserModels user)
        {
            if (ModelState.IsValid)
            {
                AppUserModels appUser = new AppUserModels { UserName = user.UserName, Email = user.Email };
                IdentityResult result = await _userManager.CreateAsync(appUser, user.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(appUser, "User"); 
                    TempData["Thành Công"] = "Tài khoản mới đã được tạo thành công";
                    return RedirectToAction("Login", "Account");
                }
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(user);
        }


        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            return Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
        }
    }
}
