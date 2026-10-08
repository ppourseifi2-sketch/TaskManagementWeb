using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task<IActionResult> Index(int id, string statusFilter, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var viewModel = await _taskService.GetTasksIndexAsync(id, userId, statusFilter, cancellationToken);

            if (viewModel == null)
            {
                return RedirectToAction("Index", "Projects");
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int projectId, int taskId, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            await _taskService.DeleteTaskAsync(projectId, taskId, userId, cancellationToken);

            return RedirectToAction("Index", new { id = projectId });
        }

        public async Task<IActionResult> Edit(int projectId, int? id, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var viewModel = await _taskService.GetTaskForEditAsync(projectId, id, userId, cancellationToken);

            if (viewModel == null)
            {
                return RedirectToAction("Index", new { id = projectId });
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int projectId, int? taskId, string title, string description, string status, int? assignedUserId, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            await _taskService.SaveTaskAsync(projectId, taskId, userId, title, description, status, assignedUserId, cancellationToken);

            return RedirectToAction("Index", new { id = projectId });
        }
    }
}