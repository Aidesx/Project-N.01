using Microsoft.AspNetCore.Mvc;
using Project_385.Models.ViewModels;
using Project_385.Models;
using Project_385.Repository;
using Project_385.Services;

namespace Project_385.Controllers
{
    public class CartController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly CartPricingService _cartPricingService;

        public CartController(DataContext _Context, CartPricingService cartPricingService)
        {
            _dataContext = _Context;
            _cartPricingService = cartPricingService;
        }

        public IActionResult Index()
        {
            List<CartItemModel> CartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemViewModel caVN = _cartPricingService.BuildSummary(CartItems);
            return View(caVN);
        }

        public IActionResult Checkout()
        {
            return View("~/Views/CheckOut/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Add(int id)
        {
            ProductModels product = await _dataContext.Products.FindAsync(id);

            if (product == null)
                return Json(false);

            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemModel cartItem = cart.Where(x => x.ProductId == id).FirstOrDefault();

            if (cartItem == null)
                cart.Add(new CartItemModel(product));
            else
                cartItem.Quantity += 1;

            HttpContext.Session.SetJson("Cart", cart);
            return Json(true);
        }


        public IActionResult Increase(int id)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemModel cartItem = cart.Where(c => c.ProductId == id).FirstOrDefault();

            if (cartItem != null && cartItem.Quantity >= 1)
            {
                ++cartItem.Quantity;
            }
            else if (cartItem != null)
            {
                cart.RemoveAll(p => p.ProductId == id);
            }
            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }
            TempData["Success"] = "Cập nhật số lượng thành công";
            return RedirectToAction("Index");
        }

        public IActionResult Decrease(int id)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemModel cartItem = cart.Where(c => c.ProductId == id).FirstOrDefault();

            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    --cartItem.Quantity;
                }
                else
                {
                    cart.RemoveAll(p => p.ProductId == id);
                }
            }

            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }
            TempData["Success"] = "Cập nhật số lượng thành công";
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int id)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            cart.RemoveAll(p => p.ProductId == id);

            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }
            TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng";
            return RedirectToAction("Index");
        }

        public IActionResult Clear()
        {
            HttpContext.Session.Remove("Cart");
            TempData["Success"] = "Đã xóa toàn bộ giỏ hàng";
            return RedirectToAction("Index");
        }
    }
}
