using AgroConnect.Domain.Entities;
using AgroConnect.Infrastructure.Data;
using AgroConnect.Web.Helpers;
using AgroConnect.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroConnect.Web.Controllers
{
    public class MarketplaceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public MarketplaceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
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

        // Müştərinin öz keçmiş sifarişlərinə baxdığı səhifə
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.ApplicationUserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
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

            // Səbətdəki məhsulları (fermer məlumatı ilə birgə) bir dəfəyə çəkirik
            var productIds = cart.Select(c => c.ProductId).Distinct().ToList();
            var products = await _context.Products
                .Include(p => p.FarmerProfile)
                    .ThenInclude(f => f!.ApplicationUser)
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            foreach (var item in cart)
            {
                newOrder.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                });

                // Stoku azaldırıq
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                    if (product.StockQuantity < 0)
                    {
                        product.StockQuantity = 0;
                    }
                }
            }

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            // Hər fermerə YALNIZ öz məhsulları üçün bildiriş email-i göndəririk
            await NotifyFarmersAsync(cart, products, newOrder);

            HttpContext.Session.Remove(cartKey); // Clear cart

            ViewBag.Message = "Sifarişiniz uğurla tamamlandı! Təşəkkür edirik.";
            return View("OrderSuccess");
        }

        private async Task NotifyFarmersAsync(List<CartItem> cart, List<Product> products, Order order)
        {
            // Hər cart item-i özünə aid Product-a bağlayıb, fermerin email-inə görə qruplaşdırırıq
            var itemsWithProduct = cart
                .Select(item => new
                {
                    CartItem = item,
                    Product = products.FirstOrDefault(p => p.Id == item.ProductId)
                })
                .Where(x => x.Product?.FarmerProfile?.ApplicationUser?.Email != null)
                .ToList();

            var farmerGroups = itemsWithProduct.GroupBy(x => x.Product!.FarmerProfile!.ApplicationUser!.Email);

            foreach (var group in farmerGroups)
            {
                var farmerEmail = group.Key!;

                var itemsListHtml = string.Join("", group.Select(x =>
                    $"<li>{x.CartItem.Title} — {x.CartItem.Quantity} ədəd × {x.CartItem.Price} AZN</li>"));

                var subject = "AgroConnect - Yeni Sifarişiniz Var!";
                var body = $@"
                    <div style='font-family:Segoe UI,Arial,sans-serif;max-width:500px;margin:auto;'>
                        <h2 style='color:#2e7d32;'>Yeni Sifariş Bildirişi</h2>
                        <p>Aşağıdakı məhsullarınız üçün yeni sifariş verildi:</p>
                        <ul>{itemsListHtml}</ul>
                        <hr/>
                        <p><strong>Çatdırılma ünvanı:</strong> {order.ShippingAddress}</p>
                        <p><strong>Əlaqə nömrəsi:</strong> {order.ContactNumber}</p>
                        <p><strong>Ödəniş növü:</strong> {order.PaymentMethod}</p>
                        <br/>
                        <p style='color:#888;font-size:13px;'>Bu, avtomatik göndərilən mesajdır, cavablamayın.</p>
                    </div>";

                await _emailSender.SendEmailAsync(farmerEmail, subject, body);
            }
        }
    }
}