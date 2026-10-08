using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public async Task<List<ProjectRow>> GetMyProjectsAsync(int userId, CancellationToken cancellationToken)
        {
            return await (from m in _db.ProjectMembers
                          join p in _db.Projects on m.ProjectId equals p.Id
                          where m.UserId == userId
                          select new ProjectRow
                          {
                              Id = p.Id,
                              Title = p.Title,
                              Description = p.Description,
                              Role = m.Role
                          }).ToListAsync(cancellationToken);
        }

        public async Task CreateProjectAsync(string title, string description, int ownerId, CancellationToken cancellationToken)
        {
            // Transaction: یا هر دو کار انجام می‌شه یا هیچ‌کدوم
            using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            var project = new Projects
            {
                Title = title,
                Description = description,
                OwnerId = ownerId,
                CreatedAt = DateTime.Now
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync(cancellationToken);

            var membership = new ProjectMember
            {
                ProjectId = project.Id,
                UserId = ownerId,
                Role = "Owner"
            };

            _db.ProjectMembers.Add(membership);
            await _db.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        public async Task<Projects> GetProjectForEditAsync(int projectId, int userId, CancellationToken cancellationToken)
        {
            string role = await RoleHelper.GetRoleAsync(_db, projectId, userId, cancellationToken);

            if (!RoleHelper.IsOwner(role))
            {
                return null;
            }

            return await _db.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
        }

        public async Task UpdateProjectAsync(int projectId, int userId, string title, string description, CancellationToken cancellationToken)
        {
            string role = await RoleHelper.GetRoleAsync(_db, projectId, userId, cancellationToken);

            if (!RoleHelper.IsOwner(role))
            {
                return;
            }

            var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

            if (project == null)
            {
                return;
            }

            project.Title = title;
            project.Description = description;

            await _db.SaveChangesAsync(cancellationToken);
        }
        public async Task DeleteProjectAsync(int projectId, int userId, CancellationToken cancellationToken)
{
    string role = await RoleHelper.GetRoleAsync(_db, projectId, userId, cancellationToken);

    if (!RoleHelper.IsOwner(role))
    {
        return;
    }

    var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

    if (project == null)
    {
        return;
    }

    var tasks = await _db.Tasks.Where(t => t.ProjectId == projectId).ToListAsync(cancellationToken);
    var members = await _db.ProjectMembers.Where(m => m.ProjectId == projectId).ToListAsync(cancellationToken);

    _db.Tasks.RemoveRange(tasks);
    _db.ProjectMembers.RemoveRange(members);
    _db.Projects.Remove(project);

    // یه SaveChanges: یا همه‌ی حذف‌ها انجام می‌شه یا هیچ‌کدوم
    await _db.SaveChangesAsync(cancellationToken);
}
    }
}