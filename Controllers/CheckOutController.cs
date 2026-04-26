using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Project_385.Models;
using Project_385.Models.ViewModels;
using Project_385.Repositery;

namespace Project_385.Controllers
{
    public class CheckOutController : Controller
    {
        private readonly DataContext _dataContext;

        public CheckOutController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public IActionResult Index()
        {
            var cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart")
                            ?? new List<CartItemModel>();

            var viewModel = new CartItemViewModel
            {
                CartItems = cartItems,
                GrandTotal = cartItems.Sum(x => x.Price * x.Quantity)
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CheckOut()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart")
                            ?? new List<CartItemModel>();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống";
                return RedirectToAction("Index", "Cart");
            }

            var order = new OrderModels
            {
                OrderNumber = Guid.NewGuid().ToString(),
                UserName = userEmail,
                OrderDate = DateTime.Now,
                Total = cartItems.Sum(x => x.Price * x.Quantity),
                Status = 1
            };
            _dataContext.Orders.Add(order);
            await _dataContext.SaveChangesAsync();
            var orderDetails = cartItems.Select(cart => new OrderDetailModels
            {
                OrderId = order.Id,
                ProductId = cart.ProductId,
                ProductName = cart.ProductName,
                Quantity = cart.Quantity,
                Price = cart.Price
            }).ToList();

            _dataContext.OrderDetails.AddRange(orderDetails);
            await _dataContext.SaveChangesAsync();
            HttpContext.Session.Remove("Cart");
            TempData["Success"] = "Checkout thành công, vui lòng chờ duyệt đơn hàng";
            return RedirectToAction("Index", "Cart");
        }
    }
}