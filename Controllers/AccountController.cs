using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TaskManagementWeb.Data;

namespace TaskManagementWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user == null)
            {
                ViewBag.ErrorMessage = "نام کاربری یا رمز عبور اشتباه است.";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}