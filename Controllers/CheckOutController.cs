using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Project_385.Models;
using Project_385.Models.ViewModels;
using Project_385.Repositery;
using Project_385.Areas.Admin.Repositery;
using System.Collections.Generic;

namespace Project_385.Controllers
{
    public class CheckOutController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateRenderer _emailRenderer;

        public CheckOutController(IEmailSender emailSender, DataContext dataContext, IEmailTemplateRenderer emailRenderer)
        {
            _dataContext = dataContext;
            _emailSender = emailSender;
            _emailRenderer = emailRenderer;
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
                return RedirectToAction("Login", "Account");

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


            var itemListHtml = "";
            foreach (var item in cartItems)
            {
                itemListHtml += $"<tr><td style='padding:8px; border:1px solid #ddd;'>{item.ProductName}</td>" +
                                $"<td style='padding:8px; border:1px solid #ddd; text-align:center;'>{item.Quantity}</td>" +
                                $"<td style='padding:8px; border:1px solid #ddd; text-align:right;'>{item.Price.ToString("#,##0")} đ</td></tr>";
            }

            var body = await _emailRenderer.RenderAsync("OrderSuccess", new Dictionary<string, string>
            {
                { "UserName", userEmail },
                { "OrderNumber", order.OrderNumber },
                { "OrderDate", order.OrderDate.ToString("dd/MM/yyyy HH:mm") },
                { "Total", order.Total.ToString("#,##0") + " đ" },
                { "ItemList", itemListHtml }
            });

            await _emailSender.SendEmailAsync(userEmail, $"Xác nhận đơn hàng #{order.OrderNumber} từ Project 385", body);

            TempData["Success"] = "Checkout thành công, vui lòng chờ duyệt đơn hàng. Chi tiết đơn hàng đã được gửi vào email của bạn.";
            return RedirectToAction("Index", "Cart");
        }
    }
}