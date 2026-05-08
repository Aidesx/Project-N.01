using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_385.Models;
using Project_385.Services.MoMo;
using Project_385.Repositery;
using Project_385.Areas.Admin.Repositery; 

namespace Project_385.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IMomoService _momoService;
        private readonly DataContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateRenderer _emailRenderer;

        public PaymentController(IMomoService momoService, DataContext context, IEmailSender emailSender, IEmailTemplateRenderer emailRenderer)
        {
            _momoService = momoService;
            _context = context;
            _emailSender = emailSender;
            _emailRenderer = emailRenderer;
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallBack()
        {
            try
            {
                var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);

                if (!response.IsValidSignature)
                {
                    TempData["Error"] = "Giao dịch không hợp lệ (Lỗi chữ ký).";
                    return RedirectToAction("Index", "Cart");
                }

                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderNumber == response.OrderId);

                if (order != null)
                {
                    if (response.ResultCode == "0") 
                    {
                        order.Status = 1;
                        await _context.SaveChangesAsync();

                        var cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

                        try
                        {
                            await SendOrderEmail(order.UserName, order, cartItems);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Lỗi gửi mail: " + ex.Message);
                        }

                        HttpContext.Session.Remove("Cart");
                        TempData["Success"] = "Thanh toán thành công! Cảm ơn bạn.";
                        return View(response);
                    }
                    else 
                    {
                        order.Status = 2;
                        await _context.SaveChangesAsync();

                        TempData["Error"] = $"Giao dịch không thành công: {response.Message}";
                        return RedirectToAction("Index", "Cart");
                    }
                }

                TempData["Error"] = "Đơn hàng không tồn tại.";
                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                return Content($"🚨 HỆ THỐNG CRASH! Lỗi chi tiết: {ex.Message} \n\n StackTrace: {ex.StackTrace}");
            }
        }

        private async Task SendOrderEmail(string userEmail, OrderModels order, List<CartItemModel> cartItems)
        {
            var itemListHtml = "";
            foreach (var item in cartItems)
            {
                itemListHtml += $"<tr><td>{item.ProductName}</td><td>{item.Quantity}</td><td>{item.Price.ToString("#,##0")} đ</td></tr>";
            }

            var body = await _emailRenderer.RenderAsync("OrderSuccess", new Dictionary<string, string>
            {
                { "UserName", userEmail },
                { "OrderNumber", order.OrderNumber },
                { "OrderDate", order.OrderDate.ToString("dd/MM/yyyy HH:mm") },
                { "Total", order.Total.ToString("#,##0") + " đ" },
                { "ItemList", itemListHtml }
            });

            await _emailSender.SendEmailAsync(userEmail, $"Xác nhận đơn hàng #{order.OrderNumber}", body);
        }
    }
}