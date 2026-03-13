using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;

namespace SmartRecycle.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/Index")]
        public IActionResult Index()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            ViewBag.ErrorMessage = exceptionFeature?.Error.Message ?? "حدث خطأ غير متوقع";
            ViewBag.ErrorPath = exceptionFeature?.Path;
            ViewBag.ErrorCode = 500;

            return View();
        }

        [Route("Error/StatusCode")]
        public IActionResult StatusCode(int code)
        {
            ViewBag.ErrorCode = code;
            ViewBag.ErrorMessage = code switch
            {
                404 => "الصفحة التي تبحث عنها غير موجودة",
                403 => "ليس لديك صلاحية الوصول لهذه الصفحة",
                500 => "حدث خطأ في الخادم",
                401 => "يجب تسجيل الدخول أولاً",
                _ => "حدث خطأ غير متوقع"
            };

            return View("Index");
        }
        [Route("Error/Style")]
        public IActionResult Style(string type, int code)
        {
            ViewBag.ErrorCode = code;
            ViewBag.ErrorMessage = code switch
            {
                404 => "الصفحة التي تبحث عنها غير موجودة",
                403 => "ليس لديك صلاحية الوصول لهذه الصفحة",
                500 => "حدث خطأ في الخادم",
                401 => "يجب تسجيل الدخول أولاً",
                _ => "حدث خطأ غير متوقع"
            };

            return type switch
            {
                "robot" => View("Robot"),
                "3d" => View("WebGL"),
                "lottie" => View("Lottie"),
                _ => View("Robot")
            };
        }
    }
}