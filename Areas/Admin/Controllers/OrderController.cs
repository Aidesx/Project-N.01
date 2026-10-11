using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_385.Areas.Admin.Filters;
using Project_385.Models;
using Project_385.Repository;
using Project_385.Services;
using System.Threading.Tasks;

namespace Project_385.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize("ManageOrder")]
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly PermissionService _permissionService;

        public OrderController(DataContext context, PermissionService permissionService)
        {
            _dataContext = context;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index(int pg = 1)
        {
            const int pageSize = 10;
            if (pg < 1) pg = 1;

            int recsCount = await _dataContext.Orders.CountAsync();
            var pager = new Paginate(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;

            var data = await _dataContext.Orders
                .AsNoTracking()
                .OrderByDescending(p => p.Id)
                .Skip(recSkip)
                .Take(pager.PageSize)
                .ToListAsync();

            ViewBag.Pager = pager;
            return View(data);
        }

        public async Task<IActionResult> ViewOrder(string orderNumber)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            if (order == null) return NotFound();
            var orderDetails = await _dataContext.OrderDetails.Where(o => o.OrderId == order.Id).ToListAsync();

            ViewBag.Order = order;
            return View(orderDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(string orderNumber, int status)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
            if (order == null) return NotFound();

            order.Status = status;
            await _dataContext.SaveChangesAsync();

            TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công!";
            return RedirectToAction("ViewOrder", new { orderNumber = orderNumber });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _dataContext.Orders.FindAsync(id);
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng này!";
                return RedirectToAction("Index");
            }
            

            var orderDetails = await _dataContext.OrderDetails
                .Where(o => o.OrderId == id)
                .ToListAsync();

            _dataContext.OrderDetails.RemoveRange(orderDetails);
            _dataContext.Orders.Remove(order);
            await _dataContext.SaveChangesAsync();

            TempData["Success"] = "Đã xóa đơn hàng thành công";
            return RedirectToAction("Index");
        }
    } 
}
