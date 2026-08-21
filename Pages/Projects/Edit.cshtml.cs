using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TaskManagementWeb.Data;

namespace TaskManagementWeb.Pages.Projects
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _db;

        public EditModel(AppDbContext db)
        {
            _db = db;
        }

        public int ProjectId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // چک می‌کنیم این کاربر واقعاً Owner همین پروژه هست
            var membership = await _db.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == id && m.UserId == userId);

            if (membership == null || membership.Role != "Owner")
            {
                return RedirectToPage("/Projects/Index");
            }

            var project = await _db.Projects.FindAsync(id);

            ProjectId = project.Id;
            Title = project.Title;
            Description = project.Description;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int projectId, string title, string description)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            var membership = await _db.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);

            if (membership == null || membership.Role != "Owner")
            {
                return RedirectToPage("/Projects/Index");
            }

            var project = await _db.Projects.FindAsync(projectId);
            project.Title = title;
            project.Description = description;

            await _db.SaveChangesAsync();

            return RedirectToPage("/Projects/Index");
        }
    }
}