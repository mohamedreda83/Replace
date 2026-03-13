using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartRecycle.Models;
using SmartRecycle.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SmartRecycle.Controllers
{
    [Authorize]
    public class UserProfileController : Controller
    {
        private readonly SmartRecycleContext _context;
        public int userid { get; set; }
        public string userids { get; set; }


        public UserProfileController(SmartRecycleContext context)
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
        // GET: /UserProfile/Edit
        public async Task<IActionResult> Edit()
        {
            l();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id ==userid);
            if (user == null)
            {
                return NotFound();
            }
    
            var viewModel = new UserProfileViewModel
            {
                Id = user.Id,
                Username = user.Username,
                // Don't send the actual password hash to the view
                Points = user.Points
            };

            return View(viewModel);
        }

        // POST: /UserProfile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserProfileViewModel model)
        {
            l();
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FindAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }
   
            // Check if the username already exists for another user
            if (await _context.Users.AnyAsync(u => u.Username == model.Username && u.Id != model.Id))
            {
                ModelState.AddModelError("Username", "اسم المستخدم مستخدم بالفعل");
                return View(model);
            }

            // Update user properties
            user.Username = model.Username;

            // Only update password if a new one was provided
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                // Hash the password (replace with your actual password hashing method)
                user.PasswordHash = HashPassword(model.NewPassword);
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تحديث البيانات الشخصية بنجاح";
                return RedirectToAction("Edit");
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "حدث خطأ أثناء تحديث البيانات. يرجى المحاولة مرة أخرى.");
                return View(model);
            }
        }

        // Helper method to get the current user ID


        // Helper method to hash passwords
        private string HashPassword(string password)
        {
            // Replace with your actual password hashing implementation
            // For example, if using ASP.NET Core Identity:
            // return _passwordHasher.HashPassword(null, password);

            // Temporary implementation for demo purposes
            return password;
        }
    }
}