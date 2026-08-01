using AgroConnect.Domain.Entities;
using AgroConnect.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroConnect.Web.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Wishlist
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var items = await _context.Wishlists
                .Include(w => w.Product)
                    .ThenInclude(p => p!.Category)
                .Where(w => w.ApplicationUserId == user.Id)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return View(items);
        }

        // POST: /Wishlist/Toggle
        // Məhsul artıq sevimlilərdədirsə çıxarır, yoxdursa əlavə edir
        [HttpPost]
        public async Task<IActionResult> Toggle(int productId, string? returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var existing = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.ApplicationUserId == user.Id && w.ProductId == productId);

            if (existing != null)
            {
                _context.Wishlists.Remove(existing);
                TempData["Success"] = "Məhsul sevimlilərdən çıxarıldı.";
            }
            else
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null) return NotFound();

                _context.Wishlists.Add(new Wishlist
                {
                    ApplicationUserId = user.Id,
                    ProductId = productId,
                    CreatedAt = DateTime.Now
                });
                TempData["Success"] = "Məhsul sevimlilərə əlavə olundu.";
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Wishlist/Remove/5
        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var item = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.Id == id && w.ApplicationUserId == user.Id);

            if (item != null)
            {
                _context.Wishlists.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}