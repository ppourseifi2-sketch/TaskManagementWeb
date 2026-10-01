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

        public IActionResult Index()
        {
            int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var rows = _projectService.GetMyProjects(userId);

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

            _projectService.CreateProject(title, description, userId);

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var project = _projectService.GetProjectForEdit(id, userId);

            if (project == null)
            {
                return RedirectToAction("Index");
            }

            return View(project);
        }

        [HttpPost]
        public IActionResult Edit(int projectId, string title, string description)
        {
            int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            _projectService.UpdateProject(projectId, userId, title, description);

            return RedirectToAction("Index");
        }
    }
}