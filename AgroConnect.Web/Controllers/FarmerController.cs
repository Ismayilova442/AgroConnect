using AgroConnect.Domain.Entities;
using AgroConnect.Infrastructure.Data;
using AgroConnect.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace AgroConnect.Web.Controllers
{
    [Authorize]
    public class FarmerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public FarmerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Apply()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var existingProfile = await _context.FarmerProfiles.FirstOrDefaultAsync(f => f.ApplicationUserId == user.Id);
            if (existingProfile != null)
            {
                if (existingProfile.Status == ApplicationStatus.Approved)
                {
                    ViewBag.Message = "Siz artıq təsdiqlənmiş fermersiniz!";
                    return View("ApplicationStatus");
                }
                else if (existingProfile.Status == ApplicationStatus.Pending)
                {
                    ViewBag.Message = "Sizin müraciətiniz baxılmaqdadır. Zəhmət olmasa adminin təsdiqini gözləyin.";
                    return View("ApplicationStatus");
                }
                else
                {
                    ViewBag.Message = "Müraciətiniz rədd edilib. Yenidən müraciət etmək üçün adminlə əlaqə saxlayın.";
                    return View("ApplicationStatus");
                }
            }

            return View(new FarmerApplicationViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Apply(FarmerApplicationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var existingProfile = await _context.FarmerProfiles.FirstOrDefaultAsync(f => f.ApplicationUserId == user.Id);
            if (existingProfile != null)
            {
                return RedirectToAction(nameof(Apply));
            }

            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            string idCardFileName = Guid.NewGuid().ToString() + "_" + model.IDCardImage!.FileName;
            string idCardFilePath = Path.Combine(uploadsFolder, idCardFileName);
            using (var stream = new FileStream(idCardFilePath, FileMode.Create))
            {
                await model.IDCardImage.CopyToAsync(stream);
            }

            string farmFileName = Guid.NewGuid().ToString() + "_" + model.FarmImage!.FileName;
            string farmFilePath = Path.Combine(uploadsFolder, farmFileName);
            using (var stream = new FileStream(farmFilePath, FileMode.Create))
            {
                await model.FarmImage.CopyToAsync(stream);
            }

            var profile = new FarmerProfile
            {
                ApplicationUserId = user.Id,
                District = model.District,
                Village = model.Village,
                FarmType = model.FarmType,
                ExperienceYears = model.ExperienceYears,
                About = model.About,
                IDCardImagePath = "/uploads/" + idCardFileName,
                FarmImagePath = "/uploads/" + farmFileName,
                Status = ApplicationStatus.Pending
            };

            _context.FarmerProfiles.Add(profile);
            await _context.SaveChangesAsync();

            ViewBag.Message = "Müraciətiniz uğurla göndərildi. Təsdiq edildikdən sonra sizə bildiriş gələcək.";
            return View("ApplicationStatus");
        }
    }
}
