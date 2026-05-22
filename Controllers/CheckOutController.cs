using Microsoft.AspNetCore.Mvc;
using Project_385.Models;
using Project_385.Models.ViewModels;
using Project_385.Repositery;
using Project_385.Services;
using Project_385.Services.MoMo;
using System.Security.Claims;

namespace Project_385.Controllers
{
    public class CheckOutController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IMomoService _momoService;
        private readonly CartPricingService _cartPricingService;

        public CheckOutController(DataContext dataContext, IMomoService momoService, CartPricingService cartPricingService)
        {
            _dataContext = dataContext;
            _momoService = momoService;
            _cartPricingService = cartPricingService;
        }

        public IActionResult Index()
        {
            var cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            var viewModel = _cartPricingService.BuildSummary(cartItems);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(double? currentLatitude, double? currentLongitude)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null) return RedirectToAction("Login", "Account");

            var cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống";
                return RedirectToAction("Index", "Cart");
            }

            var pricing = _cartPricingService.BuildSummary(cartItems, currentLatitude, currentLongitude);
            if (!pricing.CanDeliver)
            {
                TempData["Error"] = pricing.DeliveryMessage ?? "Vui lòng lấy vị trí hiện tại để tính phí di chuyển.";
                return RedirectToAction("Index");
            }

            var order = new OrderModels
            {
                OrderNumber = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                OrderDate = DateTime.Now,
                UserName = userEmail,
                Total = pricing.GrandTotal,
                Status = 0
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

            var requestInfo = new OrderInfoModel
            {
                FullName = order.UserName,
                OrderId = order.OrderNumber,
                OrderInfo = "Thanh toán đơn hàng " + order.OrderNumber,
                Amount = ((long)order.Total).ToString()
            };

            var responseFromMomo = await _momoService.CreatePaymentMomo(requestInfo);

            if (responseFromMomo != null && !string.IsNullOrEmpty(responseFromMomo.PayUrl))
            {
                return Redirect(responseFromMomo.PayUrl);
            }

            TempData["Error"] = "Không thể khởi tạo giao dịch MoMo. Vui lòng thử lại.";
            return RedirectToAction("Index", "Cart");
        }
    }
}
