using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_385.Models;
using Project_385.Models.ViewModels;
using Project_385.Repositery;
using System.Diagnostics;

namespace Project_385.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _dataContext;
        private readonly UserManager<AppUserModels> _userManager;

        public HomeController(ILogger<HomeController> logger, DataContext context, UserManager<AppUserModels> userManager)
        {
            _logger = logger;
            _dataContext = context;
            _userManager = userManager;
        }


        public IActionResult Index()
        {
            var product = _dataContext.Products.Include("Category").Include("Brand").ToList();
            return View(product);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statusCode)
        {
            if (statusCode == 404)
            {
                return View("NotFound");
            }
            else
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
        public async Task<IActionResult> WishList()
        {
            var wishlist_product = await (from w in _dataContext.WishList  
                                          join p in _dataContext.Products on w.ProductID equals p.Id
                                          join u in _dataContext.Users on w.UserId equals u.Id
                                          where w.UserId == _userManager.GetUserId(User)
                                          select new WishListViewModel
                                          {
                                              User = u,
                                              Product = p,
                                              WishList = w
                                          }).ToListAsync();
            return View(wishlist_product);
        }

        public async Task<IActionResult> Compare()
        {
            var compare_product = await (from c in _dataContext.CompareModels 
                                         join p in _dataContext.Products on c.ProductID equals p.Id
                                         join u in _dataContext.Users on c.UserId equals u.Id
                                         where c.UserId == _userManager.GetUserId(User)
                                         select new CompareViewModel
                                         {
                                             User = u,
                                             Product = p,
                                             Compares = c
                                         }).ToListAsync();
            return View(compare_product);
        }

        public async Task<IActionResult> AddWishList(int Id) 
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized("Vui lòng đăng nhập");

            var exists = await _dataContext.WishList.AnyAsync(w => w.ProductID == Id && w.UserId == user.Id);
            if (exists)
            {
                return Ok(new { success = true, message = "Sản phẩm đã có trong danh sách yêu thích" });
            }

            var wishListproduct = new WishListModels
            {
                ProductID = Id,
                UserId = user.Id
            };

            _dataContext.WishList.Add(wishListproduct);
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Thêm vào wishlist thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Có lỗi với việc thêm wishlist");
            }
        }

        public async Task<IActionResult> AddCompare(int Id) 
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized("Vui lòng đăng nhập");

            var exists = await _dataContext.CompareModels.AnyAsync(c => c.ProductID == Id && c.UserId == user.Id);
            if (exists)
            {
                return Ok(new { success = true, message = "Sản phẩm đã có trong danh sách so sánh" });
            }

            var compareProduct = new CompareModels
            {
                ProductID = Id,
                UserId = user.Id
            };

            _dataContext.CompareModels.Add(compareProduct);
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Thêm vào so sánh thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Có lỗi với việc thêm so sánh");
            }
        }
        public async Task<IActionResult> RemoveWishList(int Id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized("Vui lòng đăng nhập");

            var item = await _dataContext.WishList
                .FirstOrDefaultAsync(w => w.ProductID == Id && w.UserId == user.Id);

            if (item != null)
            {
                _dataContext.WishList.Remove(item);
                await _dataContext.SaveChangesAsync();
            }

            return RedirectToAction("WishList");
        }

        public async Task<IActionResult> RemoveCompare(int Id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized("Vui lòng đăng nhập");

            var item = await _dataContext.CompareModels
                .FirstOrDefaultAsync(c => c.ProductID == Id && c.UserId == user.Id);

            if (item != null)
            {
                _dataContext.CompareModels.Remove(item);
                await _dataContext.SaveChangesAsync();
            }

            return RedirectToAction("Compare");
        }
    }
}


