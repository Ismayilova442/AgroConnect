using AgroConnect.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgroConnect.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: /Admin/Admin
        public async Task<IActionResult> Index()
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");

            var all = admins.Concat(superAdmins)
                .DistinctBy(u => u.Id)
                .OrderBy(u => u.UserName)
                .ToList();

            var vmList = new List<AdminListItemViewModel>();
            foreach (var user in all)
            {
                var roles = await _userManager.GetRolesAsync(user);
                vmList.Add(new AdminListItemViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    IsSuperAdmin = roles.Contains("SuperAdmin")
                });
            }

            return View(vmList);
        }

        // GET: /Admin/Admin/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Admin/Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateAdminViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError("", "Bu email ilə istifadəçi artıq mövcuddur.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Admin");

            TempData["Success"] = $"{model.Email} admin olaraq əlavə edildi.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Admin/RemoveAdmin/{id}
        [HttpPost]
        public async Task<IActionResult> RemoveAdmin(string id)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == id)
            {
                TempData["Error"] = "Öz admin rolunuzu silə bilməzsiniz.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("SuperAdmin"))
            {
                TempData["Error"] = "SuperAdmin rolunu bu şəkildə silə bilməzsiniz.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.RemoveFromRoleAsync(user, "Admin");
            TempData["Success"] = $"{user.Email} admin siyahısından çıxarıldı.";
            return RedirectToAction(nameof(Index));
        }
    }

    public class AdminListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsSuperAdmin { get; set; }
    }

    public class CreateAdminViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Email tələb olunur")]
        [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Düzgün email daxil edin")]
        public string Email { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Şifrə tələb olunur")]
        [System.ComponentModel.DataAnnotations.MinLength(6, ErrorMessage = "Şifrə ən azı 6 simvol olmalıdır")]
        public string Password { get; set; } = string.Empty;
    }
}