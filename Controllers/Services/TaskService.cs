using System.Collections.Generic;
using System.Linq;
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

        public TasksIndexViewModel GetTasksIndex(int projectId, int userId, string statusFilter)
        {
            var membership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == projectId && m.UserId == userId);

            if (membership == null)
            {
                return null;
            }

            var project = _db.Projects.Find(projectId);

            List<ProjectTask> tasks;

            if (string.IsNullOrEmpty(statusFilter))
            {
                tasks = _db.Tasks.Where(t => t.ProjectId == projectId).ToList();
            }
            else
            {
                tasks = _db.Tasks.Where(t => t.ProjectId == projectId && t.Status == statusFilter).ToList();
            }

            var rows = new List<TaskRow>();

            foreach (var t in tasks)
            {
                var row = new TaskRow();
                row.Id = t.Id;
                row.Title = t.Title;
                row.Status = t.Status;

                if (t.AssignedUserId != null)
                {
                    var assignedUser = _db.Users.Find(t.AssignedUserId);
                    row.AssignedUserName = assignedUser.Name;
                }
                else
                {
                    row.AssignedUserName = "—";
                }

                rows.Add(row);
            }

            var viewModel = new TasksIndexViewModel();
            viewModel.ProjectId = projectId;
            viewModel.ProjectTitle = project.Title;
            viewModel.MyRole = membership.Role;
            viewModel.StatusFilter = statusFilter;
            viewModel.Tasks = rows;

            return viewModel;
        }

        public void DeleteTask(int projectId, int taskId, int userId)
        {
            var membership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == projectId && m.UserId == userId);

            if (membership == null || (membership.Role != "Owner" && membership.Role != "Admin"))
            {
                return;
            }

            var task = _db.Tasks.Find(taskId);
            _db.Tasks.Remove(task);
            _db.SaveChanges();
        }

        public TasksEditViewModel GetTaskForEdit(int projectId, int? taskId, int userId)
        {
            var membership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == projectId && m.UserId == userId);

            if (membership == null || (membership.Role != "Owner" && membership.Role != "Admin"))
            {
                return null;
            }

            var memberships = _db.ProjectMembers.Where(m => m.ProjectId == projectId).ToList();
            var projectUsers = new List<Users>();

            foreach (var m in memberships)
            {
                var user = _db.Users.Find(m.UserId);
                projectUsers.Add(user);
            }

            ProjectTask task;

            if (taskId != null)
            {
                task = _db.Tasks.Find(taskId);
            }
            else
            {
                task = new ProjectTask();
            }

            var viewModel = new TasksEditViewModel();
            viewModel.ProjectId = projectId;
            viewModel.Task = task;
            viewModel.ProjectUsers = projectUsers;

            return viewModel;
        }

        public void SaveTask(int projectId, int? taskId, int userId, string title, string description, string status, int? assignedUserId)
        {
            var membership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == projectId && m.UserId == userId);

            if (membership == null || (membership.Role != "Owner" && membership.Role != "Admin"))
            {
                return;
            }

            if (taskId == null)
            {
                var newTask = new ProjectTask();
                newTask.Title = title;
                newTask.Description = description;
                newTask.Status = status;
                newTask.ProjectId = projectId;
                newTask.AssignedUserId = assignedUserId;

                _db.Tasks.Add(newTask);
            }
            else
            {
                var task = _db.Tasks.Find(taskId);
                task.Title = title;
                task.Description = description;
                task.Status = status;
                task.AssignedUserId = assignedUserId;
            }

            _db.SaveChanges();
        }
    }
}