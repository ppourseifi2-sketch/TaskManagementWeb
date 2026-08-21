using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TaskManagementWeb.Data;

namespace TaskManagementWeb.Pages.Members
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;

        public IndexModel(AppDbContext db)
        {
            _db = db;
        }

        public class MemberRow
        {
            public int MemberId { get; set; }
            public string UserName { get; set; }
            public string Role { get; set; }
        }

        public int ProjectId { get; set; }
        public List<MemberRow> MemberRows { get; set; } = new List<MemberRow>();

        public async Task<IActionResult> OnGetAsync(int projectId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // فقط Owner اجازه داره اینجا باشه
            var myMembership = await _db.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);

            if (myMembership == null || myMembership.Role != "Owner")
            {
                return RedirectToPage("/Tasks/Index", new { projectId });
            }

            ProjectId = projectId;

            var allMembers = await _db.ProjectMembers
                .Where(m => m.ProjectId == projectId)
                .ToListAsync();

            foreach (var m in allMembers)
            {
                var user = await _db.Users.FindAsync(m.UserId);

                MemberRows.Add(new MemberRow
                {
                    MemberId = m.Id,
                    UserName = user.Name,
                    Role = m.Role
                });
            }

            return Page();
        }

        // این متد وقتی صدا زده میشه که فرم "تغییر نقش" یه عضو خاص فرستاده بشه
        public async Task<IActionResult> OnPostChangeRoleAsync(int projectId, int memberId, string newRole)
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

            var member = await _db.ProjectMembers.FindAsync(memberId);
            member.Role = newRole;
            await _db.SaveChangesAsync();

            return RedirectToPage(new { projectId });
        }

        // این متد وقتی صدا زده میشه که دکمه‌ی "حذف" یه عضو خاص زده بشه
        public async Task<IActionResult> OnPostRemoveAsync(int projectId, int memberId)
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

            var member = await _db.ProjectMembers.FindAsync(memberId);
            _db.ProjectMembers.Remove(member);
            await _db.SaveChangesAsync();

            return RedirectToPage(new { projectId });
        }
    }
}