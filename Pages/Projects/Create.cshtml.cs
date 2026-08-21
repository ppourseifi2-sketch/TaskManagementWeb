using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManagementWeb.Data;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Pages.Projects
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _db;

        public CreateModel(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string title, string description)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            var newProject = new TaskManagementWeb.Models.Projects
            {
                Title = title,
                Description = description,
                OwnerId = userId.Value,
                CreatedAt = DateTime.Now
            };

            _db.Projects.Add(newProject);
            await _db.SaveChangesAsync();

            var ownerMembership = new ProjectMember
            {
                ProjectId = newProject.Id,
                UserId = userId.Value,
                Role = "Owner"
            };

            _db.ProjectMembers.Add(ownerMembership);
            await _db.SaveChangesAsync();

            return RedirectToPage("/Projects/Index");
        }
    }
}