using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TaskManagementWeb.Data;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Pages.Tasks
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;

        public IndexModel(AppDbContext db)
        {
            _db = db;
        }

        public class TaskRow
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Status { get; set; }
            public string AssignedUserName { get; set; }
        }

        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; }
        public string MyRole { get; set; }
        public List<TaskRow> TaskRows { get; set; } = new List<TaskRow>();
        public string StatusFilter { get; set; }

      public async Task<IActionResult> OnGetAsync(int projectId, string statusFilter)
        {
            StatusFilter = statusFilter;
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

           
            var membership = await _db.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);

            if (membership == null)
            {
                return RedirectToPage("/Projects/Index");
            }

            ProjectId = projectId;
            MyRole = membership.Role;

            var project = await _db.Projects.FindAsync(projectId);
            ProjectTitle = project.Title;

List<ProjectTask> tasks;

if (string.IsNullOrEmpty(statusFilter))
{
    tasks = await _db.Tasks
        .Where(t => t.ProjectId == projectId)
        .ToListAsync();
}
else
{
   
    tasks = await _db.Tasks
        .Where(t => t.ProjectId == projectId && t.Status == statusFilter)
        .ToListAsync();
}

            foreach (var task in tasks)
            {
                string assignedName = "—";

                if (task.AssignedUserId != null)
                {
                    var assignedUser = await _db.Users.FindAsync(task.AssignedUserId);
                    if (assignedUser != null)
                    {
                        assignedName = assignedUser.Name;
                    }
                }

                TaskRows.Add(new TaskRow
                {
                    Id = task.Id,
                    Title = task.Title,
                    Status = task.Status,
                    AssignedUserName = assignedName
                });
            }

            return Page();
        }
        public async Task<IActionResult> OnPostDeleteAsync(int projectId, int taskId)
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
    {
        return RedirectToPage("/Login");
    }

    
    var membership = await _db.ProjectMembers
        .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);

    if (membership == null || (membership.Role != "Owner" && membership.Role != "Admin"))
    {
        return RedirectToPage(new { projectId });
    }

    var task = await _db.Tasks.FindAsync(taskId);
    if (task != null)
    {
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
    }

    return RedirectToPage(new { projectId });
}
    }
    
}