using Microsoft.AspNetCore.Mvc;

namespace SmartRecycle.Controllers
{
    public class MaintenanceController : Controller
    {
        public IActionResult Monitor()
        {
            return View();
        }
    }
}
