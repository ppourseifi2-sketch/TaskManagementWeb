using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task<IActionResult> Index(int id, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var viewModel = await _memberService.GetMembersIndexAsync(id, userId, cancellationToken);

            if (viewModel == null)
            {
                return RedirectToAction("Index", "Tasks", new { id = id });
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(int projectId, int memberId, string newRole, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            await _memberService.ChangeRoleAsync(projectId, memberId, userId, newRole, cancellationToken);

            return RedirectToAction("Index", new { id = projectId });
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int projectId, int memberId, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            await _memberService.RemoveMemberAsync(projectId, memberId, userId, cancellationToken);

            return RedirectToAction("Index", new { id = projectId });
        }

        public async Task<IActionResult> Add(int id, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var viewModel = await _memberService.GetMembersAddAsync(id, userId, cancellationToken);

            if (viewModel == null)
            {
                return RedirectToAction("Index", "Tasks", new { id = id });
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int projectId, int userId, string role, CancellationToken cancellationToken)
        {
            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            await _memberService.AddMemberAsync(projectId, currentUserId, userId, role, cancellationToken);

            return RedirectToAction("Index", new { id = projectId });
        }
    }
}