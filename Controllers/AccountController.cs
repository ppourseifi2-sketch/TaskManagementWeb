using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TaskManagementWeb.Data;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AccountController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public IActionResult Login()
        {
            ViewBag.RecaptchaSiteKey = _config["Recaptcha:SiteKey"];
            return View();
        }

       [HttpPost]
public async System.Threading.Tasks.Task<IActionResult> Login(string username, string password)
{
    ViewBag.RecaptchaSiteKey = _config["Recaptcha:SiteKey"];

    string captchaResponse = Request.Form["g-recaptcha-response"];
    bool captchaIsValid = await VerifyCaptcha(captchaResponse);

            if (!captchaIsValid)
            {
                ViewBag.ErrorMessage = "لطفاً کپچا رو تایید کنید.";
                return View();
            }

            var user = _db.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user == null)
            {
                ViewBag.ErrorMessage = "نام کاربری یا رمز عبور اشتباه است.";
                return View();
            }

            await SignInUser(user);

            return RedirectToAction("Index", "Home");
        }

        private async System.Threading.Tasks.Task<bool> VerifyCaptcha(string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                return false;
            }

            string secretKey = _config["Recaptcha:SecretKey"];

            var client = new HttpClient();
            string url = "https://www.google.com/recaptcha/api/siteverify?secret=" + secretKey + "&response=" + response;

            var result = await client.GetStringAsync(url);
        System.Console.WriteLine("Google response: " + result);
            return result.Contains("\"success\": true") || result.Contains("\"success\":true");
        }

        [HttpPost]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties();
            properties.RedirectUri = Url.Action("GoogleResponse");

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async System.Threading.Tasks.Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync("External");

            if (!result.Succeeded)
            {
                return RedirectToAction("Login");
            }

            string email = result.Principal.FindFirst(ClaimTypes.Email).Value;
            string name = result.Principal.FindFirst(ClaimTypes.Name).Value;

            var user = _db.Users.FirstOrDefault(u => u.Username == email);

            if (user == null)
            {
                user = new Users();
                user.Name = name;
                user.Username = email;
                user.Password = "GOOGLE_LOGIN";

                _db.Users.Add(user);
                _db.SaveChanges();
            }

            await SignInUser(user);
            await HttpContext.SignOutAsync("External");

            return RedirectToAction("Index", "Home");
        }

        public async System.Threading.Tasks.Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        private async System.Threading.Tasks.Task SignInUser(Users user)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.Name));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
    }
}