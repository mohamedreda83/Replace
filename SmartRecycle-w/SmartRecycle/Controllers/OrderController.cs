using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartRecycle.Models;
using SmartRecycle.Repositories;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartRecycle.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        public int userId { get; set; }
        public string userids { get; set; }
        private readonly SmartRecycleContext _context;

        public OrderController(IOrderRepository orderRepository, SmartRecycleContext context)
        {
            _orderRepository = orderRepository;
            _context = context;
        }

        // ✅ Method محسنة لجلب UserId من أي مصدر متاح
        private int GetCurrentUserId()
        {
            // 1. من Claims (الأفضل)
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int claimUserId))
                {
                    return claimUserId;
                }
            }

            // 2. من Session
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue)
            {
                return sessionUserId.Value;
            }

            // 3. من TempData (آخر خيار)
            if (TempData["Id"] != null)
            {
                TempData.Keep("Id");
                if (int.TryParse(TempData["Id"].ToString(), out int tempUserId))
                {
                    return tempUserId;
                }
            }

            return 0;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            userId = GetCurrentUserId();
            base.OnActionExecuting(filterContext);
        }

        public void l()
        {
            var user = _context.Users.FirstOrDefault(a => a.Id == userId);
            if (user != null)
            {
                ViewBag.UserName = user.Username;
                ViewBag.UserPoints = user.Points;
            }
        }

        // GET: /Order
        public async Task<IActionResult> Index()
        {
            l();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _orderRepository.GetUserOrdersAsync(userId);
            return View(orders);
        }

        // GET: /Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            l();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderRepository.GetOrderDetailsAsync(id, userId);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: /Order/Checkout
        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            l();

            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var order = await _orderRepository.CreateOrderFromCartAsync(userId);
                TempData["SuccessMessage"] = "تم إنشاء الطلب بنجاح";
                return RedirectToAction("Details", new { id = order.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Cart", "Shop");
            }
        }

        // POST: /Order/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            l();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var order = await _context.Orders
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "الطلب غير موجود";
                    return RedirectToAction("Index");
                }

                // التحقق من إمكانية إلغاء الطلب
                if (order.Status == "ملغي")
                {
                    TempData["ErrorMessage"] = "هذا الطلب ملغي بالفعل";
                    return RedirectToAction("Details", new { id = id });
                }

                if (order.Status == "مكتمل")
                {
                    TempData["ErrorMessage"] = "لا يمكن إلغاء طلب مكتمل";
                    return RedirectToAction("Details", new { id = id });
                }

                // إرجاع النقاط للمستخدم
                var user = order.User;
                user.Points += order.TotalPoints;

                // تحديث حالة الطلب
                order.Status = "ملغي";

                _context.Update(order);
                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"تم إلغاء الطلب بنجاح وإرجاع {order.TotalPoints} نقطة لحسابك";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء إلغاء الطلب";
                return RedirectToAction("Details", new { id = id });
            }
        }

        // GET: /Order/SelectBranch/5
        public async Task<IActionResult> SelectBranch(int id)
        {
            l();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            // التحقق من أن الطلب في حالة متاح للاستلام
            if (string.IsNullOrEmpty(order.AvailableBranches))
            {
                TempData["ErrorMessage"] = "لا توجد فروع متاحة لهذا الطلب حالياً";
                return RedirectToAction("Details", new { id = id });
            }

            // تحويل الفروع المتاحة إلى قائمة
            var availableBranches = order.AvailableBranches.Split(',')
                .Select(branch => new SelectListItem
                {
                    Value = branch.Trim(),
                    Text = branch.Trim()
                }).ToList();

            ViewBag.AvailableBranches = availableBranches;
            ViewBag.OrderId = id;

            return View(order);
        }

        // POST: /Order/SelectBranch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectBranch(int orderId, string selectedBranch)
        {
            l();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(selectedBranch))
            {
                order.SelectedBranch = selectedBranch;
                order.Status = $"يرجى التوجه لفرع {selectedBranch} بكود الطلب لاستلام الطلب";

                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"تم تأكيد اختيار فرع {selectedBranch} بنجاح";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "حدث خطأ أثناء حفظ اختيارك";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "يرجى اختيار فرع";
                return RedirectToAction("SelectBranch", new { id = orderId });
            }

            return RedirectToAction("Details", new { id = orderId });
        }

        // GET: عرض فواتير المستخدم
        public async Task<IActionResult> MyInvoices()
        {
            l();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var invoices = await _context.Invoices
              .Include(i => i.Order)
              .Where(i => i.UserId == userId)
              .OrderByDescending(i => i.CreatedDate)
              .ToListAsync();
            return View(invoices);
        }

        // GET: عرض الفاتورة
        public async Task<IActionResult> ViewInvoice(int id)
        {
            l();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var invoice = await _context.Invoices
                .Include(i => i.Order)
                .ThenInclude(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        // ✅ GET: تحميل الفاتورة - محسنة بالكامل
        public async Task<IActionResult> DownloadInvoice(int id)
        {
            int currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var invoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.Id == id && i.UserId == currentUserId);

                if (invoice == null)
                {
                    TempData["ErrorMessage"] = "الفاتورة غير موجودة أو ليس لديك صلاحية للوصول إليها";
                    return RedirectToAction("MyInvoices");
                }

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", invoice.FilePath.TrimStart('/'));

                if (!System.IO.File.Exists(filePath))
                {
                    TempData["ErrorMessage"] = "الملف غير موجود على السيرفر";
                    return RedirectToAction("MyInvoices");
                }

                // قراءة الملف كـ byte array
                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                // استخراج اسم الملف الأصلي
                string fileName = Path.GetFileName(filePath);

                // تحديد نوع المحتوى (يمكن تحسينه حسب امتداد الملف)
                string contentType = "application/pdf"; // أو استخدم GetContentType(fileName)

                // لو الملفات كبيرة، استخدم FileStream بدلاً من تحميل الملف بالكامل في الذاكرة
                var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return File(stream, contentType, fileName);
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["ErrorMessage"] = "ليس لديك صلاحية للوصول إلى هذا الملف";
                return RedirectToAction("MyInvoices");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطأ في تحميل الفاتورة: {ex.Message}";
                return RedirectToAction("MyInvoices");
            }
        }

        // دالة مساعدة لتحديد نوع المحتوى (اختيارية)
        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };
        }
    }
}