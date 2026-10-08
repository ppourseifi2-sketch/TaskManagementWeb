using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagementWeb.Data;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _db;

        public TaskService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<TasksIndexViewModel> GetTasksIndexAsync(int projectId, int userId, string statusFilter, CancellationToken cancellationToken)
        {
            string role = await RoleHelper.GetRoleAsync(_db, projectId, userId, cancellationToken);

            if (role == null)
            {
                return null;
            }

            string projectTitle = await _db.Projects
                .Where(p => p.Id == projectId)
                .Select(p => p.Title)
                .FirstOrDefaultAsync(cancellationToken);

            var tasksQuery = _db.Tasks.Where(t => t.ProjectId == projectId);

            if (!string.IsNullOrEmpty(statusFilter))
            {
                tasksQuery = tasksQuery.Where(t => t.Status == statusFilter);
            }

            // Left Join: تسکی که مسئول نداره هم تو لیست می‌مونه
            var rows = await (from t in tasksQuery
                              join u in _db.Users on t.AssignedUserId equals (int?)u.Id into assignedUsers
                              from assignedUser in assignedUsers.DefaultIfEmpty()
                              orderby t.Id
                              select new TaskRow
                              {
                                  Id = t.Id,
                                  Title = t.Title,
                                  Status = t.Status,
                                  AssignedUserName = assignedUser == null ? "—" : assignedUser.Name
                              }).ToListAsync(cancellationToken);

            var viewModel = new TasksIndexViewModel
            {
                ProjectId = projectId,
                ProjectTitle = projectTitle,
                MyRole = role,
                StatusFilter = statusFilter,
                Tasks = rows
            };

            return viewModel;
        }

        public async Task DeleteTaskAsync(int projectId, int taskId, int userId, CancellationToken cancellationToken)
        {
            string role = await RoleHelper.GetRoleAsync(_db, projectId, userId, cancellationToken);

            if (!RoleHelper.CanEditTasks(role))
            {
                return;
            }

            // شرط ProjectId باعث می‌شه فقط تسک‌های همین پروژه قابل حذف باشن
            var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId, cancellationToken);

            if (task == null)
            {
                return;
            }

            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<TasksEditViewModel> GetTaskForEditAsync(int projectId, int? taskId, int userId, CancellationToken cancellationToken)
        {
            string role = await RoleHelper.GetRoleAsync(_db, projectId, userId, cancellationToken);

            if (!RoleHelper.CanEditTasks(role))
            {
                return null;
            }

            var projectUsers = await (from m in _db.ProjectMembers
                                      join u in _db.Users on m.UserId equals u.Id
                                      where m.ProjectId == projectId
                                      select u).AsNoTracking().ToListAsync(cancellationToken);

            ProjectTask task;

            if (taskId == null)
            {
                task = new ProjectTask();
            }
            else
            {
                task = await _db.Tasks
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == taskId.Value && t.ProjectId == projectId, cancellationToken);

                if (task == null)
                {
                    return null;
                }
            }

            var viewModel = new TasksEditViewModel
            {
                ProjectId = projectId,
                Task = task,
                ProjectUsers = projectUsers
            };

            return viewModel;
        }

        public async Task SaveTaskAsync(int projectId, int? taskId, int userId, string title, string description, string status, int? assignedUserId, CancellationToken cancellationToken)
        {
            string role = await RoleHelper.GetRoleAsync(_db, projectId, userId, cancellationToken);

            if (!RoleHelper.CanEditTasks(role))
            {
                return;
            }

            if (taskId == null)
            {
                var newTask = new ProjectTask
                {
                    Title = title,
                    Description = description,
                    Status = status,
                    ProjectId = projectId,
                    AssignedUserId = assignedUserId
                };

                _db.Tasks.Add(newTask);
            }
            else
            {
                var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId.Value && t.ProjectId == projectId, cancellationToken);

                if (task == null)
                {
                    return;
                }

                task.Title = title;
                task.Description = description;
                task.Status = status;
                task.AssignedUserId = assignedUserId;
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}