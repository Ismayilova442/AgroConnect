using AgroConnect.Domain.Entities;
using AgroConnect.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AgroConnect.Web.Controllers
{
    [Authorize(Roles = "Farmer,SuperAdmin")]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public ChatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserName = $"{user!.FirstName} {user.LastName}";
            // Son 50 mesajı gətirək
            var messages = await _context.ChatMessages
                .Include(m => m.ApplicationUser)
                .OrderByDescending(m => m.SentAt)
                .Take(50)
                .ToListAsync();
            messages.Reverse(); // Köhnədən yeniyə düzmək üçün
            return View(messages);
        }
    }
}