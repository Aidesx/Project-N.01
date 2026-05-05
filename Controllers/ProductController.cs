using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_385.Models;
using Project_385.Models.ViewModels;
using Project_385.Repositery;

namespace Project_385.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<AppUserModels> _userManager;
        public ProductController(DataContext context, UserManager<AppUserModels> userManager)
        {
            _dataContext = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int? Id)
        {
            if (Id == null) return RedirectToAction("Index");

            var productsByID = await _dataContext.Products
                .Include(p => p.Ratings)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(b => b.Id == Id)
                .FirstOrDefaultAsync();

            if (productsByID == null) return RedirectToAction("Index");

            bool hasReviewed = false;
            if (User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(User); 
                if (currentUser != null)
                {
                    hasReviewed = await _dataContext.Ratings
                        .AnyAsync(r => r.ProductId == Id && r.Email == currentUser.Email);

                    ViewBag.UserEmail = currentUser.Email;
                    ViewBag.UserName = currentUser.UserName;
                }
            }
            ViewBag.HasReviewed = hasReviewed;

            var viewModels = new ProductDetailViewModels
            {
                Product = productsByID,
                Ratings = productsByID.Ratings?.ToList() ?? new List<RatingModels>(),
                Rating = new RatingModels { ProductId = productsByID.Id }
            };

            ViewBag.RelatedByBrand = await _dataContext.Products.Where(p => p.BrandID == productsByID.BrandID && p.Id != productsByID.Id).Take(4).ToListAsync();
            ViewBag.RelatedByCategory = await _dataContext.Products.Where(p => p.CategoryID == productsByID.CategoryID && p.Id != productsByID.Id).Take(6).ToListAsync();

            return View(viewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] 
        public async Task<IActionResult> CommentProduct(RatingModels Rating)
        {
            if (Rating.ProductId == 0) return RedirectToAction("Index", "Home");
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            string userEmail = currentUser.Email;

            var alreadyExists = await _dataContext.Ratings.AnyAsync(r => r.ProductId == Rating.ProductId && r.Email == userEmail);

            if (alreadyExists)
            {
                TempData["Error"] = "Bạn đã đánh giá sản phẩm này rồi!";
                return RedirectToAction("Details", new { Id = Rating.ProductId });
            }

            if (ModelState.IsValid)
            {
                Rating.Email = userEmail;
                Rating.Name = currentUser.UserName; 

                _dataContext.Ratings.Add(Rating);
                await _dataContext.SaveChangesAsync();
                TempData["Success"] = "Cảm ơn bạn đã đánh giá!";
            }
            else
            {
                TempData["Error"] = "Vui lòng nhập nội dung đánh giá.";
            }

            return RedirectToAction("Details", new { Id = Rating.ProductId });
        }
    }
}