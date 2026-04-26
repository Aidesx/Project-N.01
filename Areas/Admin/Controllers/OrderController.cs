using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Project_385.Areas.Admin.Controllers
{
    public class OrderController : Controller
    {
        [Area("Admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View();
        }
        

    } 
}
