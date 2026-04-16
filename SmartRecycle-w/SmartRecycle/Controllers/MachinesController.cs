using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartRecycle.Hubs;
using SmartRecycle.Models;
using System.Security.Claims;

namespace SmartRecycle.Controllers
{
    public class MachinesController : Controller
    {
        private readonly SmartRecycleContext context;
        private readonly IHubContext<MachineStateHub> _hubContext;

        public MachinesController(SmartRecycleContext _context, IHubContext<MachineStateHub> hubContext)
        {
            context    = _context;
            _hubContext = hubContext;
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

            ViewBag.MachineId       = machineId;
            ViewBag.UrlGuid         = urlGuid;
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
                ViewBag.Error           = "Invalid credentials";
                ViewBag.MachineId       = machineId;
                ViewBag.UrlGuid         = urlGuid;
                ViewBag.MachineLocation = machine.Location;
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,      user.Username),
                new Claim(ClaimTypes.Role,      user.Roles),
                new Claim("UserId",             user.Id.ToString()),
                new Claim("MachineId",          machineId.ToString()),
                new Claim("MachineLocation",    machine.Location),
                new Claim("SessionType",        "Machine")
            };

            var claimsIdentity  = new ClaimsIdentity(claims, "MachineSession");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("MachineSession", claimsPrincipal, new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc   = DateTimeOffset.UtcNow.AddMinutes(5)
            });

            // ✅ Log to MaintenanceLogs (already in your project)
            context.Add(new MaintenanceLog
            {
                MachineId       = machineId,
                MaintenanceDate = DateTime.UtcNow,
                Command         = "LOGIN_SUCCESS"
            });

            // ✅ Track session for Flutter API polling
            MachineSessionTracker.SetSession(machineId, user.Id, DateTime.UtcNow);

            context.SaveChanges();

            TempData["MachineId"]   = machineId;
            TempData["UserId"]      = user.Id;
            TempData["UserPoints"]  = user.Points;

            // ✅ Notify Flutter app instantly via SignalR
            await _hubContext.Clients.Group($"machine_{machineId}").SendAsync("UserLoggedIn", new
            {
                isLoggedIn      = true,
                userId          = user.Id,
                username        = user.Username,
                points          = user.Points,
                machineId       = machineId,
                machineLocation = machine.Location
            });

            return RedirectToAction("MachineHome");
        }

        [Authorize(AuthenticationSchemes = "MachineSession")]
        public IActionResult MachineHome()
        {
            var machineIdStr   = User.FindFirst("MachineId")?.Value;
            var machineLocation = User.FindFirst("MachineLocation")?.Value;
            var userIdStr      = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(machineIdStr) || string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("LogoutSuccess");

            int.TryParse(machineIdStr, out int machineId);
            int.TryParse(userIdStr,   out int userId);

            var machine = context.Machines.FirstOrDefault(m => m.Id == machineId);
            var user    = context.Users.FirstOrDefault(u => u.Id == userId);

            if (machine == null || user == null)
                return RedirectToAction("LogoutSuccess");

            ViewBag.EspIpAddress    = machine.ApiKey ?? "192.168.1.29";
            ViewBag.MachineId       = machineId;
            ViewBag.MachineLocation = machineLocation;
            ViewBag.UserId          = userId;
            ViewBag.Username        = User.Identity.Name;
            ViewBag.UserPoints      = user.Points;

            return View();
        }

        [Authorize(AuthenticationSchemes = "MachineSession")]
        public async Task<IActionResult> LogoutFromMachine()
        {
            var machineIdClaim = User.FindFirst("MachineId")?.Value;
            int machineId = 0;

            if (int.TryParse(machineIdClaim, out machineId))
            {
                var machine = context.Machines.FirstOrDefault(m => m.Id == machineId);
                if (machine != null)
                {
                    machine.URL_GUID = Guid.NewGuid();
                    context.SaveChanges();

                    // ✅ Clear in-memory tracker
                    MachineSessionTracker.ClearSession(machineId);

                    // ✅ Notify Flutter: new QR ready
                    var newLoginUrl = $"{Request.Scheme}://{Request.Host}/Machines/LoginToMachine?machineId={machine.Id}&urlGuid={machine.URL_GUID}";
                    await _hubContext.Clients.Group($"machine_{machineId}").SendAsync("UserLoggedOut", new
                    {
                        isLoggedIn   = false,
                        machineId    = machineId,
                        newUrlGuid   = machine.URL_GUID.ToString(),
                        newLoginUrl  = newLoginUrl
                    });
                }
            }

            // ✅ Existing MaintenanceLog entry
            context.Add(new MaintenanceLog
            {
                MachineId       = machineId,
                MaintenanceDate = DateTime.UtcNow,
                Command         = "LOGOUT"
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
                success   = true,
                username  = User.Identity.Name,
                machineId = User.FindFirst("MachineId")?.Value,
                expiresIn = "5 minutes from login"
            });
        }

        public IActionResult MaintenanceMonitor() => View();

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
                    return BadRequest(new { success = false, message = "المستخدم غير موجود" });

                int plasticPoints = request.PlasticBottles * 7;
                int canPoints     = request.Cans * 12;
                int totalPoints   = plasticPoints + canPoints;

                if (request.PlasticBottles > 0)
                    context.RecyclingLogs.Add(new RecyclingLog
                    {
                        UserId        = request.UserId,
                        BottleType    = "Plastic",
                        PointsAwarded = plasticPoints,
                        Timestamp     = DateTime.UtcNow
                    });

                if (request.Cans > 0)
                    context.RecyclingLogs.Add(new RecyclingLog
                    {
                        UserId        = request.UserId,
                        BottleType    = "Can",
                        PointsAwarded = canPoints,
                        Timestamp     = DateTime.UtcNow
                    });

                user.Points += totalPoints;

                context.Add(new MaintenanceLog
                {
                    MachineId       = request.MachineId,
                    MaintenanceDate = DateTime.UtcNow,
                    Command         = "ITEM_DETECTED"
                });

                // ✅ Stop detecting state
                MachineSessionTracker.SetDetecting(request.MachineId, false);

                await context.SaveChangesAsync();

                // ✅ Notify Flutter: points updated in real-time
                await _hubContext.Clients.Group($"machine_{request.MachineId}").SendAsync("PointsUpdated", new
                {
                    userId         = user.Id,
                    username       = user.Username,
                    newTotalPoints = user.Points,
                    pointsEarned   = totalPoints,
                    plasticBottles = request.PlasticBottles,
                    cans           = request.Cans,
                    plasticPoints  = plasticPoints,
                    canPoints      = canPoints,
                    machineId      = request.MachineId
                });

                return Ok(new
                {
                    success        = true,
                    message        = "تم التسجيل بنجاح",
                    plasticBottles = request.PlasticBottles,
                    cans           = request.Cans,
                    plasticPoints  = plasticPoints,
                    canPoints      = canPoints,
                    pointsEarned   = totalPoints,
                    totalPoints    = user.Points
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> LogNoItemDetected([FromBody] NoItemDto dto)
        {
            context.Add(new MaintenanceLog
            {
                MachineId       = dto.MachineId,
                MaintenanceDate = DateTime.UtcNow,
                Command         = "NO_ITEM_DETECTED"
            });
            MachineSessionTracker.SetDetecting(dto.MachineId, false);
            context.SaveChanges();

            // ✅ Notify Flutter: detection ended (nothing found)
            await _hubContext.Clients.Group($"machine_{dto.MachineId}").SendAsync("DetectionEnded", new
            {
                machineId = dto.MachineId,
                found     = false,
                message   = "لم يتم اكتشاف أي عنصر"
            });

            return Ok();
        }

        public class NoItemDto { public int MachineId { get; set; } }

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
        public IActionResult Create() => View();

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Machines machine)
        {
            if (ModelState.IsValid)
            {
                machine.URL_GUID = Guid.NewGuid();
                if (string.IsNullOrEmpty(machine.Status)) machine.Status = "Active";
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
            if (machine == null) { TempData["ErrorMessage"] = "الماكينة غير موجودة"; return RedirectToAction("Index"); }
            return View(machine);
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var machine = context.Machines.FirstOrDefault(m => m.Id == id);
            if (machine == null) { TempData["ErrorMessage"] = "الماكينة غير موجودة"; return RedirectToAction("Index"); }
            return View(machine);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Machines machine)
        {
            if (ModelState.IsValid)
            {
                var existing = context.Machines.FirstOrDefault(m => m.Id == machine.Id);
                if (existing == null) { TempData["ErrorMessage"] = "الماكينة غير موجودة"; return RedirectToAction("Index"); }
                existing.Location            = machine.Location;
                existing.Status              = machine.Status;
                existing.LastMaintenanceDate = machine.LastMaintenanceDate;
                existing.ApiKey              = machine.ApiKey;
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
            if (machine == null) { TempData["ErrorMessage"] = "الماكينة غير موجودة"; return RedirectToAction("Index"); }
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
            if (machine == null) { TempData["ErrorMessage"] = "الماكينة غير موجودة"; return RedirectToAction("Index"); }
            machine.URL_GUID = Guid.NewGuid();
            context.SaveChanges();
            TempData["SuccessMessage"] = "تم تجديد رابط الماكينة بنجاح";
            return RedirectToAction("Details", new { id = machine.Id });
        }

        #endregion

        #region API Endpoints (Flutter + Web)

        [HttpGet]
        [Route("api/machines/get-url-guid/{machineId}")]
        public IActionResult GetMachineUrlGuid(int machineId)
        {
            try
            {
                var machine = context.Machines.FirstOrDefault(m => m.Id == machineId && m.Status == "Active");
                if (machine == null)
                    return NotFound(new { success = false, message = "Machine not found or not active" });

                var loginUrl = $"{Request.Scheme}://{Request.Host}/Machines/LoginToMachine?machineId={machine.Id}&urlGuid={machine.URL_GUID}";
                return Ok(new { success = true, machineId = machine.Id, location = machine.Location, urlGuid = machine.URL_GUID, status = machine.Status, loginUrl });
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
                return NotFound(new { success = false, message = "Machine not found or not active" });

            return Ok(new
            {
                success   = true,
                machineId = machine.Id,
                location  = machine.Location,
                urlGuid   = machine.URL_GUID,
                status    = machine.Status,
                loginUrl  = Url.Action("LoginToMachine", "Machines", new { machineId = machine.Id, urlGuid = machine.URL_GUID }, Request.Scheme)
            });
        }

        /// <summary>
        /// ✅ Flutter polls this every ~800ms to get real-time machine state
        /// Returns: isLoggedIn, user info, points, recent logs, session timer, isDetecting
        /// </summary>
        [HttpGet]
        [Route("api/machines/{machineId}/state")]
        public IActionResult GetMachineState(int machineId)
        {
            try
            {
                var machine = context.Machines.FirstOrDefault(m => m.Id == machineId);
                if (machine == null)
                    return NotFound(new { success = false, message = "Machine not found" });

                var session = MachineSessionTracker.GetSession(machineId);

                if (session == null)
                {
                    var loginUrl = $"{Request.Scheme}://{Request.Host}/Machines/LoginToMachine?machineId={machine.Id}&urlGuid={machine.URL_GUID}";
                    return Ok(new
                    {
                        success         = true,
                        isLoggedIn      = false,
                        machineId       = machineId,
                        machineLocation = machine.Location,
                        loginUrl        = loginUrl,
                        urlGuid         = machine.URL_GUID.ToString()
                    });
                }

                var user = context.Users.FirstOrDefault(u => u.Id == session.UserId);
                if (user == null)
                {
                    MachineSessionTracker.ClearSession(machineId);
                    var loginUrl = $"{Request.Scheme}://{Request.Host}/Machines/LoginToMachine?machineId={machine.Id}&urlGuid={machine.URL_GUID}";
                    return Ok(new { success = true, isLoggedIn = false, machineId, loginUrl, urlGuid = machine.URL_GUID.ToString() });
                }

                var recentLogs = context.RecyclingLogs
                    .Where(r => r.UserId == user.Id && r.Timestamp >= session.LoginTime)
                    .OrderByDescending(r => r.Timestamp)
                    .Take(20)
                    .Select(r => new { r.BottleType, r.PointsAwarded, r.Timestamp })
                    .ToList();

                int    sessionPoints  = recentLogs.Sum(r => r.PointsAwarded);
                int    totalItems     = context.RecyclingLogs.Count(r => r.UserId == user.Id);
                string userLevel      = GetUserLevel(totalItems);
                double secondsLeft    = (session.LoginTime.AddMinutes(5) - DateTime.UtcNow).TotalSeconds;
                bool   isDetecting    = MachineSessionTracker.IsDetecting(machineId);

                return Ok(new
                {
                    success            = true,
                    isLoggedIn         = true,
                    machineId          = machineId,
                    machineLocation    = machine.Location,
                    userId             = user.Id,
                    username           = user.Username,
                    points             = user.Points,
                    sessionPoints      = sessionPoints,
                    recentLogs         = recentLogs,
                    loginTime          = session.LoginTime,
                    sessionSecondsLeft = (int)Math.Max(0, secondsLeft),
                    itemsRecycled      = totalItems,
                    userLevel          = userLevel,
                    isDetecting        = isDetecting
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// ✅ Flutter logout button — invalidates session + regenerates QR
        /// </summary>
        [HttpPost]
        [Route("api/machines/{machineId}/logout")]
        public async Task<IActionResult> LogoutFromMachineApi(int machineId)
        {
            try
            {
                var machine = context.Machines.FirstOrDefault(m => m.Id == machineId);
                if (machine == null)
                    return NotFound(new { success = false, message = "Machine not found" });

                machine.URL_GUID = Guid.NewGuid();

                context.Add(new MaintenanceLog
                {
                    MachineId       = machineId,
                    MaintenanceDate = DateTime.UtcNow,
                    Command         = "LOGOUT"
                });

                context.SaveChanges();
                MachineSessionTracker.ClearSession(machineId);

                // Force the web browser (MachineHome) to logout via SignalR
                await _hubContext.Clients.Group($"machine_{machineId}").SendAsync("ForceLogout", new
                {
                    machineId = machineId,
                    message   = "تم تسجيل الخروج من التطبيق"
                });

                var newLoginUrl = $"{Request.Scheme}://{Request.Host}/Machines/LoginToMachine?machineId={machine.Id}&urlGuid={machine.URL_GUID}";
                return Ok(new { success = true, message = "تم تسجيل الخروج بنجاح", newLoginUrl, newUrlGuid = machine.URL_GUID.ToString() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// ✅ Called from MachineHome JS when user presses "ابدأ عملية التدوير"
        /// Notifies Flutter to show loading animation
        /// </summary>
        [HttpPost]
        [Route("api/machines/{machineId}/detection-started")]
        public async Task<IActionResult> DetectionStarted(int machineId)
        {
            MachineSessionTracker.SetDetecting(machineId, true);
            context.Add(new MaintenanceLog
            {
                MachineId       = machineId,
                MaintenanceDate = DateTime.UtcNow,
                Command         = "DETECTION_STARTED"
            });
            context.SaveChanges();

            await _hubContext.Clients.Group($"machine_{machineId}").SendAsync("DetectionStarted", new
            {
                machineId = machineId,
                message   = "جاري الكشف عن العناصر..."
            });
            return Ok(new { success = true });
        }

        private static string GetUserLevel(int items)
        {
            if (items >= 50) return "Platinum";
            if (items >= 30) return "Gold";
            if (items >= 15) return "Silver";
            return "Bronze";
        }

        #endregion
    }

    // =========================================================
    // In-memory session tracker
    // =========================================================
    public static class MachineSessionTracker
    {
        private static readonly Dictionary<int, MachineSession> _sessions = new();
        private static readonly HashSet<int> _detecting = new();
        private static readonly object _lock = new();

        public static void SetSession(int machineId, int userId, DateTime loginTime)
        {
            lock (_lock)
            {
                _sessions[machineId] = new MachineSession { UserId = userId, LoginTime = loginTime };
            }
        }

        public static MachineSession? GetSession(int machineId)
        {
            lock (_lock)
            {
                if (_sessions.TryGetValue(machineId, out var s))
                {
                    if (DateTime.UtcNow > s.LoginTime.AddMinutes(5))
                    {
                        _sessions.Remove(machineId);
                        _detecting.Remove(machineId);
                        return null;
                    }
                    return s;
                }
                return null;
            }
        }

        public static void ClearSession(int machineId)
        {
            lock (_lock)
            {
                _sessions.Remove(machineId);
                _detecting.Remove(machineId);
            }
        }

        public static void SetDetecting(int machineId, bool val)
        {
            lock (_lock) { if (val) _detecting.Add(machineId); else _detecting.Remove(machineId); }
        }

        public static bool IsDetecting(int machineId)
        {
            lock (_lock) { return _detecting.Contains(machineId); }
        }
    }

    public class MachineSession
    {
        public int UserId { get; set; }
        public DateTime LoginTime { get; set; }
    }

    public class RecyclingDetectionRequest
    {
        public int UserId { get; set; }
        public int MachineId { get; set; }
        public int PlasticBottles { get; set; }
        public int Cans { get; set; }
    }
}
