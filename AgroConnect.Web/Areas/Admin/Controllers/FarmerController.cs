using AgroConnect.Domain.Entities;
using AgroConnect.Infrastructure.Common;
using AgroConnect.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace AgroConnect.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")]
    public class FarmerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const int PageSize = 10;

        public FarmerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<List<FarmerListItemViewModel>> GetFilteredFarmersAsync(string? search)
        {
            var query = _context.FarmerProfiles
                .Include(f => f.ApplicationUser)
                .Where(f => f.Status == ApplicationStatus.Approved)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(f =>
                    (f.ApplicationUser != null && f.ApplicationUser.UserName != null && f.ApplicationUser.UserName.Contains(search)) ||
                    (f.ApplicationUser != null && f.ApplicationUser.Email != null && f.ApplicationUser.Email.Contains(search)) ||
                    (f.District != null && f.District.Contains(search)) ||
                    (f.Village != null && f.Village.Contains(search)));
            }

            var farmers = await query.ToListAsync();

            var vmList = new List<FarmerListItemViewModel>();
            foreach (var f in farmers)
            {
                var productCount = await _context.Products.CountAsync(p => p.FarmerProfileId == f.Id);
                vmList.Add(new FarmerListItemViewModel
                {
                    Id = f.Id,
                    UserName = f.ApplicationUser?.UserName ?? "Ad daxil edilməyib",
                    Email = f.ApplicationUser?.Email ?? "",
                    District = f.District ?? "",
                    Village = f.Village ?? "",
                    FarmType = f.FarmType ?? "",
                    ProductCount = productCount
                });
            }

            return vmList.OrderBy(f => f.UserName).ToList();
        }

        // GET: /Admin/Farmer
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var allFiltered = await GetFilteredFarmersAsync(search);
            var paged = PaginatedList<FarmerListItemViewModel>.Create(allFiltered, page, PageSize);

            ViewBag.CurrentSearch = search;
            return View(paged);
        }

        // GET: /Admin/Farmer/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var farmer = await _context.FarmerProfiles
                .Include(f => f.ApplicationUser)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (farmer == null)
            {
                return NotFound();
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.FarmerProfileId == id)
                .ToListAsync();

            ViewBag.Products = products;
            return View(farmer);
        }

        // POST: /Admin/Farmer/Block/5
        // Fermerin fəaliyyətini dayandırır: statusu "Rejected" edir, Farmer rolunu geri alır
        [HttpPost]
        public async Task<IActionResult> Block(int id)
        {
            var farmer = await _context.FarmerProfiles
                .Include(f => f.ApplicationUser)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (farmer == null)
            {
                return NotFound();
            }

            farmer.Status = ApplicationStatus.Rejected;

            try
            {
                if (farmer.ApplicationUser != null)
                {
                    var isInFarmerRole = await _userManager.IsInRoleAsync(farmer.ApplicationUser, "Farmer");
                    if (isInFarmerRole)
                    {
                        await _userManager.RemoveFromRoleAsync(farmer.ApplicationUser, "Farmer");
                    }

                    var isInMemberRole = await _userManager.IsInRoleAsync(farmer.ApplicationUser, "Member");
                    if (!isInMemberRole)
                    {
                        await _userManager.AddToRoleAsync(farmer.ApplicationUser, "Member");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Rol dəyişdirilmə xətası: " + ex.Message);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Fermerin fəaliyyəti dayandırıldı.";
            return RedirectToAction(nameof(Index));
        }

        // ============================================
        // EXPORT
        // ============================================

        // GET: /Admin/Farmer/ExportExcel
        public async Task<IActionResult> ExportExcel(string? search)
        {
            var farmers = await GetFilteredFarmersAsync(search);

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Fermerlər");

            string[] headers = { "Ad", "Email", "Rayon", "Kənd", "Fəaliyyət növü", "Məhsul sayı" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
            }
            var headerRow = ws.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#2e7d32");
            headerRow.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;

            int row = 2;
            foreach (var f in farmers)
            {
                ws.Cell(row, 1).Value = f.UserName;
                ws.Cell(row, 2).Value = f.Email;
                ws.Cell(row, 3).Value = f.District;
                ws.Cell(row, 4).Value = f.Village;
                ws.Cell(row, 5).Value = f.FarmType;
                ws.Cell(row, 6).Value = f.ProductCount;
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var fileName = $"Fermerler_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // GET: /Admin/Farmer/ExportPdf
        public async Task<IActionResult> ExportPdf(string? search)
        {
            var farmers = await GetFilteredFarmersAsync(search);

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4.Landscape());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                    page.Header()
                        .Text("Fermerlər Hesabatı")
                        .FontSize(18).Bold().FontColor(QuestPDF.Helpers.Colors.Green.Darken2);

                    page.Content().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2.5f);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            string[] cols = { "Ad", "Email", "Rayon", "Kənd", "Fəaliyyət növü", "Məhsul sayı" };
                            foreach (var c in cols)
                            {
                                header.Cell().Background(QuestPDF.Helpers.Colors.Green.Darken2)
                                    .Padding(5).Text(c).FontColor(QuestPDF.Helpers.Colors.White).Bold();
                            }
                        });

                        foreach (var f in farmers)
                        {
                            table.Cell().Padding(4).Text(f.UserName);
                            table.Cell().Padding(4).Text(f.Email);
                            table.Cell().Padding(4).Text(f.District);
                            table.Cell().Padding(4).Text(f.Village);
                            table.Cell().Padding(4).Text(f.FarmType);
                            table.Cell().Padding(4).Text(f.ProductCount.ToString());
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
            var fileName = $"Fermerler_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
    }

    public class FarmerListItemViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Village { get; set; } = string.Empty;
        public string FarmType { get; set; } = string.Empty;
        public int ProductCount { get; set; }
    }
}