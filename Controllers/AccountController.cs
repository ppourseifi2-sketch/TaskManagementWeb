using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TaskManagementWeb.Models;
using TaskManagementWeb.Services;

namespace TaskManagementWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IConfiguration _config;

        public AccountController(IAccountService accountService, IConfiguration config)
        {
            _accountService = accountService;
            _config = config;
        }

        public IActionResult Login()
        {
            ViewBag.RecaptchaSiteKey = _config["Recaptcha:SiteKey"];
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, CancellationToken cancellationToken)
        {
            ViewBag.RecaptchaSiteKey = _config["Recaptcha:SiteKey"];

            string captchaResponse = Request.Form["g-recaptcha-response"];
            bool captchaIsValid = await _accountService.VerifyCaptchaAsync(captchaResponse, cancellationToken);

            if (!captchaIsValid)
            {
                ViewBag.ErrorMessage = "لطفاً کپچا رو تایید کنید.";
                return View();
            }

            var user = await _accountService.ValidateUserAsync(username, password, cancellationToken);

            if (user == null)
            {
                ViewBag.ErrorMessage = "نام کاربری یا رمز عبور اشتباه است.";
                return View();
            }

            await SignInUser(user);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties();
            properties.RedirectUri = Url.Action("GoogleResponse");

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse(CancellationToken cancellationToken)
        {
            var result = await HttpContext.AuthenticateAsync("External");

            if (!result.Succeeded)
            {
                return RedirectToAction("Login");
            }

            string email = result.Principal.FindFirst(ClaimTypes.Email).Value;
            string name = result.Principal.FindFirst(ClaimTypes.Name).Value;

            var user = await _accountService.GetOrCreateGoogleUserAsync(email, name, cancellationToken);

            await SignInUser(user);
            await HttpContext.SignOutAsync("External");

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        private async Task SignInUser(Users user)
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