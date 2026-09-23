using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementWeb.Data;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly AppDbContext _db;

        public ProjectsController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
{
    int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

    var memberships = _db.ProjectMembers.Where(m => m.UserId == userId).ToList();
    
            var rows = new List<ProjectRow>();

            foreach (var m in memberships)
            {
                var project = _db.Projects.Find(m.ProjectId);

                var row = new ProjectRow();
                row.Id = project.Id;
                row.Title = project.Title;
                row.Description = project.Description;
                row.Role = m.Role;

                rows.Add(row);
            }

            return View(rows);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(string title, string description)
        {
            int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var project = new Projects();
            project.Title = title;
            project.Description = description;
            project.OwnerId = userId;
            project.CreatedAt = System.DateTime.Now;

            _db.Projects.Add(project);
            _db.SaveChanges();

            var membership = new ProjectMember();
            membership.ProjectId = project.Id;
            membership.UserId = userId;
            membership.Role = "Owner";

            _db.ProjectMembers.Add(membership);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var membership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == id && m.UserId == userId);

            if (membership == null || membership.Role != "Owner")
            {
                return RedirectToAction("Index");
            }

            var project = _db.Projects.Find(id);

            return View(project);
        }

        [HttpPost]
        public IActionResult Edit(int projectId, string title, string description)
        {
            int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var membership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == projectId && m.UserId == userId);

            if (membership == null || membership.Role != "Owner")
            {
                return RedirectToAction("Index");
            }

            var project = _db.Projects.Find(projectId);
            project.Title = title;
            project.Description = description;

            _db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}