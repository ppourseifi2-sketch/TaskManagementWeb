using System.Collections.Generic;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services
{
    public interface IProjectService
    {
        List<ProjectRow> GetMyProjects(int userId);
        void CreateProject(string title, string description, int ownerId);
        Projects GetProjectForEdit(int projectId, int userId);
        void UpdateProject(int projectId, int userId, string title, string description);
    }
}