using Microsoft.AspNetCore.Mvc;
using Project_385.Models.ViewModels;
using Project_385.Models;
using Project_385.Repositery;

namespace Project_385.Controllers
{
    public class CartController : Controller
    {
        private readonly DataContext _dataContext;

        public CartController(DataContext _Context)
        {
            _dataContext = _Context;
        }
        public IActionResult Index()
        {
            List<CartItemModel> CartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemViewModel caVN = new()
            {
                CartItems = CartItems,
                GrandTotal = CartItems.Sum(x => x.Quantity * x.Price)

            };
            return View(caVN);
        }

        public IActionResult Checkout()
        {
            return View("~/Views/Checkout/Index.cshtml");
        }

        public async Task<IActionResult> AddToCart(int id)
        {
            ProductModels product = await _dataContext.Products.FindAsync(id);
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemModel cartItem = cart.Where(x => x.ProductId == id).FirstOrDefault();

            if (cartItem == null)
            {
                cart.Add(new CartItemModel(product));

            }else
            {
                cartItem.Quantity += 1;
            }
                return Redirect(Request.Headers["Referer"].ToString());
            }
        }
    }

