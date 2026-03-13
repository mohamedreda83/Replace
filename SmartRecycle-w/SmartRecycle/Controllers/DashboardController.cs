using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using SmartRecycle.Models;

namespace SmartRecycle.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly SmartRecycleContext _context;

        public int userid { get; set; }
        public string userids { get; set; }



        public DashboardController(SmartRecycleContext context)
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
        public async Task<IActionResult> Index()
        {

            l();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userid);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get recycling history
            var recyclingLogs = await _context.RecyclingLogs
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();

            // Count total items recycled
            int itemsRecycled = recyclingLogs.Count;

            // Determine user level
            string level = GetUserLevel(itemsRecycled);

            // Create view model
            var viewModel = new DashboardViewModel
            {
                User = user,
                RecyclingLogs = recyclingLogs,
                ItemsRecycled = itemsRecycled,
                UserLevel = level
            };

            return View(viewModel);
        }

        private string GetUserLevel(int itemsRecycled)
        {
            l();
            if (itemsRecycled >= 50)
                return "Platinum";
            else if (itemsRecycled >= 30)
                return "Gold";
            else if (itemsRecycled >= 15)
                return "Silver";
            else
                return "Bronze";
        }
    }
}