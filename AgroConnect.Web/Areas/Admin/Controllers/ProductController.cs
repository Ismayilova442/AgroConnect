using AgroConnect.Domain.Entities;
using AgroConnect.Infrastructure.Common;
using AgroConnect.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Security.Claims;

namespace AgroConnect.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Product
        public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.FarmerProfile)
                    .ThenInclude(f => f!.ApplicationUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Title.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            query = query.OrderByDescending(p => p.CreatedAt);

            var pagedProducts = await PaginatedList<Product>.CreateAsync(query, page, PageSize);

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", categoryId);
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = categoryId;

            return View(pagedProducts);
        }

        // GET: /Admin/Product/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: /Admin/Product/Create
        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var farmerProfile = await _context.FarmerProfiles.FirstOrDefaultAsync(f => f.ApplicationUserId == userId);

            if (farmerProfile != null)
            {
                product.FarmerProfileId = farmerProfile.Id;
            }
            else
            {
                var anyFarmer = await _context.FarmerProfiles.FirstOrDefaultAsync();
                if (anyFarmer != null)
                {
                    product.FarmerProfileId = anyFarmer.Id;
                }
            }

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/products", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    product.ImageUrl = "/uploads/products/" + fileName;
                }

                product.CreatedAt = DateTime.Now;
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Məhsul uğurla əlavə edildi.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View(product);
        }

        // GET: /Admin/Product/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: /Admin/Product/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            var existing = await _context.Products.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existing.Title = product.Title;
                existing.Description = product.Description;
                existing.Price = product.Price;
                existing.StockQuantity = product.StockQuantity;
                existing.CategoryId = product.CategoryId;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/products", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    existing.ImageUrl = "/uploads/products/" + fileName;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Məhsul uğurla yeniləndi.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: /Admin/Product/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.OrderItems)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
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

        // ============================================
        // EXPORT
        // ============================================

        private async Task<List<Product>> GetFilteredProductsAsync(string? search, int? categoryId)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.FarmerProfile)
                    .ThenInclude(f => f!.ApplicationUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Title.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        // GET: /Admin/Product/ExportExcel
        public async Task<IActionResult> ExportExcel(string? search, int? categoryId)
        {
            var products = await GetFilteredProductsAsync(search, categoryId);

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Məhsullar");

            string[] headers = { "Ad", "Kateqoriya", "Qiymət (AZN)", "Stok", "Fermer", "Əlavə tarixi" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
            }
            var headerRow = ws.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#2e7d32");
            headerRow.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;

            int row = 2;
            foreach (var p in products)
            {
                ws.Cell(row, 1).Value = p.Title;
                ws.Cell(row, 2).Value = p.Category?.Name ?? "-";
                ws.Cell(row, 3).Value = p.Price;
                ws.Cell(row, 4).Value = p.StockQuantity;
                ws.Cell(row, 5).Value = p.FarmerProfile?.ApplicationUser?.Email ?? "-";
                ws.Cell(row, 6).Value = p.CreatedAt;
                ws.Cell(row, 6).Style.DateFormat.Format = "dd.MM.yyyy";
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var fileName = $"Mehsullar_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // GET: /Admin/Product/ExportPdf
        public async Task<IActionResult> ExportPdf(string? search, int? categoryId)
        {
            var products = await GetFilteredProductsAsync(search, categoryId);

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4.Landscape());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                    page.Header()
                        .Text("Məhsullar Hesabatı")
                        .FontSize(18).Bold().FontColor(QuestPDF.Helpers.Colors.Green.Darken2);

                    page.Content().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            string[] cols = { "Ad", "Kateqoriya", "Qiymət", "Stok", "Fermer", "Tarix" };
                            foreach (var c in cols)
                            {
                                header.Cell().Background(QuestPDF.Helpers.Colors.Green.Darken2)
                                    .Padding(5).Text(c).FontColor(QuestPDF.Helpers.Colors.White).Bold();
                            }
                        });

                        foreach (var p in products)
                        {
                            table.Cell().Padding(4).Text(p.Title);
                            table.Cell().Padding(4).Text(p.Category?.Name ?? "-");
                            table.Cell().Padding(4).Text($"{p.Price} AZN");
                            table.Cell().Padding(4).Text(p.StockQuantity.ToString());
                            table.Cell().Padding(4).Text(p.FarmerProfile?.ApplicationUser?.Email ?? "-");
                            table.Cell().Padding(4).Text(p.CreatedAt.ToString("dd.MM.yyyy"));
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Səhifə ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            var fileName = $"Mehsullar_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}