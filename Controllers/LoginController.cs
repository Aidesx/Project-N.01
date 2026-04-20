using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_385.Models;
using Project_385.Repositery;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Cryptography;
using System.Text;

namespace Project_385.Controllers
{
    public class LoginController : Controller
    {
        private readonly DataContext _dataContext;

        public LoginController(DataContext context)
        {
            _dataContext = context;
        }

        private string Hash(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(input)));
            }
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(LoginModels user)
        {
            if (ModelState.IsValid)
            {
                var exists = await _dataContext.Users.AnyAsync(u => u.UserName == user.UserName);
                if (exists)
                {
                    ModelState.AddModelError("", "Tên đăng nhập này đã được sử dụng");
                    return View(user);
                }

                user.Password = Hash(user.Password);

                user.RoleId = 2;

                _dataContext.Users.Add(user);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Đăng ký thành công!";
                return RedirectToAction("Login");
            }
            return View(user);
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginModels loginModel)
        {
            string hashedPassword = Hash(loginModel.Password);

            var user = await _dataContext.Users.FirstOrDefaultAsync(u =>
                u.UserName == loginModel.UserName &&
                u.Password == hashedPassword);

            if (user != null)
            {
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, user.UserName), 
                    new Claim(ClaimTypes.Role, user.RoleId.ToString()) 
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                if (user.RoleId == 1) 
                {
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }


                return RedirectToAction("Index", "Home", new { area = "" });
            }

            ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu");
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}