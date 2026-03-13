using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartRecycle.Models;
using System.Net;
using System.Net.Mail;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Replace.Controllers
{
    public class HomeController : Controller
    {
        private readonly SmartRecycleContext _context;
        private readonly IConfiguration _configuration;

        public HomeController(SmartRecycleContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel
            {
                Rules = await _context.Rules
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.Order)
                    .ToListAsync(),

                AverageRating = await _context.Ratings
                    .Where(r => r.IsApproved)
                    .AverageAsync(r => (double?)r.RatingValue) ?? 0,

                TotalRatings = await _context.Ratings
                    .Where(r => r.IsApproved)
                    .CountAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRating(int Rating, string Comment)
        {
            if (Rating < 1 || Rating > 5 || string.IsNullOrEmpty(Comment))
            {
                TempData["Error"] = "يرجى تقديم تقييم صحيح مع تعليق";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var userId = User.Identity?.IsAuthenticated == true
    ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
    : null;
                var rating = new Rating
                {
                    RatingValue = Rating,
                    Comment = Comment,
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    IsApproved = false // يحتاج موافقة الأدمن
                };

                _context.Ratings.Add(rating);
                await _context.SaveChangesAsync();

                TempData["Success"] = "شكراً لتقييمك! سيتم مراجعته قريباً";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["Error"] ="عملية التقيم تلزم تسجيل الدخول";
                return RedirectToAction(nameof(Index));
            }


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitContact(string Type, string Email, string Subject, string Message)
        {
            if (string.IsNullOrEmpty(Type) || string.IsNullOrEmpty(Email) ||
                string.IsNullOrEmpty(Subject) || string.IsNullOrEmpty(Message))
            {
                TempData["Error"] = "يرجى ملء جميع الحقول";
                return RedirectToAction(nameof(Index));
            }

       

            var contact = new ContactMessage
            {
                Type = Type,
                Email = Email,
                Subject = Subject,
                Message = Message,
                
                CreatedAt = DateTime.Now
            };

            _context.ContactMessages.Add(contact);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إرسال رسالتك بنجاح! سنتواصل معك قريباً";
            return RedirectToAction(nameof(Index));
        }

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
                return false;
            }
        }
    }
}