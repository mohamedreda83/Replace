using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using SmartRecycle.Models;
using SmartRecycle.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net;
using System.Net.Mail;

namespace SmartRecycle.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly SmartRecycleContext context;
        private readonly IConfiguration _configuration;
        public int userId { get; set; }
        public string userids { get; set; }
        public AdminController(SmartRecycleContext _context, IConfiguration configuration)
        {
            context = _context;
            _configuration = configuration;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (TempData["Id"] != null)
            {
                userids = TempData["Id"].ToString();
                TempData["Id"] = userids;

                if (int.TryParse(userids, out int parsedId))
                {
                    userId = parsedId;
                }
                else
                {
                    userId = 0; // أو أي قيمة افتراضية حسب اختيارك
                }
            }

            base.OnActionExecuting(filterContext);
        }
        public void l()
        {
            var user = context.Users.FirstOrDefault(a => a.Id == userId);
            ViewBag.UserName = user.Username;
            ViewBag.UserPoints = user.Points;
        }

        public async Task<IActionResult> Index()
        {
            l();
            ViewBag.TotalUsers = await context.Users.CountAsync();
            ViewBag.TotalOrders = await context.Orders.CountAsync();
            ViewBag.TotalRecyclingLogs = await context.RecyclingLogs.CountAsync();

            return View();
        }

        public async Task<IActionResult> Users()
        {
            l();
            var users = await context.Users.ToListAsync();
            return View(users);
        }

        public IActionResult CreateUser()
        {
            l();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            l();
   
                var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == user.Username.ToLower());

                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Username already exists");
                    return View(user);
                }

                // Set default values if not provided
                if (string.IsNullOrEmpty(user.Roles))
                {
                    user.Roles = "user";
                }

                context.Users.Add(user);
                await context.SaveChangesAsync();

                TempData["SuccessMessage"] = "User created successfully";
                return RedirectToAction(nameof(Users));
            
        }

        public async Task<IActionResult> EditUser(int id)
        {
            l();
            var user = await context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(User updatedUser)
        {
            l();

            try
            {
                // البحث عن المستخدم الموجود بالفعل في قاعدة البيانات
                var existingUser = await context.Users.FindAsync(updatedUser.Id);

                if (existingUser == null)
                {
                    return NotFound();
                }

                // تحديث البيانات
                existingUser.Username = updatedUser.Username;
                existingUser.PasswordHash = updatedUser.PasswordHash;
                existingUser.Gmail = updatedUser.Gmail;
                existingUser.Phone = updatedUser.Phone;
                existingUser.Address = updatedUser.Address;
                existingUser.Branch = updatedUser.Branch;
                existingUser.Roles = updatedUser.Roles;
                existingUser.Points = updatedUser.Points;

                // حفظ التغييرات
                await context.SaveChangesAsync();

                TempData["SuccessMessage"] = "User updated successfully";
                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "حدث خطأ أثناء تحديث البيانات: " + ex.Message);
                return View(updatedUser);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            l();
            var user = await context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = "User deleted successfully";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(int id, string role)
        {
            l();
            var user = await context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.Roles = role;
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"User role changed to {role} successfully";
            return RedirectToAction(nameof(Users));
        }

        private bool UserExists(int id)
        {
            l();
            return context.Users.Any(e => e.Id == id);
        }
        //__________________________________________________________
        // Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                TotalRatings = await context.Ratings.CountAsync(),
                PendingRatings = await context.Ratings.CountAsync(r => !r.IsApproved),
                TotalMessages = await context.ContactMessages.CountAsync(),
                UnreadMessages = await context.ContactMessages.CountAsync(m => !m.IsRead),
                AverageRating = await context.Ratings
                    .Where(r => r.IsApproved)
                    .AverageAsync(r => (double?)r.RatingValue) ?? 0
            };

            return View(model);
        }

        // ==================== RULES MANAGEMENT ====================

        public async Task<IActionResult> Rules()
        {
            var rules = await context.Rules.OrderBy(r => r.Order).ToListAsync();
            return View(rules);
        }

        [HttpGet]
        public IActionResult CreateRule()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRule(SmartRecycle.Models.Rule rule)
        {
            if (ModelState.IsValid)
            {
                rule.CreatedAt = DateTime.Now;
                context.Rules.Add(rule);
                await context.SaveChangesAsync();
                TempData["Success"] = "تم إضافة القاعدة بنجاح";
                return RedirectToAction(nameof(Rules));
            }
            return View(rule);
        }

        [HttpGet]
        public async Task<IActionResult> EditRule(int id)
        {
            var rule = await context.Rules.FindAsync(id);
            if (rule == null)
                return NotFound();
            return View(rule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRule(SmartRecycle.Models.Rule rule)
        {
            if (ModelState.IsValid)
            {
                context.Rules.Update(rule);
                await context.SaveChangesAsync();
                TempData["Success"] = "تم تحديث القاعدة بنجاح";
                return RedirectToAction(nameof(Rules));
            }
            return View(rule);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRule(int id)
        {
            var rule = await context.Rules.FindAsync(id);
            if (rule != null)
            {
                context.Rules.Remove(rule);
                await context.SaveChangesAsync();
                TempData["Success"] = "تم حذف القاعدة بنجاح";
            }
            return RedirectToAction(nameof(Rules));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleRuleStatus(int id)
        {
            var rule = await context.Rules.FindAsync(id);
            if (rule != null)
            {
                rule.IsActive = !rule.IsActive;
                await context.SaveChangesAsync();
                return Json(new { success = true, isActive = rule.IsActive });
            }
            return Json(new { success = false });
        }

        // ==================== RATINGS MANAGEMENT ====================

        public async Task<IActionResult> Ratings(string filter = "all")
        {
            IQueryable<Rating> query = context.Ratings.OrderByDescending(r => r.CreatedAt);

            switch (filter)
            {
                case "pending":
                    query = query.Where(r => !r.IsApproved);
                    break;
                case "approved":
                    query = query.Where(r => r.IsApproved);
                    break;
            }

            var ratings = await query.ToListAsync();
            ViewBag.Filter = filter;
            return View(ratings);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRating(int id)
        {
            var rating = await context.Ratings.FindAsync(id);
            if (rating != null)
            {
                rating.IsApproved = true;
                await context.SaveChangesAsync();
                TempData["Success"] = "تم الموافقة على التقييم";
            }
            return RedirectToAction(nameof(Ratings));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRating(int id)
        {
            var rating = await context.Ratings.FindAsync(id);
            if (rating != null)
            {
                context.Ratings.Remove(rating);
                await context.SaveChangesAsync();
                TempData["Success"] = "تم حذف التقييم";
            }
            return RedirectToAction(nameof(Ratings));
        }

        // ==================== MESSAGES MANAGEMENT ====================

        public async Task<IActionResult> Messages(string filter = "all")
        {
            IQueryable<ContactMessage> query = context.ContactMessages
                .OrderByDescending(m => m.CreatedAt);

            switch (filter)
            {
                case "unread":
                    query = query.Where(m => !m.IsRead);
                    break;
                case "questions":
                    query = query.Where(m => m.Type == "Question");
                    break;
                case "complaints":
                    query = query.Where(m => m.Type == "Complaint");
                    break;
                case "suggestions":
                    query = query.Where(m => m.Type == "Suggestion");
                    break;
            }

            var messages = await query.ToListAsync();
            ViewBag.Filter = filter;
            return View(messages);
        }

        [HttpGet]
        public async Task<IActionResult> ViewMessage(int id)
        {
            var message = await context.ContactMessages.FindAsync(id);
            if (message == null)
                return NotFound();

            // Mark as read
            if (!message.IsRead)
            {
                message.IsRead = true;
                await context.SaveChangesAsync();
            }

            return View(message);
        }

        [HttpGet]
        public async Task<IActionResult> ReplyMessage(int id)
        {
            var message = await context.ContactMessages.FindAsync(id);
            if (message == null)
                return NotFound();

            var model = new ReplyMessageViewModel
            {
                MessageId = message.Id,
                ToEmail = message.Email,
                OriginalSubject = message.Subject,
                OriginalMessage = message.Message,
                ReplySubject = $"Re: {message.Subject}"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyMessage(ReplyMessageViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var message = await context.ContactMessages.FindAsync(model.MessageId);
            if (message == null)
                return NotFound();

            // إرسال البريد الإلكتروني
            var emailBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='background-color: #4CAF50; color: white; padding: 20px; text-align: center;'>
                        <h2>Replace Support</h2>
                    </div>
                    <div style='padding: 20px;'>
                        <p>مرحباً،</p>
                        <p>{model.ReplyMessage}</p>
                        <hr>
                        <p style='color: #666; font-size: 12px;'>
                            رسالتك الأصلية:<br>
                            {message.Message}
                        </p>
                    </div>
                    <div style='background-color: #f5f5f5; padding: 10px; text-align: center; font-size: 12px;'>
                        <p>شكراً لاستخدامك Replace</p>
                    </div>
                </body>
                </html>
            ";

            var emailSent = await SendEmailAsync(message.Email, model.ReplySubject, emailBody);

            if (emailSent)
            {
                // تحديث حالة الرسالة
                message.IsReplied = true;
                message.Reply = model.ReplyMessage;
                message.RepliedAt = DateTime.Now;
                message.RepliedBy = User.Identity.Name;
                await context.SaveChangesAsync();

                TempData["Success"] = "تم إرسال الرد بنجاح";
                return RedirectToAction(nameof(Messages));
            }
            else
            {
                TempData["Error"] = "فشل إرسال البريد الإلكتروني";
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var message = await context.ContactMessages.FindAsync(id);
            if (message != null)
            {
                message.IsRead = true;
                await context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Messages));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await context.ContactMessages.FindAsync(id);
            if (message != null)
            {
                context.ContactMessages.Remove(message);
                await context.SaveChangesAsync();
                TempData["Success"] = "تم حذف الرسالة";
            }
            return RedirectToAction(nameof(Messages));
        }

        // ==================== HELPER METHODS ====================

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var password = _configuration["EmailSettings:Password"];

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(fromEmail, password);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail, "Replace Support"),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Email send failed: {ex.Message}");
                return false;
            }
        }
    }

    // ViewModels
    public class AdminDashboardViewModel
    {
        public int TotalRatings { get; set; }
        public int PendingRatings { get; set; }
        public int TotalMessages { get; set; }
        public int UnreadMessages { get; set; }
        public double AverageRating { get; set; }
    }

    public class ReplyMessageViewModel
    {
        public int MessageId { get; set; }
        public string ToEmail { get; set; }
        public string OriginalSubject { get; set; }
        public string OriginalMessage { get; set; }

        [Required]
        public string ReplySubject { get; set; }

        [Required]
        public string ReplyMessage { get; set; }
    }
}