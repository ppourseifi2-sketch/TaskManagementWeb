using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TaskManagementWeb.Data;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Controllers
{
    public class TasksController : Controller
    {
        private readonly AppDbContext _db;

        public TasksController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(int id, string statusFilter)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var membership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == id && m.UserId == userId);

            if (membership == null)
            {
                return RedirectToAction("Index", "Projects");
            }

            var project = _db.Projects.Find(id);

            List<ProjectTask> tasks;

            if (string.IsNullOrEmpty(statusFilter))
            {
                tasks = _db.Tasks.Where(t => t.ProjectId == id).ToList();
            }
            else
            {
                tasks = _db.Tasks.Where(t => t.ProjectId == id && t.Status == statusFilter).ToList();
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

            ViewBag.ProjectId = id;
            ViewBag.ProjectTitle = project.Title;
            ViewBag.MyRole = membership.Role;
            ViewBag.StatusFilter = statusFilter;

            return View(rows);
        }

        [HttpPost]
        public IActionResult Delete(int projectId, int taskId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var membership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == projectId && m.UserId == userId);

            if (membership == null || (membership.Role != "Owner" && membership.Role != "Admin"))
            {
                return RedirectToAction("Index", new { id = projectId });
            }

            var task = _db.Tasks.Find(taskId);
            _db.Tasks.Remove(task);
            _db.SaveChanges();

            return RedirectToAction("Index", new { id = projectId });
        }

        public IActionResult Edit(int projectId, int? id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var membership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == projectId && m.UserId == userId);

            if (membership == null || (membership.Role != "Owner" && membership.Role != "Admin"))
            {
                return RedirectToAction("Index", new { id = projectId });
            }

            var memberships = _db.ProjectMembers.Where(m => m.ProjectId == projectId).ToList();
            var projectUsers = new List<Users>();

            foreach (var m in memberships)
            {
                var user = _db.Users.Find(m.UserId);
                projectUsers.Add(user);
            }

            ViewBag.ProjectId = projectId;
            ViewBag.ProjectUsers = projectUsers;

            if (id != null)
            {
                var task = _db.Tasks.Find(id);
                return View(task);
            }

            return View(new ProjectTask());
        }

        [HttpPost]
        public IActionResult Edit(int projectId, int? taskId, string title, string description, string status, int? assignedUserId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var membership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == projectId && m.UserId == userId);

            if (membership == null || (membership.Role != "Owner" && membership.Role != "Admin"))
            {
                return RedirectToAction("Index", new { id = projectId });
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

            return RedirectToAction("Index", new { id = projectId });
        }
    }
}