using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TaskManagementWeb.Data;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Pages.Tasks
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _db;

        public EditModel(AppDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public int ProjectId { get; set; }

        [BindProperty]
        public int? TaskId { get; set; }

        [BindProperty]
        public string Title { get; set; }

        [BindProperty]
        public string Description { get; set; }

        [BindProperty]
        public string Status { get; set; }

        [BindProperty]
        public int? AssignedUserId { get; set; }

        public List<Users> ProjectUsers { get; set; } = new List<Users>();

        public async Task<IActionResult> OnGetAsync(int projectId, int? id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            ProjectId = projectId;
            TaskId = id;
            Status = "To Do"; // مقدار پیش‌فرض برای تسک جدید

            // اعضای این پروژه رو می‌گیریم، تا تو دراپ‌داون «مسئول تسک» نشونشون بدیم
            var memberships = await _db.ProjectMembers
                .Where(m => m.ProjectId == projectId)
                .ToListAsync();

            foreach (var m in memberships)
            {
                var user = await _db.Users.FindAsync(m.UserId);
                if (user != null)
                {
                    ProjectUsers.Add(user);
                }
            }

            // اگه id فرستاده شده بود، یعنی داریم ویرایش می‌کنیم، پس اطلاعات فعلی تسک رو می‌خونیم
            if (id != null)
            {
                var task = await _db.Tasks.FindAsync(id);
                if (task != null)
                {
                    Title = task.Title;
                    Description = task.Description;
                    Status = task.Status;
                    AssignedUserId = task.AssignedUserId;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            if (TaskId == null)
            {
                // تسک جدید می‌سازیم
                var newTask = new ProjectTask
                {
                    Title = Title,
                    Description = Description,
                    Status = Status,
                    ProjectId = ProjectId,
                    AssignedUserId = AssignedUserId
                };
                _db.Tasks.Add(newTask);
            }
            else
            {
                // تسک موجود رو ویرایش می‌کنیم
                var task = await _db.Tasks.FindAsync(TaskId);
                task.Title = Title;
                task.Description = Description;
                task.Status = Status;
                task.AssignedUserId = AssignedUserId;
            }

            await _db.SaveChangesAsync();

            return RedirectToPage("/Tasks/Index", new { projectId = ProjectId });
        }
    }
}