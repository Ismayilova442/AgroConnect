using AgroConnect.Domain.Entities;
using AgroConnect.Infrastructure.Data;
using AgroConnect.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroConnect.Web.Controllers
{
    public class ForumController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ForumController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var topics = await _context.ForumTopics
                .Include(t => t.ApplicationUser)
                .Include(t => t.Replies)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(topics);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(ForumTopicViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var topic = new ForumTopic
                {
                    Title = model.Title,
                    Content = model.Content,
                    ApplicationUserId = user!.Id,
                    CreatedAt = DateTime.Now
                };

                _context.ForumTopics.Add(topic);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var topic = await _context.ForumTopics
                .Include(t => t.ApplicationUser)
                .Include(t => t.Replies)
                    .ThenInclude(r => r.ApplicationUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (topic == null) return NotFound();

            // Səhifə baxışını artır
            topic.ViewsCount++;
            await _context.SaveChangesAsync();

            return View(topic);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Reply(ForumReplyViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var reply = new ForumReply
                {
                    Content = model.Content,
                    ForumTopicId = model.TopicId,
                    ApplicationUserId = user!.Id,
                    CreatedAt = DateTime.Now
                };

                _context.ForumReplies.Add(reply);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Details), new { id = model.TopicId });
            }

            return RedirectToAction(nameof(Details), new { id = model.TopicId });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> MarkBestAnswer(int replyId)
        {
            var reply = await _context.ForumReplies
                .Include(r => r.ForumTopic)
                .FirstOrDefaultAsync(r => r.Id == replyId);

            if (reply == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            // Yalnız mövzu sahibi və ya admin ən yaxşı cavabı seçə bilər
            if (reply.ForumTopic!.ApplicationUserId == user!.Id || await _userManager.IsInRoleAsync(user, "SuperAdmin"))
            {
                // Digər cavablardan "BestAnswer" statusunu götür
                var allReplies = await _context.ForumReplies
                    .Where(r => r.ForumTopicId == reply.ForumTopicId)
                    .ToListAsync();

                foreach (var r in allReplies)
                {
                    r.IsBestAnswer = false;
                }

                reply.IsBestAnswer = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = reply.ForumTopicId });
        }
    }
}
