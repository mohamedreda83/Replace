using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRecycle.Models;
using System.Reflection.PortableExecutable;
using System.Security.Claims;

namespace SmartRecycle.Controllers
{
    public class MachinesController : Controller
    {
        private readonly SmartRecycleContext context;

        public MachinesController(SmartRecycleContext _context)
        {
            context = _context;
        }

        #region Machine Login & Session Management

        [HttpGet]
        public IActionResult LoginToMachine(int machineId, Guid urlGuid)
        {
            var machine = context.Machines.FirstOrDefault(m => m.Id == machineId && m.URL_GUID == urlGuid);

            if (machine == null)
            {
                ViewBag.Error = "Invalid machine or session expired";
                return View("Error");
            }

            if (machine.Status != "Active")
            {
                ViewBag.Error = "This machine is not active";
                return View("Error");
            }

            ViewBag.MachineId = machineId;
            ViewBag.UrlGuid = urlGuid;
            ViewBag.MachineLocation = machine.Location;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginToMachine(int machineId, Guid urlGuid, string username, string password)
        {
            var machine = context.Machines.FirstOrDefault(m => m.Id == machineId && m.URL_GUID == urlGuid);

            if (machine == null)
            {
                ViewBag.Error = "Invalid machine or session expired";
                return View("Error");
            }

            if (machine.Status != "Active")
            {
                ViewBag.Error = "This machine is not active";
                return View("Error");
            }

            var user = context.Users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower() && u.PasswordHash == password);

            if (user == null)
            {
                ViewBag.Error = "Invalid credentials";
                ViewBag.MachineId = machineId;
                ViewBag.UrlGuid = urlGuid;
                ViewBag.MachineLocation = machine.Location;
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Roles),
                new Claim("UserId", user.Id.ToString()),
                new Claim("MachineId", machineId.ToString()),
                new Claim("MachineLocation", machine.Location),
                new Claim("SessionType", "Machine")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "MachineSession");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("MachineSession", claimsPrincipal, new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5)
            });
            context.Add(new MaintenanceLog
            {
                MachineId = machineId,
                MaintenanceDate = DateTime.UtcNow,
                Command = "LOGIN_SUCCESS"
            });
            context.SaveChanges();
            TempData["MachineId"] = machineId;
            TempData["UserId"] = user.Id;
            TempData["UserPoints"] = user.Points;

            return RedirectToAction("MachineHome");
        }

        [Authorize(AuthenticationSchemes = "MachineSession")]
        public IActionResult MachineHome()
        {
            var machineIdStr = User.FindFirst("MachineId")?.Value;
            var machineLocation = User.FindFirst("MachineLocation")?.Value;
            var userIdStr = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(machineIdStr) || string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("LogoutSuccess");
            }

            int.TryParse(machineIdStr, out int machineId);
            int.TryParse(userIdStr, out int userId);

            var machine = context.Machines.FirstOrDefault(m => m.Id == machineId);
            var user = context.Users.FirstOrDefault(u => u.Id == userId);

            if (machine == null || user == null)
            {
                return RedirectToAction("LogoutSuccess");
            }

            // هنا تحط الـ IP الخاص بالـ ESP32 - يفضل تخزينه في الداتابيز مع كل ماكينة
            ViewBag.EspIpAddress = machine.ApiKey ?? "192.168.1.29"; // استخدم ApiKey field لتخزين ESP IP
            ViewBag.MachineId = machineId;
            ViewBag.MachineLocation = machineLocation;
            ViewBag.UserId = userId;
            ViewBag.Username = User.Identity.Name;
            ViewBag.UserPoints = user.Points;

            return View();
        }

        [Authorize(AuthenticationSchemes = "MachineSession")]
        public async Task<IActionResult> LogoutFromMachine()
        {
            var machineIdClaim = User.FindFirst("MachineId")?.Value;

            if (int.TryParse(machineIdClaim, out int machineId))
            {
                var machine = context.Machines.FirstOrDefault(m => m.Id == machineId);
                if (machine != null)
                {
                    machine.URL_GUID = Guid.NewGuid();
                    context.SaveChanges();
                }
            }
            context.Add(new MaintenanceLog
            {
                MachineId = machineId,
                MaintenanceDate = DateTime.UtcNow,
                Command = "LOGOUT"
            });
            context.SaveChanges();
            await HttpContext.SignOutAsync("MachineSession");
            TempData.Clear();

            return RedirectToAction("LogoutSuccess");
        }

        public IActionResult LogoutSuccess()
        {
            ViewBag.Message = "Logged out successfully. Session has been terminated.";
            return View();
        }

        [Authorize(AuthenticationSchemes = "MachineSession")]
        [HttpGet]
        public IActionResult CheckSession()
        {
            return Ok(new
            {
                success = true,
                username = User.Identity.Name,
                machineId = User.FindFirst("MachineId")?.Value,
                expiresIn = "5 minutes from login"
            });
        }

        #endregion

        #region Recycling Detection & Logging

        [Authorize(AuthenticationSchemes = "MachineSession")]
        [HttpPost]
        public async Task<IActionResult> SaveRecyclingLog([FromBody] RecyclingDetectionRequest request)
        {
            try
            {
                var user = context.Users.FirstOrDefault(u => u.Id == request.UserId);

                if (user == null)
                {
                    return BadRequest(new { success = false, message = "المستخدم غير موجود" });
                }

                int plasticPoints = request.PlasticBottles * 7;
                int canPoints = request.Cans * 12;
                int totalPoints = plasticPoints + canPoints;

                if (request.PlasticBottles > 0)
                {
                    var plasticLog = new RecyclingLog
                    {
                        UserId = request.UserId,
                        BottleType = "Plastic",
                        PointsAwarded = plasticPoints,
                        Timestamp = DateTime.UtcNow
                    };
                    context.RecyclingLogs.Add(plasticLog);
                }

                if (request.Cans > 0)
                {
                    var canLog = new RecyclingLog
                    {
                        UserId = request.UserId,
                        BottleType = "Can",
                        PointsAwarded = canPoints,
                        Timestamp = DateTime.UtcNow
                    };
                    context.RecyclingLogs.Add(canLog);
                }

                user.Points += totalPoints;
                context.Add(new MaintenanceLog
                {
                    MachineId = request.MachineId,
                    MaintenanceDate = DateTime.UtcNow,
                    Command = "ITEM_DETECTED"
                });
                context.SaveChanges();

                await context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "تم التسجيل بنجاح",
                    plasticBottles = request.PlasticBottles,
                    cans = request.Cans,
                    plasticPoints = plasticPoints,
                    canPoints = canPoints,
                    pointsEarned = totalPoints,
                    totalPoints = user.Points
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطأ في الخادم",
                    error = ex.Message
                });
            }
        }

        #endregion

        #region Machine Management (Admin)

        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult Index()
        {
            var machines = context.Machines.OrderByDescending(m => m.Id).ToList();
            return View(machines);
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Machines machine)
        {
            if (ModelState.IsValid)
            {
                machine.URL_GUID = Guid.NewGuid();

                if (string.IsNullOrEmpty(machine.Status))
                {
                    machine.Status = "Active";
                }

                context.Machines.Add(machine);
                context.SaveChanges();

                TempData["SuccessMessage"] = "تم إضافة الماكينة بنجاح";
                return RedirectToAction("Index");
            }

            return View(machine);
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult Details(int id)
        {
            var machine = context.Machines.FirstOrDefault(m => m.Id == id);

            if (machine == null)
            {
                TempData["ErrorMessage"] = "الماكينة غير موجودة";
                return RedirectToAction("Index");
            }

            return View(machine);
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var machine = context.Machines.FirstOrDefault(m => m.Id == id);

            if (machine == null)
            {
                TempData["ErrorMessage"] = "الماكينة غير موجودة";
                return RedirectToAction("Index");
            }

            return View(machine);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Machines machine)
        {
            if (ModelState.IsValid)
            {
                var existingMachine = context.Machines.FirstOrDefault(m => m.Id == machine.Id);

                if (existingMachine == null)
                {
                    TempData["ErrorMessage"] = "الماكينة غير موجودة";
                    return RedirectToAction("Index");
                }

                existingMachine.Location = machine.Location;
                existingMachine.Status = machine.Status;
                existingMachine.LastMaintenanceDate = machine.LastMaintenanceDate;
                existingMachine.ApiKey = machine.ApiKey;

                context.SaveChanges();

                TempData["SuccessMessage"] = "تم تحديث الماكينة بنجاح";
                return RedirectToAction("Details", new { id = machine.Id });
            }

            return View(machine);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var machine = context.Machines.FirstOrDefault(m => m.Id == id);

            if (machine == null)
            {
                TempData["ErrorMessage"] = "الماكينة غير موجودة";
                return RedirectToAction("Index");
            }

            context.Machines.Remove(machine);
            context.SaveChanges();

            TempData["SuccessMessage"] = "تم حذف الماكينة بنجاح";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegenerateGuid(int id)
        {
            var machine = context.Machines.FirstOrDefault(m => m.Id == id);

            if (machine == null)
            {
                TempData["ErrorMessage"] = "الماكينة غير موجودة";
                return RedirectToAction("Index");
            }

            machine.URL_GUID = Guid.NewGuid();
            context.SaveChanges();

            TempData["SuccessMessage"] = "تم تجديد رابط الماكينة بنجاح";
            return RedirectToAction("Details", new { id = machine.Id });
        }

        #endregion

        #region API Endpoints

        [HttpGet]
        [Route("api/machines/get-url-guid/{machineId}")]
        public IActionResult GetMachineUrlGuid(int machineId)
        {
            try
            {
                var machine = context.Machines.FirstOrDefault(m => m.Id == machineId && m.Status == "Active");

                if (machine == null)
                {
                    return NotFound(new { success = false, message = "Machine not found or not active" });
                }

                var loginUrl = $"{Request.Scheme}://{Request.Host}/Machines/LoginToMachine?machineId={machine.Id}&urlGuid={machine.URL_GUID}";

                return Ok(new
                {
                    success = true,
                    machineId = machine.Id,
                    location = machine.Location,
                    urlGuid = machine.URL_GUID,
                    status = machine.Status,
                    loginUrl = loginUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Server error: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("api/machines/get-url-guid-by-location")]
        public IActionResult GetMachineUrlGuidByLocation(string location)
        {
            var machine = context.Machines.FirstOrDefault(m => m.Location.ToLower() == location.ToLower() && m.Status == "Active");

            if (machine == null)
            {
                return NotFound(new { success = false, message = "Machine not found or not active" });
            }

            return Ok(new
            {
                success = true,
                machineId = machine.Id,
                location = machine.Location,
                urlGuid = machine.URL_GUID,
                status = machine.Status,
                loginUrl = Url.Action("LoginToMachine", "Machines", new { machineId = machine.Id, urlGuid = machine.URL_GUID }, Request.Scheme)
            });
        }
        public IActionResult MaintenanceMonitor()
        {
            return View();
        }
        [HttpPost]
        public IActionResult LogNoItemDetected([FromBody] NoItemDto dto)
        {
            context.Add(new MaintenanceLog
            {
                MachineId = dto.MachineId,
                MaintenanceDate = DateTime.UtcNow,
                Command = "NO_ITEM_DETECTED"
            });
            context.SaveChanges();
            return Ok();
        }

        public class NoItemDto
        {
            public int MachineId { get; set; }
        }

        #endregion
    }

    // Request Model
    public class RecyclingDetectionRequest
    {
        public int UserId { get; set; }
        public int MachineId { get; set; }
        public int PlasticBottles { get; set; }
        public int Cans { get; set; }
    }

}