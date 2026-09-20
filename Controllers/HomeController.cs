using Microsoft.AspNetCore.Mvc;

namespace TaskManagementWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.CurrentUserName = HttpContext.Session.GetString("UserName");
            return View();
        }
    }
}