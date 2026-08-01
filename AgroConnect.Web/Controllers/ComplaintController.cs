using AgroConnect.Domain.Entities;
using AgroConnect.Infrastructure.Data;
using AgroConnect.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroConnect.Web.Controllers
{
    public class ComplaintController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ComplaintController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Hər kəs (qonaqlar da daxil) bütün şikayətlərə baxa bilər
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Complaints
                .Include(c => c.ApplicationUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.ApplicationUser != null &&
                    c.ApplicationUser.UserName != null &&
                    c.ApplicationUser.UserName.Contains(search));
            }

            var complaints = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.CurrentSearch = search;
            return View(complaints);
        }

        // Yalnız qeydiyyatdan keçmiş (daxil olmuş) istifadəçilər şikayət yaza bilər
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new ComplaintViewModel());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(ComplaintViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var complaint = new Complaint
            {
                Subject = model.Subject,
                Content = model.Content,
                ApplicationUserId = user.Id,
                CreatedAt = DateTime.Now
            };

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Şikayətiniz uğurla göndərildi.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Complaint/Edit/5
        // Yalnız şikayəti yazan şəxs özününkünü redaktə edə bilər
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.Id == id);
            if (complaint == null)
            {
                return NotFound();
            }

            if (complaint.ApplicationUserId != user.Id)
            {
                TempData["Error"] = "Yalnız öz şikayətinizi redaktə edə bilərsiniz.";
                return RedirectToAction(nameof(Index));
            }

            var model = new ComplaintViewModel
            {
                Subject = complaint.Subject,
                Content = complaint.Content
            };

            ViewBag.ComplaintId = complaint.Id;
            return View(model);
        }

        // POST: /Complaint/Edit/5
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, ComplaintViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.Id == id);
            if (complaint == null)
            {
                return NotFound();
            }

            if (complaint.ApplicationUserId != user.Id)
            {
                TempData["Error"] = "Yalnız öz şikayətinizi redaktə edə bilərsiniz.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ComplaintId = id;
                return View(model);
            }

            complaint.Subject = model.Subject;
            complaint.Content = model.Content;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Şikayətiniz yeniləndi.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Complaint/Delete/5
        // Yalnız şikayəti yazan şəxs özününkünü silə bilər
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.Id == id);
            if (complaint == null)
            {
                return NotFound();
            }

            if (complaint.ApplicationUserId != user.Id)
            {
                TempData["Error"] = "Yalnız öz şikayətinizi silə bilərsiniz.";
                return RedirectToAction(nameof(Index));
            }

            _context.Complaints.Remove(complaint);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Şikayət silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}