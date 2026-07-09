using AgroConnect.Domain.Entities;
using AgroConnect.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroConnect.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var pendingApplications = await _context.FarmerProfiles
                .Include(f => f.ApplicationUser)
                .Where(f => f.Status == ApplicationStatus.Pending)
                .ToListAsync();

            return View(pendingApplications);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var profile = await _context.FarmerProfiles.Include(f => f.ApplicationUser).FirstOrDefaultAsync(f => f.Id == id);
            if (profile != null)
            {
                profile.Status = ApplicationStatus.Approved;
                await _userManager.AddToRoleAsync(profile.ApplicationUser!, "Farmer");
                await _userManager.RemoveFromRoleAsync(profile.ApplicationUser!, "Member");
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var profile = await _context.FarmerProfiles.FindAsync(id);
            if (profile != null)
            {
                profile.Status = ApplicationStatus.Rejected;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
