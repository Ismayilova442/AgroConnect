using AgroConnect.Domain.Entities;
using AgroConnect.Infrastructure.Data;
using AgroConnect.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace AgroConnect.Web.Controllers
{
    [Authorize(Roles = "Farmer")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ProductController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var farmerProfile = await _context.FarmerProfiles.FirstOrDefaultAsync(f => f.ApplicationUserId == user.Id);
            if (farmerProfile == null) return Challenge();

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.FarmerProfileId == farmerProfile.Id)
                .ToListAsync();

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View(new ProductViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var farmerProfile = await _context.FarmerProfiles.FirstOrDefaultAsync(f => f.ApplicationUserId == user!.Id);
                
                string imageUrl = "";
                if (model.Image != null)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsFolder);
                    string fileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                    string filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Image.CopyToAsync(stream);
                    }
                    imageUrl = "/uploads/" + fileName;
                }

                var product = new Product
                {
                    Title = model.Title,
                    Description = model.Description,
                    Price = model.Price,
                    StockQuantity = model.StockQuantity,
                    CategoryId = model.CategoryId,
                    FarmerProfileId = farmerProfile!.Id,
                    ImageUrl = imageUrl
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", model.CategoryId);
            return View(model);
        }
    }
}
