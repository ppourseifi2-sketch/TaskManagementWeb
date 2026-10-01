using System.Collections.Generic;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services
{
    public interface ITaskService
    {
        TasksIndexViewModel GetTasksIndex(int projectId, int userId, string statusFilter);
        void DeleteTask(int projectId, int taskId, int userId);
        TasksEditViewModel GetTaskForEdit(int projectId, int? taskId, int userId);
        void SaveTask(int projectId, int? taskId, int userId, string title, string description, string status, int? assignedUserId);
    }
}