using AgroConnect.Domain.Entities;
using AgroConnect.Infrastructure.Data;
using AgroConnect.Web.Helpers;
using AgroConnect.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroConnect.Web.Controllers
{
    public class MarketplaceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MarketplaceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Hər istifadəçinin (və ya qonağın) səbəti öz açarında saxlanılır ki,
        // fərqli hesablar arasında qarışma olmasın.
        private string GetCartSessionKey()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                return $"Cart_{userId}";
            }
            return "Cart_Guest";
        }

        public async Task<IActionResult> Index(int? categoryId, string? searchString)
        {
            var products = _context.Products.Include(p => p.Category).Include(p => p.FarmerProfile).AsQueryable();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Title.Contains(searchString) || p.Description.Contains(searchString));
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(await products.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            // Hesabsız (anonim) istifadəçi səbətə məhsul əlavə edə bilməz
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["Error"] = "Səbətə məhsul əlavə etmək üçün hesabınız olmalıdır. Zəhmət olmasa daxil olun və ya qeydiyyatdan keçin.";
                return RedirectToAction(nameof(Index));
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var cartKey = GetCartSessionKey();
            List<CartItem> cart = HttpContext.Session.Get<List<CartItem>>(cartKey) ?? new List<CartItem>();

            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Title = product.Title,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                });
            }

            HttpContext.Session.Set(cartKey, cart);
            return RedirectToAction(nameof(Cart));
        }

        [Authorize]
        public IActionResult Cart()
        {
            var cartKey = GetCartSessionKey();
            List<CartItem> cart = HttpContext.Session.Get<List<CartItem>>(cartKey) ?? new List<CartItem>();
            return View(cart);
        }

        [Authorize]
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cartKey = GetCartSessionKey();
            List<CartItem> cart = HttpContext.Session.Get<List<CartItem>>(cartKey) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.Set(cartKey, cart);
            }
            return RedirectToAction(nameof(Cart));
        }

        // Səbətdəki miqdarı artırır/azaldır (change: +1 və ya -1)
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int change)
        {
            var cartKey = GetCartSessionKey();
            List<CartItem> cart = HttpContext.Session.Get<List<CartItem>>(cartKey) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item != null)
            {
                var product = await _context.Products.FindAsync(productId);
                int maxStock = product?.StockQuantity ?? int.MaxValue;

                item.Quantity += change;

                if (item.Quantity < 1)
                {
                    item.Quantity = 1;
                }
                if (item.Quantity > maxStock)
                {
                    item.Quantity = maxStock;
                }

                HttpContext.Session.Set(cartKey, cart);
            }

            return RedirectToAction(nameof(Cart));
        }

        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            var cartKey = GetCartSessionKey();
            List<CartItem> cart = HttpContext.Session.Get<List<CartItem>>(cartKey) ?? new List<CartItem>();
            if (!cart.Any()) return RedirectToAction(nameof(Index));

            return View(new Order()); // Using domain model directly for simplicity in prototype
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cartKey = GetCartSessionKey();
            List<CartItem> cart = HttpContext.Session.Get<List<CartItem>>(cartKey) ?? new List<CartItem>();
            if (!cart.Any()) return RedirectToAction(nameof(Index));

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var newOrder = new Order
            {
                ApplicationUserId = user.Id,
                ShippingAddress = order.ShippingAddress,
                ContactNumber = order.ContactNumber,
                TotalAmount = cart.Sum(c => c.Total),
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending
            };

            foreach (var item in cart)
            {
                newOrder.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                });
            }

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove(cartKey); // Clear cart

            ViewBag.Message = "Sifarişiniz uğurla tamamlandı! Təşəkkür edirik.";
            return View("OrderSuccess");
        }
    }
}