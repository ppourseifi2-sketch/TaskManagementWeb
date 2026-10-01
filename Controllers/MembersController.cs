using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementWeb.Services;

namespace TaskManagementWeb.Controllers
{
    [Authorize]
    public class MembersController : Controller
    {
        private readonly IMemberService _memberService;

        public MembersController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        public IActionResult Index(int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var viewModel = _memberService.GetMembersIndex(id, userId);

            if (viewModel == null)
            {
                return RedirectToAction("Index", "Tasks", new { id = id });
            }

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult ChangeRole(int projectId, int memberId, string newRole)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            _memberService.ChangeRole(projectId, memberId, userId, newRole);

            return RedirectToAction("Index", new { id = projectId });
        }

        [HttpPost]
        public IActionResult Remove(int projectId, int memberId)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            _memberService.RemoveMember(projectId, memberId, userId);

            return RedirectToAction("Index", new { id = projectId });
        }

        public IActionResult Add(int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var viewModel = _memberService.GetMembersAdd(id, userId);

            if (viewModel == null)
            {
                return RedirectToAction("Index", "Tasks", new { id = id });
            }

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Add(int projectId, int userId, string role)
        {
            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            _memberService.AddMember(projectId, currentUserId, userId, role);

            return RedirectToAction("Index", new { id = projectId });
        }
    }
}