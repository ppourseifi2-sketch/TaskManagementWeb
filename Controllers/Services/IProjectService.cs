using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services
{
    public interface IProjectService
    {
        Task<List<ProjectRow>> GetMyProjectsAsync(int userId, CancellationToken cancellationToken);
        Task CreateProjectAsync(string title, string description, int ownerId, CancellationToken cancellationToken);
        Task<Projects> GetProjectForEditAsync(int projectId, int userId, CancellationToken cancellationToken);
        Task UpdateProjectAsync(int projectId, int userId, string title, string description, CancellationToken cancellationToken);
        Task DeleteProjectAsync(int projectId, int userId, CancellationToken cancellationToken);
    }
}