using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartRecycle.Models;
using SmartRecycle.Repositories;
using System.Security.Claims;

namespace SmartRecycle.Controllers
{
    public class AccountController : Controller
    {
        private readonly SmartRecycleContext context;

        public AccountController(SmartRecycleContext _context)
        {
            context = _context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(string username, string password)
        {
            var user = context.Users.FirstOrDefault(a => a.Username.ToLower() == username.ToLower() && a.PasswordHash == password);

            if (user == null)
            {
                ViewBag.LoginError = "Invalid credentials";
                return View();
            }
            else
            {
                // ✅ حفظ في TempData و Session
                TempData["Id"] = user.Id;
                HttpContext.Session.SetInt32("UserId", user.Id);

                ViewBag.UserName = user.Username;
                ViewBag.UserPoints = user.Points;

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // ✅ أضفنا الـ ID
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("Point", (user.Points).ToString()),
                    new Claim(ClaimTypes.Role, user.Roles)
                };

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync("Cookies", claimsPrincipal);

                if (user.Roles.ToLower() == "admin")
                {
                    return RedirectToAction("Index", "Admin");
                }

                return RedirectToAction("Index", "Dashboard");
            }
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, string phone, string address, string branch)
        {
            var existingUser = context.Users.FirstOrDefault(a => a.Username.ToLower() == username.ToLower());

            if (existingUser == null)
            {
                User newUser = new User()
                {
                    Username = username,
                    PasswordHash = password,
                    Phone = phone,
                    Address = address,
                    Branch = branch,
                    Roles = "user",
                    Points = 0
                };

                context.Users.Add(newUser);
                context.SaveChanges();

                var user = context.Users.FirstOrDefault(a => a.Username.ToLower() == username.ToLower());

                // ✅ حفظ في TempData و Session
                TempData["Id"] = user.Id;
                HttpContext.Session.SetInt32("UserId", user.Id);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // ✅ أضفنا الـ ID
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Roles)
                };

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync("Cookies", claimsPrincipal);

                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                ViewBag.LoginError = "This username already exists.";
                return View();
            }
        }

        public IActionResult Download()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/apks/Replace.apk");
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/vnd.android.package-archive", "Replace.apk");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            HttpContext.Session.Clear(); // ✅ أضفنا مسح الـ Session
            TempData.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}