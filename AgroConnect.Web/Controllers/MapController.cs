using AgroConnect.Domain.Entities;
using AgroConnect.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroConnect.Web.Controllers
{
    public class MapController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MapController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Yalnız qəbul olunmuş fermerləri gətiririk
            var farmers = await _context.FarmerProfiles
                .Include(f => f.ApplicationUser)
                .Where(f => f.Status == ApplicationStatus.Approved)
                .ToListAsync();

            return View(farmers);
        }
    }
}
