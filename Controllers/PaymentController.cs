using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_385.Areas.Admin.Repositery;
using Project_385.Models;
using Project_385.Repositery;
using Project_385.Services.MoMo;

namespace Project_385.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IMomoService _momoService;
        private readonly DataContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateRenderer _emailRenderer;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IMomoService momoService,
            DataContext context,
            IEmailSender emailSender,
            IEmailTemplateRenderer emailRenderer,
            ILogger<PaymentController> logger)
        {
            _momoService = momoService;
            _context = context;
            _emailSender = emailSender;
            _emailRenderer = emailRenderer;
            _logger = logger;
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

                        var orderDetails = await _context.OrderDetails
                            .Where(od => od.OrderId == order.Id)
                            .ToListAsync();
                        try
                        {
                            await SendOrderEmail(order.UserName, order, orderDetails);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Không thể gửi email xác nhận đơn hàng {OrderNumber}", order.OrderNumber);
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
                _logger.LogError(ex, "Lỗi khi xử lý callback thanh toán MoMo");
                TempData["Error"] = "Đã xảy ra lỗi khi xử lý thanh toán. Vui lòng thử lại sau.";
                return RedirectToAction("Index", "Cart");
            }
        }

        private async Task SendOrderEmail(string userEmail, OrderModels order, List<OrderDetailModels> orderDetails)
        {
            var itemListHtml = "";
            foreach (var item in orderDetails)
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
