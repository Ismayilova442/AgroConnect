using AgroConnect.Domain.Entities;
using AgroConnect.Infrastructure.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroConnect.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private const int PageSize = 10;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: /Admin/User
        public async Task<IActionResult> Index(string? search, string? role, int page = 1)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    (u.UserName != null && u.UserName.Contains(search)) ||
                    (u.Email != null && u.Email.Contains(search)));
            }

            var matchedUsers = await query.ToListAsync();

            var viewModelList = new List<UserListItemViewModel>();
            foreach (var user in matchedUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var isLocked = await _userManager.IsLockedOutAsync(user);

                viewModelList.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    Roles = roles.ToList(),
                    IsLocked = isLocked
                });
            }

            // Rol filtri (DB səviyyəsində join tələb etdiyi üçün yaddaşda edilir)
            if (!string.IsNullOrWhiteSpace(role))
            {
                viewModelList = viewModelList.Where(u => u.Roles.Contains(role)).ToList();
            }

            var ordered = viewModelList.OrderBy(u => u.UserName).ToList();
            var paged = PaginatedList<UserListItemViewModel>.Create(ordered, page, PageSize);

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentRole = role;
            ViewBag.Roles = new List<string> { "SuperAdmin", "Admin", "Farmer", "Member" };

            return View(paged);
        }

        // POST: /Admin/User/ToggleLock/{id}
        [HttpPost]
        public async Task<IActionResult> ToggleLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // SuperAdmin özünü bloklaya bilməz
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == id)
            {
                TempData["Error"] = "Öz hesabınızı bloklaya bilməzsiniz.";
                return RedirectToAction(nameof(Index));
            }

            var isLocked = await _userManager.IsLockedOutAsync(user);

            // Lockout-un işləməsi üçün əvvəlcə aktiv olduğuna əmin oluruq
            if (!user.LockoutEnabled)
            {
                await _userManager.SetLockoutEnabledAsync(user, true);
            }

            if (isLocked)
            {
                // Blokdan çıxar
                await _userManager.SetLockoutEndDateAsync(user, null);
                TempData["Success"] = $"{user.Email} blokdan çıxarıldı.";
            }
            else
            {
                // Blokla (uzun müddətə)
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                TempData["Success"] = $"{user.Email} bloklandı.";
            }

            return RedirectToAction(nameof(Index));
        }
    }

    public class UserListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public bool IsLocked { get; set; }
    }
}