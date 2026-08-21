using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TaskManagementWeb.Data;

namespace TaskManagementWeb.Pages
{
    public class LoginModel : PageModel
    {
        private readonly AppDbContext _db;

        public LoginModel(AppDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            // وقتی صفحه رو باز می‌کنی، فقط فرم خالی نشون داده می‌شه
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _db.Users.FirstOrDefaultAsync(u =>
                u.Username == Username && u.Password == Password);

            if (user == null)
            {
                ErrorMessage = "نام کاربری یا رمز عبور اشتباه است.";
                return Page();
            }

            // کاربر پیدا شد، یادش می‌ذاریم توی Session که لاگین کرده
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);

            return RedirectToPage("/Index");
        }
    }
}