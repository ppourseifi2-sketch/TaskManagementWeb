using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TaskManagementWeb.Data;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Controllers
{
    public class MembersController : Controller
    {
        private readonly AppDbContext _db;

        public MembersController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var myMembership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == id && m.UserId == userId);

            if (myMembership == null || myMembership.Role != "Owner")
            {
                return RedirectToAction("Index", "Tasks", new { id = id });
            }

            var allMembers = _db.ProjectMembers.Where(m => m.ProjectId == id).ToList();
            var rows = new List<MemberRow>();

            foreach (var m in allMembers)
            {
                var user = _db.Users.Find(m.UserId);

                var row = new MemberRow();
                row.MemberId = m.Id;
                row.UserName = user.Name;
                row.Role = m.Role;

                rows.Add(row);
            }

            ViewBag.ProjectId = id;

            return View(rows);
        }

        [HttpPost]
        public IActionResult ChangeRole(int projectId, int memberId, string newRole)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var myMembership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == projectId && m.UserId == userId);

            if (myMembership == null || myMembership.Role != "Owner")
            {
                return RedirectToAction("Index", "Tasks", new { id = projectId });
            }

            var member = _db.ProjectMembers.Find(memberId);
            member.Role = newRole;
            _db.SaveChanges();

            return RedirectToAction("Index", new { id = projectId });
        }

        [HttpPost]
        public IActionResult Remove(int projectId, int memberId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var myMembership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == projectId && m.UserId == userId);

            if (myMembership == null || myMembership.Role != "Owner")
            {
                return RedirectToAction("Index", "Tasks", new { id = projectId });
            }

            var member = _db.ProjectMembers.Find(memberId);
            _db.ProjectMembers.Remove(member);
            _db.SaveChanges();

            return RedirectToAction("Index", new { id = projectId });
        }

        public IActionResult Add(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var myMembership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == id && m.UserId == userId);

            if (myMembership == null || myMembership.Role != "Owner")
            {
                return RedirectToAction("Index", "Tasks", new { id = id });
            }

            var existingMembers = _db.ProjectMembers.Where(m => m.ProjectId == id).ToList();
            var existingUserIds = new List<int>();

            foreach (var m in existingMembers)
            {
                existingUserIds.Add(m.UserId);
            }

            var allUsers = _db.Users.ToList();
            var availableUsers = new List<Users>();

            foreach (var u in allUsers)
            {
                if (!existingUserIds.Contains(u.Id))
                {
                    availableUsers.Add(u);
                }
            }

            ViewBag.ProjectId = id;
            ViewBag.AvailableUsers = availableUsers;

            return View();
        }

        [HttpPost]
        public IActionResult Add(int projectId, int userId, string role)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var myMembership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == projectId && m.UserId == currentUserId);

            if (myMembership == null || myMembership.Role != "Owner")
            {
                return RedirectToAction("Index", "Tasks", new { id = projectId });
            }

            var newMember = new ProjectMember();
            newMember.ProjectId = projectId;
            newMember.UserId = userId;
            newMember.Role = role;

            _db.ProjectMembers.Add(newMember);
            _db.SaveChanges();

            return RedirectToAction("Index", new { id = projectId });
        }
    }
}