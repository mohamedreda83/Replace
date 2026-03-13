using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartRecycle.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SmartRecycle.Controllers
{
    [Authorize(Roles = "admin")]
    public class OrdersController : Controller
    {
        private readonly SmartRecycleContext _context;
        public int userid { get; set; }
        public string userids { get; set; }

        public OrdersController(SmartRecycleContext context)
        {
            _context = context;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (TempData["Id"] != null)
            {
                userids = TempData["Id"].ToString();
                TempData["Id"] = userids;

                if (int.TryParse(userids, out int parsedId))
                {
                    userid = parsedId;
                }
                else
                {
                    userid = 0; // أو أي قيمة افتراضية حسب اختيارك
                }
            }

            base.OnActionExecuting(filterContext);
        }
        public void l()
        {
            var user = _context.Users.FirstOrDefault(a => a.Id == userid);
            ViewBag.UserName = user.Username;
            ViewBag.UserPoints = user.Points;
        }
        // GET: Orders
        public async Task<IActionResult> Index()
        {
            l();
            var orders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            l();
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            l();
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.Branches = new List<SelectListItem>
    {
        new SelectListItem { Value = "1", Text = "سموحة" },
        new SelectListItem { Value = "2", Text = "سيدي جابر" },
        new SelectListItem { Value = "3", Text = "النصر" },
        new SelectListItem { Value = "4", Text = "جاردينيا" }
    };

            return View(order);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string[] selectedBranches)
        {
            l();
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            if (selectedBranches != null && selectedBranches.Length > 0)
            {
                // تحويل أرقام الفروع إلى أسماء
                var branchNames = selectedBranches.Select(branchId => branchId switch
                {
                    "1" => "سموحة",
                    "2" => "سيدي جابر",
                    "3" => "النصر",
                    "4" => "جاردينيا",
                    _ => ""
                }).Where(name => !string.IsNullOrEmpty(name)).ToList();

                if (branchNames.Any())
                {
                    // حفظ الفروع المتاحة مفصولة بفاصلة
                    order.AvailableBranches = string.Join(",", branchNames);
                    order.Status = "متاح للاستلام - يرجى اختيار الفرع";

                    try
                    {
                        _context.Update(order);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!OrderExists(order.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }

                    return RedirectToAction(nameof(Index));
                }
            }

            // If we get here, something failed, redisplay form
            ViewBag.Branches = new List<SelectListItem>
    {
        new SelectListItem { Value = "1", Text = "سموحة" },
        new SelectListItem { Value = "2", Text = "سيدي جابر" },
        new SelectListItem { Value = "3", Text = "النصر" },
        new SelectListItem { Value = "4", Text = "جاردينيا" }
    };

            // Re-include the order items for the view
            order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            return View(order);
        }
        public async Task<IActionResult> BranchOrders()
        {
            l();

            ViewBag.Branches = new List<SelectListItem>
    {
        new SelectListItem { Value = "", Text = "اختر الفرع" },
        new SelectListItem { Value = "سموحة", Text = "سموحة" },
        new SelectListItem { Value = "سيدي جابر", Text = "سيدي جابر" },
        new SelectListItem { Value = "النصر", Text = "النصر" },
        new SelectListItem { Value = "جاردينيا", Text = "جاردينيا" }
    };

            return View();
        }

        // POST: جلب طلبات الفرع المحدد
        [HttpPost]
        public async Task<IActionResult> BranchOrders(string selectedBranch)
        {
            l();

            if (string.IsNullOrEmpty(selectedBranch))
            {
                TempData["ErrorMessage"] = "يرجى اختيار الفرع";
                return RedirectToAction("BranchOrders");
            }

            var orders = await _context.Orders
                .Include(o => o.User)
                .Where(o => o.SelectedBranch == selectedBranch &&
                           (o.Status.Contains("يرجى التوجه لفرع") || o.Status == "مكتمل"))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.SelectedBranch = selectedBranch;
            ViewBag.Branches = new List<SelectListItem>
    {
        new SelectListItem { Value = "", Text = "اختر الفرع" },
        new SelectListItem { Value = "سموحة", Text = "سموحة", Selected = selectedBranch == "سموحة" },
        new SelectListItem { Value = "سيدي جابر", Text = "سيدي جابر", Selected = selectedBranch == "سيدي جابر" },
        new SelectListItem { Value = "النصر", Text = "النصر", Selected = selectedBranch == "النصر" },
        new SelectListItem { Value = "جاردينيا", Text = "جاردينيا", Selected = selectedBranch == "جاردينيا" }
    };

            return View(orders);
        }

        // GET: البحث بكود الطلب
        public IActionResult SearchOrder()
        {
            l();
            return View();
        }

        // POST: البحث بكود الطلب
        [HttpPost]
        public async Task<IActionResult> SearchOrder(string orderCode)
        {
            l();

            if (string.IsNullOrEmpty(orderCode) || !int.TryParse(orderCode, out int orderId))
            {
                TempData["ErrorMessage"] = "كود الطلب غير صحيح";
                return View();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "الطلب غير موجود";
                return View();
            }

            return View("OrderFound", order);
        }

        // POST: تسليم الطلب
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOrder(int orderId)
        {
            l();

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "الطلب غير موجود";
                return RedirectToAction("SearchOrder");
            }

            if (order.Status == "مكتمل")
            {
                TempData["ErrorMessage"] = "هذا الطلب مكتمل بالفعل";
                return RedirectToAction("SearchOrder");
            }

            try
            {
                // إنشاء الفاتورة
                await CreateInvoicePDF(order);

                // تحديث حالة الطلب
                order.Status = "مكتمل";
                _context.Update(order);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم تسليم الطلب بنجاح وإنشاء الفاتورة";
                return RedirectToAction("SearchOrder");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء تسليم الطلب";
                return RedirectToAction("SearchOrder");
            }
        }

        // POST: إلغاء الطلب
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrderAdmin(int orderId)
        {
            l();

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "الطلب غير موجود";
                return RedirectToAction("SearchOrder");
            }

            if (order.Status == "ملغي")
            {
                TempData["ErrorMessage"] = "هذا الطلب ملغي بالفعل";
                return RedirectToAction("SearchOrder");
            }

            try
            {
                // إرجاع النقاط للمستخدم
                order.User.Points += order.TotalPoints;
                order.Status = "ملغي";

                _context.Update(order);
                _context.Update(order.User);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"تم إلغاء الطلب وإرجاع {order.TotalPoints} نقطة للمستخدم";
                return RedirectToAction("SearchOrder");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء إلغاء الطلب";
                return RedirectToAction("SearchOrder");
            }
        }

        // إنشاء فاتورة PDF
        // إنشاء فاتورة PDF
        private async Task CreateInvoicePDF(Order order)
        {
            var invoicesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "invoices");
            if (!Directory.Exists(invoicesPath))
                Directory.CreateDirectory(invoicesPath);

            var userInvoicesPath = Path.Combine(invoicesPath, order.UserId.ToString());
            if (!Directory.Exists(userInvoicesPath))
                Directory.CreateDirectory(userInvoicesPath);

            var fileName = $"Invoice_{order.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(userInvoicesPath, fileName);

            // إنشاء PDF
            using (var document = new Document(PageSize.A4))
            {
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
                document.Open();

                // إضافة خط يدعم العربية
                BaseFont bf = BaseFont.CreateFont("c:/windows/fonts/arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                Font arabicFont = new Font(bf, 12);
                Font arabicFontBold = new Font(bf, 14, Font.BOLD);

                // العنوان - استخدام جدول لدعم الاتجاه
                PdfPTable titleTable = new PdfPTable(1);
                titleTable.WidthPercentage = 100;
                titleTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                PdfPCell titleCell = new PdfPCell(new Phrase("RePlace - فاتورة الطلب", arabicFontBold));
                titleCell.HorizontalAlignment = Element.ALIGN_CENTER;
                titleCell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                titleCell.Border = PdfPCell.NO_BORDER;
                titleTable.AddCell(titleCell);

                document.Add(titleTable);
                document.Add(new Paragraph(" ")); // مسافة

                // معلومات الطلب - استخدام جدول
                PdfPTable infoTable = new PdfPTable(1);
                infoTable.WidthPercentage = 100;
                infoTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                string orderInfoText = $"رقم الطلب: {order.Id}\nاسم العميل: {order.User.Username}\nتاريخ الطلب: {order.OrderDate:yyyy/MM/dd}\nالفرع: {order.SelectedBranch}";
                PdfPCell infoCell = new PdfPCell(new Phrase(orderInfoText, arabicFont));
                infoCell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                infoCell.Border = PdfPCell.NO_BORDER;
                infoTable.AddCell(infoCell);

                document.Add(infoTable);
                document.Add(new Paragraph(" ")); // مسافة

                // جدول المنتجات
                PdfPTable table = new PdfPTable(4);
                table.WidthPercentage = 100;
                table.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                // رؤوس الجدول
                table.AddCell(new PdfPCell(new Phrase("المنتج", arabicFontBold)) { RunDirection = PdfWriter.RUN_DIRECTION_RTL });
                table.AddCell(new PdfPCell(new Phrase("الكمية", arabicFontBold)) { RunDirection = PdfWriter.RUN_DIRECTION_RTL });
                table.AddCell(new PdfPCell(new Phrase("النقاط لكل قطعة", arabicFontBold)) { RunDirection = PdfWriter.RUN_DIRECTION_RTL });
                table.AddCell(new PdfPCell(new Phrase("إجمالي النقاط", arabicFontBold)) { RunDirection = PdfWriter.RUN_DIRECTION_RTL });

                // بيانات المنتجات
                foreach (var item in order.OrderItems)
                {
                    table.AddCell(new PdfPCell(new Phrase(item.Product.Name, arabicFont)) { RunDirection = PdfWriter.RUN_DIRECTION_RTL });
                    table.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString(), arabicFont)) { RunDirection = PdfWriter.RUN_DIRECTION_RTL });
                    table.AddCell(new PdfPCell(new Phrase(item.PointsPerItem.ToString(), arabicFont)) { RunDirection = PdfWriter.RUN_DIRECTION_RTL });
                    table.AddCell(new PdfPCell(new Phrase((item.Quantity * item.PointsPerItem).ToString(), arabicFont)) { RunDirection = PdfWriter.RUN_DIRECTION_RTL });
                }

                document.Add(table);

                // الإجمالي - استخدام جدول
                PdfPTable totalTable = new PdfPTable(1);
                totalTable.WidthPercentage = 100;
                totalTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                PdfPCell totalCell = new PdfPCell(new Phrase($"إجمالي النقاط: {order.TotalPoints}", arabicFontBold));
                totalCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                totalCell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                totalCell.Border = PdfPCell.NO_BORDER;
                totalTable.AddCell(totalCell);

                document.Add(totalTable);

                document.Close();
            }

            // حفظ معلومات الفاتورة في قاعدة البيانات
            var invoice = new Invoice
            {
                OrderId = order.Id,
                UserId = order.UserId,
                InvoiceNumber = $"INV-{order.Id}-{DateTime.Now:yyyyMMdd}",
                FilePath = $"/invoices/{order.UserId}/{fileName}",
                FileName = fileName,
                TotalAmount = 0,
                TotalPoints = order.TotalPoints
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
        }

        // إنشاء HTML للفاتورة
        private string GenerateInvoiceHTML(Order order)
        {
            var html = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8'>
        <title>فاتورة رقم {order.Id}</title>
        <style>
            body {{ font-family: Arial, sans-serif; direction: rtl; }}
            .header {{ text-align: center; margin-bottom: 30px; }}
            .order-info {{ margin-bottom: 20px; }}
            table {{ width: 100%; border-collapse: collapse; }}
            th, td {{ border: 1px solid #ddd; padding: 8px; text-align: right; }}
            th {{ background-color: #f2f2f2; }}
            .total {{ font-weight: bold; font-size: 18px; }}
        </style>
    </head>
    <body>
        <div class='header'>
            <h1>RePlace</h1>
            <h2>فاتورة الطلب</h2>
        </div>
        
        <div class='order-info'>
            <p><strong>رقم الطلب:</strong> {order.Id}</p>
            <p><strong>اسم العميل:</strong> {order.User.Username}</p>
            <p><strong>تاريخ الطلب:</strong> {order.OrderDate:yyyy/MM/dd}</p>
            <p><strong>الفرع:</strong> {order.SelectedBranch}</p>
        </div>
        
        <table>
            <thead>
                <tr>
                    <th>المنتج</th>
                    <th>الكمية</th>
                    <th>النقاط لكل قطعة</th>
                    <th>إجمالي النقاط</th>
                </tr>
            </thead>
            <tbody>";

            foreach (var item in order.OrderItems)
            {
                html += $@"
                <tr>
                    <td>{item.Product.Name}</td>
                    <td>{item.Quantity}</td>
                    <td>{item.PointsPerItem}</td>
                    <td>{(item.Quantity * item.PointsPerItem)}</td>
                </tr>";
            }

            html += $@"
            </tbody>
            <tfoot>
                <tr class='total'>
                    <td colspan='3'>إجمالي النقاط</td>
                    <td>{order.TotalPoints}</td>
                </tr>
            </tfoot>
        </table>
        
        <div style='margin-top: 30px; text-align: center;'>
            <p>شكراً لاستخدامكم RePlace</p>
        </div>
    </body>
    </html>";

            return html;
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}