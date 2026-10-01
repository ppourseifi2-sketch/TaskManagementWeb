using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementWeb.Services;

namespace TaskManagementWeb.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        public IActionResult Index(int id, string statusFilter)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var viewModel = _taskService.GetTasksIndex(id, userId, statusFilter);

            if (viewModel == null)
            {
                return RedirectToAction("Index", "Projects");
            }

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Delete(int projectId, int taskId)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            _taskService.DeleteTask(projectId, taskId, userId);

            return RedirectToAction("Index", new { id = projectId });
        }

        public IActionResult Edit(int projectId, int? id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var viewModel = _taskService.GetTaskForEdit(projectId, id, userId);

            if (viewModel == null)
            {
                return RedirectToAction("Index", new { id = projectId });
            }

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Edit(int projectId, int? taskId, string title, string description, string status, int? assignedUserId)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            _taskService.SaveTask(projectId, taskId, userId, title, description, status, assignedUserId);

            return RedirectToAction("Index", new { id = projectId });
        }
    }
}