using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartRecycle.Models;
using Microsoft.AspNetCore.Authorization;
using SmartRecycle.ViewModels;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SmartRecycle.Controllers
{
    [Authorize(Roles = "admin")] // Ensure only admins can access (Roles = "Admin")
    public class ProductsController : Controller
    {
        private readonly SmartRecycleContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public int userid { get; set; }
        public string userids { get; set; }


        public ProductsController(SmartRecycleContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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
        // GET: Products
        public async Task<IActionResult> Index()
        {
            l();
            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();

            return View(products);
        }

        // GET: Products/Create
        public async Task<IActionResult> Create()
        {
            l();
            ViewData["CategoryId"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel productVM)
        {
            l();
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;

                // Process the uploaded image
                if (productVM.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + productVM.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Ensure directory exists
                    Directory.CreateDirectory(uploadsFolder);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await productVM.ImageFile.CopyToAsync(fileStream);
                    }
                }

                // Create new product with the model data
                Product product = new Product
                {
                    Name = productVM.Name,
                    Description = productVM.Description,
                    PointsPrice = productVM.PointsPrice,
                    Stock = productVM.Stock,
                    IsAvailable = productVM.IsAvailable,
                    CategoryId = productVM.CategoryId,
                    ImageUrl = uniqueFileName != null ? "/images/products/" + uniqueFileName : null
                };

                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إضافة المنتج بنجاح";
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", productVM.CategoryId);
            return View(productVM);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            l();
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var productVM = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                PointsPrice = product.PointsPrice,
                Stock = product.Stock,
                IsAvailable = product.IsAvailable,
                CategoryId = product.CategoryId,
                ExistingImagePath = product.ImageUrl
            };

            ViewData["CategoryId"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            return View(productVM);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel productVM)
        {
            l();
            if (id != productVM.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var product = await _context.Products.FindAsync(id);
                    if (product == null)
                    {
                        return NotFound();
                    }

                    // Process the uploaded image
                    if (productVM.ImageFile != null)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(product.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Save new image
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + productVM.ImageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Ensure directory exists
                        Directory.CreateDirectory(uploadsFolder);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await productVM.ImageFile.CopyToAsync(fileStream);
                        }

                        product.ImageUrl = "/images/products/" + uniqueFileName;
                    }

                    // Update product properties
                    product.Name = productVM.Name;
                    product.Description = productVM.Description;
                    product.PointsPrice = productVM.PointsPrice;
                    product.Stock = productVM.Stock;
                    product.IsAvailable = productVM.IsAvailable;
                    product.CategoryId = productVM.CategoryId;

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم تعديل المنتج بنجاح";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(productVM.Id))
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

            ViewData["CategoryId"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", productVM.CategoryId);
            return View(productVM);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            l();
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            l();
            var product = await _context.Products.FindAsync(id);

            // Delete the product image if it exists
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف المنتج بنجاح";
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}