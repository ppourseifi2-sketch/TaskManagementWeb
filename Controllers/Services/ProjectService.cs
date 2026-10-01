using System.Collections.Generic;
using System.Linq;
using TaskManagementWeb.Data;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _db;

        public ProjectService(AppDbContext db)
        {
            _db = db;
        }

        public List<ProjectRow> GetMyProjects(int userId)
        {
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

            return rows;
        }

        public void CreateProject(string title, string description, int ownerId)
        {
            var project = new Projects();
            project.Title = title;
            project.Description = description;
            project.OwnerId = ownerId;
            project.CreatedAt = System.DateTime.Now;

            _db.Projects.Add(project);
            _db.SaveChanges();

            var membership = new ProjectMember();
            membership.ProjectId = project.Id;
            membership.UserId = ownerId;
            membership.Role = "Owner";

            _db.ProjectMembers.Add(membership);
            _db.SaveChanges();
        }

        public Projects GetProjectForEdit(int projectId, int userId)
        {
            var membership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == projectId && m.UserId == userId);

            if (membership == null || membership.Role != "Owner")
            {
                return null;
            }

            return _db.Projects.Find(projectId);
        }

        public void UpdateProject(int projectId, int userId, string title, string description)
        {
            var membership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == projectId && m.UserId == userId);

            if (membership == null || membership.Role != "Owner")
            {
                return;
            }

            var project = _db.Projects.Find(projectId);
            project.Title = title;
            project.Description = description;

            _db.SaveChanges();
        }
    }
}