using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_385.Models;
using Project_385.Repositery;
using Project_385.Services;
using System.Threading.Tasks;

namespace Project_385.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
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
            if (!await _permissionService.HasPermissionAsync(User, "ManageOrder"))
                return Forbid();

            var orders = await _dataContext.Orders.OrderByDescending(p => p.Id).ToListAsync();

            const int pageSize = 10;
            if (pg < 1) pg = 1;

            int recsCount = orders.Count;
            var pager = new Paginate(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = orders.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;
            return View(data);
        }

        public async Task<IActionResult> ViewOrder(string orderNumber)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            if (order == null) return NotFound();
            var orderDetails = await _dataContext.OrderDetails.Where(o => o.OrderId == order.Id).ToListAsync();

            return View(orderDetails);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var order = await _dataContext.Orders.FindAsync(id);
            if (order == null) return NotFound();

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
