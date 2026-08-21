using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TaskManagementWeb.Data;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Pages.Members
{
    public class AddModel : PageModel
    {
        private readonly AppDbContext _db;

        public AddModel(AppDbContext db)
        {
            _db = db;
        }

        public int ProjectId { get; set; }
        public List<Users> AvailableUsers { get; set; } = new List<Users>();

        public async Task<IActionResult> OnGetAsync(int projectId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            var myMembership = await _db.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);

            if (myMembership == null || myMembership.Role != "Owner")
            {
                return RedirectToPage("/Tasks/Index", new { projectId });
            }

            ProjectId = projectId;

            // کاربرهایی که هنوز عضو این پروژه نیستن رو پیدا می‌کنیم
            var existingMembers = await _db.ProjectMembers
                .Where(m => m.ProjectId == projectId)
                .ToListAsync();

            var existingUserIds = new List<int>();
            foreach (var m in existingMembers)
            {
                existingUserIds.Add(m.UserId);
            }

            var allUsers = await _db.Users.ToListAsync();

            foreach (var u in allUsers)
            {
                if (!existingUserIds.Contains(u.Id))
                {
                    AvailableUsers.Add(u);
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int projectId, int userId, string role)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToPage("/Login");
            }

            var myMembership = await _db.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == currentUserId);

            if (myMembership == null || myMembership.Role != "Owner")
            {
                return RedirectToPage("/Tasks/Index", new { projectId });
            }

            var newMember = new ProjectMember
            {
                ProjectId = projectId,
                UserId = userId,
                Role = role
            };

            _db.ProjectMembers.Add(newMember);
            await _db.SaveChangesAsync();

            return RedirectToPage("/Members/Index", new { projectId });
        }
    }
}