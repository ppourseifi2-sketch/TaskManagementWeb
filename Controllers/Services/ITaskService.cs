using System.Threading;
using System.Threading.Tasks;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services
{
    public interface ITaskService
    {
        Task<TasksIndexViewModel> GetTasksIndexAsync(int projectId, int userId, string statusFilter, CancellationToken cancellationToken);
        Task DeleteTaskAsync(int projectId, int taskId, int userId, CancellationToken cancellationToken);
        Task<TasksEditViewModel> GetTaskForEditAsync(int projectId, int? taskId, int userId, CancellationToken cancellationToken);
        Task SaveTaskAsync(int projectId, int? taskId, int userId, string title, string description, string status, int? assignedUserId, CancellationToken cancellationToken);
    }
}