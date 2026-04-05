using Microsoft.AspNetCore.Mvc;
using Project_385.Repositery;

namespace Project_385.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;

        public ProductController(DataContext context)
        {
            _dataContext = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult>Details(int ?Id )
        {
            if(Id == null ){return RedirectToAction("Index");
            }
            var productsByID = _dataContext.Products.Where(b => b.Id == Id).FirstOrDefault();

            return View(productsByID);
        }
    }
}
