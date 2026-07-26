using AgroConnect.Domain.Entities;
using AgroConnect.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace AgroConnect.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Order
        public async Task<IActionResult> Index(OrderStatus? status)
        {
            var query = _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.SelectedStatus = status;
            return View(orders);
        }

        // GET: /Admin/Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: /Admin/Order/UpdateStatus/5
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus newStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = newStatus;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Sifariş #{order.Id} statusu \"{newStatus}\" olaraq yeniləndi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ============================================
        // EXPORT
        // ============================================

        private async Task<List<Order>> GetFilteredOrdersAsync(OrderStatus? status)
        {
            var query = _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        }

        // GET: /Admin/Order/ExportExcel
        public async Task<IActionResult> ExportExcel(OrderStatus? status)
        {
            var orders = await GetFilteredOrdersAsync(status);

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Sifarişlər");

            string[] headers = { "#", "Müştəri", "Tarix", "Məhsul sayı", "Ümumi məbləğ", "Status", "Ünvan", "Əlaqə" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
            }
            var headerRow = ws.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#2e7d32");
            headerRow.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;

            int row = 2;
            foreach (var o in orders)
            {
                ws.Cell(row, 1).Value = o.Id;
                ws.Cell(row, 2).Value = o.ApplicationUser?.Email ?? "-";
                ws.Cell(row, 3).Value = o.OrderDate;
                ws.Cell(row, 3).Style.DateFormat.Format = "dd.MM.yyyy HH:mm";
                ws.Cell(row, 4).Value = o.OrderItems.Count;
                ws.Cell(row, 5).Value = o.TotalAmount;
                ws.Cell(row, 6).Value = o.Status.ToString();
                ws.Cell(row, 7).Value = o.ShippingAddress;
                ws.Cell(row, 8).Value = o.ContactNumber;
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var fileName = $"Sifarisler_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // GET: /Admin/Order/ExportPdf
        public async Task<IActionResult> ExportPdf(OrderStatus? status)
        {
            var orders = await GetFilteredOrdersAsync(status);

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4.Landscape());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                    page.Header()
                        .Text("Sifarişlər Hesabatı")
                        .FontSize(18).Bold().FontColor(QuestPDF.Helpers.Colors.Green.Darken2);

                    page.Content().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            string[] cols = { "#", "Müştəri", "Tarix", "Say", "Məbləğ", "Status" };
                            foreach (var c in cols)
                            {
                                header.Cell().Background(QuestPDF.Helpers.Colors.Green.Darken2)
                                    .Padding(5).Text(c).FontColor(QuestPDF.Helpers.Colors.White).Bold();
                            }
                        });

                        foreach (var o in orders)
                        {
                            table.Cell().Padding(4).Text($"#{o.Id}");
                            table.Cell().Padding(4).Text(o.ApplicationUser?.Email ?? "-");
                            table.Cell().Padding(4).Text(o.OrderDate.ToString("dd.MM.yyyy"));
                            table.Cell().Padding(4).Text(o.OrderItems.Count.ToString());
                            table.Cell().Padding(4).Text($"{o.TotalAmount} AZN");
                            table.Cell().Padding(4).Text(o.Status.ToString());
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
            var fileName = $"Sifarisler_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}