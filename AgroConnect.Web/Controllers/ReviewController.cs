using AgroConnect.Domain.Entities;
using AgroConnect.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroConnect.Web.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: /Review/Add
        // Yalnız məhsulu ALMIŞ istifadəçi rəy yaza bilər, hər məhsula 1 dəfə
        [HttpPost]
        public async Task<IActionResult> Add(int productId, int rating, string comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Reytinq 1 ilə 5 arasında olmalıdır.";
                return RedirectToAction("MyOrders", "Marketplace");
            }

            // İstifadəçi bu məhsulu həqiqətən alıb-almadığını yoxlayırıq
            var hasPurchased = await _context.OrderItems
                .Include(oi => oi.Order)
                .AnyAsync(oi => oi.ProductId == productId && oi.Order!.ApplicationUserId == user.Id);

            if (!hasPurchased)
            {
                TempData["Error"] = "Yalnız aldığınız məhsullara rəy yaza bilərsiniz.";
                return RedirectToAction("MyOrders", "Marketplace");
            }

            // Artıq rəy yazıbsa, ikinci dəfə yaza bilməz (əvəzinə yenilənir)
            var existingReview = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.ApplicationUserId == user.Id);

            if (existingReview != null)
            {
                existingReview.Rating = rating;
                existingReview.Comment = comment;
                existingReview.CreatedAt = DateTime.Now;
                TempData["Success"] = "Rəyiniz yeniləndi.";
            }
            else
            {
                _context.ProductReviews.Add(new ProductReview
                {
                    ProductId = productId,
                    ApplicationUserId = user.Id,
                    Rating = rating,
                    Comment = comment ?? string.Empty,
                    CreatedAt = DateTime.Now
                });
                TempData["Success"] = "Rəyiniz üçün təşəkkürlər!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("MyOrders", "Marketplace");
        }
    }
}