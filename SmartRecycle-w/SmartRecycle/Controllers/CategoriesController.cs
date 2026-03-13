using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SmartRecycle.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SmartRecycle.Controllers
{
    [Authorize(Roles = "admin")] // Ensure only admins can access (Roles = "Admin")
    public class CategoriesController : Controller
    {
        private readonly SmartRecycleContext _context;
        public int userid { get; set; }
        public string userids { get; set; }


        public CategoriesController(SmartRecycleContext context)
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
        // GET: Categories
        public async Task<IActionResult> Index()
        {
            l();
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            l();
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Category category)
        {
            l();
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إضافة الفئة بنجاح";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            l();
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category category)
        {
            l();
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم تعديل الفئة بنجاح";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            l();
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            l();
            var category = await _context.Categories.FindAsync(id);

            // Check if category has associated products
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);

            if (hasProducts)
            {
                TempData["ErrorMessage"] = "لا يمكن حذف الفئة لأنها تحتوي على منتجات. قم بحذف أو نقل المنتجات أولاً.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف الفئة بنجاح";
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}