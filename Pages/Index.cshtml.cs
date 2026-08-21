using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TaskManagementWeb.Pages;

public class IndexModel : PageModel
{
    public string CurrentUserName { get; set; }

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            // لاگین نکرده، بفرستش صفحه‌ی لاگین
            return RedirectToPage("/Login");
        }

        CurrentUserName = HttpContext.Session.GetString("UserName");
        return Page();
    }
}