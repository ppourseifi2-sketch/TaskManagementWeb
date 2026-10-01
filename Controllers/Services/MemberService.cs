using System.Collections.Generic;
using System.Linq;
using TaskManagementWeb.Data;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services
{
    public class MemberService : IMemberService
    {
        private readonly AppDbContext _db;

        public MemberService(AppDbContext db)
        {
            _db = db;
        }

        public MembersIndexViewModel GetMembersIndex(int projectId, int userId)
        {
            var myMembership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == projectId && m.UserId == userId);

            if (myMembership == null || myMembership.Role != "Owner")
            {
                return null;
            }

            var allMembers = _db.ProjectMembers.Where(m => m.ProjectId == projectId).ToList();
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

            var viewModel = new MembersIndexViewModel();
            viewModel.ProjectId = projectId;
            viewModel.Members = rows;

            return viewModel;
        }

        public void ChangeRole(int projectId, int memberId, int userId, string newRole)
        {
            var myMembership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == projectId && m.UserId == userId);

            if (myMembership == null || myMembership.Role != "Owner")
            {
                return;
            }

            var member = _db.ProjectMembers.Find(memberId);
            member.Role = newRole;
            _db.SaveChanges();
        }

        public void RemoveMember(int projectId, int memberId, int userId)
        {
            var myMembership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == projectId && m.UserId == userId);

            if (myMembership == null || myMembership.Role != "Owner")
            {
                return;
            }

            var member = _db.ProjectMembers.Find(memberId);
            _db.ProjectMembers.Remove(member);
            _db.SaveChanges();
        }

        public MembersAddViewModel GetMembersAdd(int projectId, int userId)
        {
            var myMembership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == projectId && m.UserId == userId);

            if (myMembership == null || myMembership.Role != "Owner")
            {
                return null;
            }

            var existingMembers = _db.ProjectMembers.Where(m => m.ProjectId == projectId).ToList();
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

            var viewModel = new MembersAddViewModel();
            viewModel.ProjectId = projectId;
            viewModel.AvailableUsers = availableUsers;

            return viewModel;
        }

        public void AddMember(int projectId, int currentUserId, int newUserId, string role)
        {
            var myMembership = _db.ProjectMembers.FirstOrDefault(m => m.ProjectId == projectId && m.UserId == currentUserId);

            if (myMembership == null || myMembership.Role != "Owner")
            {
                return;
            }

            var newMember = new ProjectMember();
            newMember.ProjectId = projectId;
            newMember.UserId = newUserId;
            newMember.Role = role;

            _db.ProjectMembers.Add(newMember);
            _db.SaveChanges();
        }
    }
}