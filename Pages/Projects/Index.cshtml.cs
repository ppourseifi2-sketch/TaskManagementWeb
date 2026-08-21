using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TaskManagementWeb.Data;

namespace TaskManagementWeb.Pages.Projects
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;

        public IndexModel(AppDbContext db)
        {
            _db = db;
        }

        // یه کلاس ساده فقط برای نمایش هر ردیف تو جدول
        public class ProjectRow
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Role { get; set; }
        }

        public List<ProjectRow> MyProjects { get; set; } = new List<ProjectRow>();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // اول عضویت‌های کاربر رو پیدا می‌کنیم
            var myMemberships = await _db.ProjectMembers
                .Where(m => m.UserId == userId)
                .ToListAsync();

            // بعد برای هر عضویت، اطلاعات پروژه رو جدا می‌گیریم
            foreach (var membership in myMemberships)
            {
                var project = await _db.Projects.FindAsync(membership.ProjectId);

                if (project != null)
                {
                    MyProjects.Add(new ProjectRow
                    {
                        Id = project.Id,
                        Title = project.Title,
                        Description = project.Description,
                        Role = membership.Role
                    });
                }
            }

            return Page();
        }
    }
}