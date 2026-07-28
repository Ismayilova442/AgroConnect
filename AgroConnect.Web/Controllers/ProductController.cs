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

        // GET: /Product/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var farmerProfile = await _context.FarmerProfiles.FirstOrDefaultAsync(f => f.ApplicationUserId == user.Id);
            if (farmerProfile == null) return Challenge();

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.FarmerProfileId == farmerProfile.Id);

            if (product == null)
            {
                TempData["Error"] = "Məhsul tapılmadı və ya sizə aid deyil.";
                return RedirectToAction(nameof(Index));
            }

            var model = new ProductViewModel
            {
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId
            };

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            ViewBag.ProductId = product.Id;
            ViewBag.CurrentImageUrl = product.ImageUrl;
            return View(model);
        }

        // POST: /Product/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProductViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var farmerProfile = await _context.FarmerProfiles.FirstOrDefaultAsync(f => f.ApplicationUserId == user.Id);
            if (farmerProfile == null) return Challenge();

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.FarmerProfileId == farmerProfile.Id);

            if (product == null)
            {
                TempData["Error"] = "Məhsul tapılmadı və ya sizə aid deyil.";
                return RedirectToAction(nameof(Index));
            }

            // Şəkil redaktədə məcburi deyil (boş buraxılsa köhnə şəkil qalır)
            ModelState.Remove(nameof(model.Image));

            if (ModelState.IsValid)
            {
                product.Title = model.Title;
                product.Description = model.Description;
                product.Price = model.Price;
                product.StockQuantity = model.StockQuantity;
                product.CategoryId = model.CategoryId;

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
                    product.ImageUrl = "/uploads/" + fileName;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Məhsul uğurla yeniləndi.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", model.CategoryId);
            ViewBag.ProductId = id;
            ViewBag.CurrentImageUrl = product.ImageUrl;
            return View(model);
        }

        // POST: /Product/Delete/5
        // Fermer öz məhsulunu (yalnız özününkü) admin panelə girmədən silə bilir
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var farmerProfile = await _context.FarmerProfiles.FirstOrDefaultAsync(f => f.ApplicationUserId == user.Id);
            if (farmerProfile == null) return Challenge();

            // Yalnız CARİ fermerə aid məhsulu tap - başqasının məhsulunu silə bilməsin
            var product = await _context.Products
                .Include(p => p.OrderItems)
                .FirstOrDefaultAsync(p => p.Id == id && p.FarmerProfileId == farmerProfile.Id);

            if (product == null)
            {
                TempData["Error"] = "Məhsul tapılmadı və ya sizə aid deyil.";
                return RedirectToAction(nameof(Index));
            }

            if (product.OrderItems.Any())
            {
                TempData["Error"] = "Bu məhsul sifarişlərdə istifadə olunduğu üçün silinə bilməz.";
                return RedirectToAction(nameof(Index));
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Məhsul silindi.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Product/UpdateStock/5
        // Fermer öz məhsulunun stokunu birbaşa bu səhifədən artırıb-azalda bilir
        [HttpPost]
        public async Task<IActionResult> UpdateStock(int id, int change)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var farmerProfile = await _context.FarmerProfiles.FirstOrDefaultAsync(f => f.ApplicationUserId == user.Id);
            if (farmerProfile == null) return Challenge();

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.FarmerProfileId == farmerProfile.Id);

            if (product == null)
            {
                TempData["Error"] = "Məhsul tapılmadı və ya sizə aid deyil.";
                return RedirectToAction(nameof(Index));
            }

            product.StockQuantity += change;
            if (product.StockQuantity < 0)
            {
                product.StockQuantity = 0;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}