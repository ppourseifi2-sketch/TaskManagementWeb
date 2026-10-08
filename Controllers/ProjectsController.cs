using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementWeb.Services;

namespace TaskManagementWeb.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var rows = await _projectService.GetMyProjectsAsync(userId, cancellationToken);

            return View(rows);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string title, string description, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            await _projectService.CreateProjectAsync(title, description, userId, cancellationToken);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var project = await _projectService.GetProjectForEditAsync(id, userId, cancellationToken);

            if (project == null)
            {
                return RedirectToAction("Index");
            }

            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int projectId, string title, string description, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            await _projectService.UpdateProjectAsync(projectId, userId, title, description, cancellationToken);

            return RedirectToAction("Index");
        }
        [HttpPost]
public async Task<IActionResult> Delete(int projectId, CancellationToken cancellationToken)
{
    int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

    await _projectService.DeleteProjectAsync(projectId, userId, cancellationToken);

    return RedirectToAction("Index");
}
    }
}