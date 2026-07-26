using AgroConnect.Domain.Entities;
using AgroConnect.Infrastructure.Data;
using AgroConnect.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroConnect.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // ============================================
        // DASHBOARD - Statistikalar
        // ============================================
        public async Task<IActionResult> Index()
        {
            var vm = new DashboardViewModel
            {
                PendingCount = await _context.FarmerProfiles.CountAsync(f => f.Status == ApplicationStatus.Pending),
                ApprovedCount = await _context.FarmerProfiles.CountAsync(f => f.Status == ApplicationStatus.Approved),
                RejectedCount = await _context.FarmerProfiles.CountAsync(f => f.Status == ApplicationStatus.Rejected),
                ProductsCount = await _context.Products.CountAsync(),
                UsersCount = await _userManager.Users.CountAsync(),

                // NOT: Order entity strukturu tam məlum olmadığı üçün yalnız ümumi say götürülür.
                OrdersCount = await _context.Orders.CountAsync(),

                // NOT: ApplicationUser-də CreatedAt sahəsi görünmədiyi üçün Id-yə görə sıralanır.
                RecentUsers = await _userManager.Users
                    .OrderByDescending(u => u.Id)
                    .Take(5)
                    .ToListAsync(),

                RecentProducts = await _context.Products
                    .Include(p => p.Category)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToListAsync(),

                // NOT: FarmerProfile-da CreatedAt sahəsi görünmədiyi üçün Id-yə görə sıralanır.
                RecentFarmerRequests = await _context.FarmerProfiles
                    .Include(f => f.ApplicationUser)
                    .OrderByDescending(f => f.Id)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }

        // ============================================
        // FERMER MÜRACİƏTLƏRİ (əvvəlki Index buraya köçürüldü)
        // Yalnız SuperAdmin — fermer təsdiqləmə həssas əməliyyatdır
        // ============================================
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> FarmerRequests()
        {
            var allApplications = await _context.FarmerProfiles
                .Include(f => f.ApplicationUser)
                .ToListAsync();

            return View(allApplications);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Approve(int id)
        {
            var profile = await _context.FarmerProfiles
                .Include(f => f.ApplicationUser)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (profile != null)
            {
                profile.Status = ApplicationStatus.Approved;

                try
                {
                    if (profile.ApplicationUser != null)
                    {
                        var isInFarmerRole = await _userManager.IsInRoleAsync(profile.ApplicationUser, "Farmer");
                        if (!isInFarmerRole)
                        {
                            await _userManager.AddToRoleAsync(profile.ApplicationUser, "Farmer");
                        }

                        var isInMemberRole = await _userManager.IsInRoleAsync(profile.ApplicationUser, "Member");
                        if (isInMemberRole)
                        {
                            await _userManager.RemoveFromRoleAsync(profile.ApplicationUser, "Member");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Rol dəyişdirilmə xətası: " + ex.Message);
                }

                _context.Entry(profile).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                // Fermerə təsdiq bildirişi göndəririk
                if (!string.IsNullOrEmpty(profile.ApplicationUser?.Email))
                {
                    var subject = "AgroConnect - Müraciətiniz təsdiqləndi";
                    var body = $@"
                        <div style='font-family:Segoe UI,Arial,sans-serif;max-width:500px;margin:auto;'>
                            <h2 style='color:#2e7d32;'>Təbriklər!</h2>
                            <p>Hörmətli {profile.ApplicationUser.UserName},</p>
                            <p>AgroConnect platformasına fermer kimi müraciətiniz <strong>təsdiqləndi</strong>.</p>
                            <p>İndi hesabınıza daxil olub məhsullarınızı əlavə edə bilərsiniz.</p>
                            <br/>
                            <p style='color:#888;font-size:13px;'>Bu, avtomatik göndərilən mesajdır, cavablamayın.</p>
                        </div>";

                    await _emailSender.SendEmailAsync(profile.ApplicationUser.Email, subject, body);
                }
            }

            return RedirectToAction(nameof(FarmerRequests));
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Reject(int id)
        {
            var profile = await _context.FarmerProfiles
                .Include(f => f.ApplicationUser)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (profile != null)
            {
                profile.Status = ApplicationStatus.Rejected;
                _context.Entry(profile).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(profile.ApplicationUser?.Email))
                {
                    var subject = "AgroConnect - Müraciətiniz haqqında";
                    var body = $@"
                        <div style='font-family:Segoe UI,Arial,sans-serif;max-width:500px;margin:auto;'>
                            <h2 style='color:#c62828;'>Müraciətiniz nəzərdən keçirildi</h2>
                            <p>Hörmətli {profile.ApplicationUser.UserName},</p>
                            <p>Təəssüf ki, AgroConnect platformasına fermer kimi müraciətiniz hazırda <strong>təsdiqlənmədi</strong>.</p>
                            <p>Ətraflı məlumat üçün bizimlə əlaqə saxlaya bilərsiniz.</p>
                            <br/>
                            <p style='color:#888;font-size:13px;'>Bu, avtomatik göndərilən mesajdır, cavablamayın.</p>
                        </div>";

                    await _emailSender.SendEmailAsync(profile.ApplicationUser.Email, subject, body);
                }
            }

            return RedirectToAction(nameof(FarmerRequests));
        }

    }
}