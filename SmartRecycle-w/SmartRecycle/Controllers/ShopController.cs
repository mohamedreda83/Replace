using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartRecycle.Models;
using SmartRecycle.Repositories;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartRecycle.Controllers
{
    [Authorize]
    public class ShopController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICartRepository _cartRepository;
        private readonly SmartRecycleContext _context;
        public int userId { get; set; }
        public string userids { get; set; }
        public ShopController(IProductRepository productRepository, ICartRepository cartRepository, SmartRecycleContext context)
        {
            _productRepository = productRepository;
            _cartRepository = cartRepository;
            _context = context;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (TempData["Id"] != null)
            {
                userids = TempData["Id"].ToString();
                TempData["Id"] = userids;

                if (int.TryParse(userids, out int parsedId))
                {
                    userId = parsedId;
                }
                else
                {
                    userId = 0; // أو أي قيمة افتراضية حسب اختيارك
                }
            }

            base.OnActionExecuting(filterContext);
        }

        public void l()
        {
            var user = _context.Users.FirstOrDefault(a => a.Id == userId);
            ViewBag.UserName = user.Username;
            ViewBag.UserPoints = user.Points;
        }
        // GET: /Shop
        public async Task<IActionResult> Index()
        {
            l();
            var products = await _productRepository.GetAllProductsAsync();
            var categories = await _productRepository.GetAllCategoriesAsync();

            ViewBag.Categories = categories;
            return View(products);
        }

        // GET: /Shop/Category/5
        public async Task<IActionResult> Category(int id)
        {
            l();
            var category = await _productRepository.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var products = await _productRepository.GetProductsByCategoryAsync(id);

            ViewBag.CategoryName = category.Name;
            ViewBag.CategoryId = category.Id;
            ViewBag.Categories = await _productRepository.GetAllCategoriesAsync();

            return View("Index", products);
        }

        // GET: /Shop/Details/5
        public async Task<IActionResult> Details(int id)
        {
            l();
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: /Shop/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            l();
            if (quantity <= 0)
            {
                quantity = 1;
            }

       
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null || !product.IsAvailable || product.Stock < quantity)
            {
                TempData["ErrorMessage"] = "المنتج غير متوفر بالكمية المطلوبة";
                return RedirectToAction("Details", new { id = productId });
            }

            await _cartRepository.AddItemToCartAsync(userId, productId, quantity);
            TempData["SuccessMessage"] = "تمت إضافة المنتج إلى سلة المشتريات بنجاح";

            return RedirectToAction("Cart");
        }

        // GET: /Shop/Cart
        public async Task<IActionResult> Cart()
        {
            l();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            return View(cart);
        }

        // POST: /Shop/UpdateCart
        [HttpPost]
        public async Task<IActionResult> UpdateCart(int cartItemId, int quantity)
        {
            l();
            if (quantity <= 0)
            {
                await _cartRepository.RemoveCartItemAsync(cartItemId);
            }
            else
            {
                await _cartRepository.UpdateCartItemQuantityAsync(cartItemId, quantity);
            }

            TempData["SuccessMessage"] = "تم تحديث سلة المشتريات بنجاح";
            return RedirectToAction("Cart");
        }

        // POST: /Shop/RemoveFromCart
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            l();
            await _cartRepository.RemoveCartItemAsync(cartItemId);
            TempData["SuccessMessage"] = "تم حذف المنتج من سلة المشتريات بنجاح";
            return RedirectToAction("Cart");
        }

        // GET: /Shop/ClearCart
        public async Task<IActionResult> ClearCart()
        {
            l();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            await _cartRepository.ClearCartAsync(userId);
            TempData["SuccessMessage"] = "تم تفريغ سلة المشتريات بنجاح";
            return RedirectToAction("Cart");
        }

        //private int GetCurrentUserId()
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        //        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        //        {
        //            return userId;
        //        }
        //    }
        //    return 0;
        //}
    }
}