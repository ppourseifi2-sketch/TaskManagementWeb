using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagementWeb.Data;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services
{
    public class MemberService : IMemberService
    {
        private static readonly string[] ValidRoles = { "Member", "Admin", "Owner" };

        private readonly AppDbContext _db;

        public MemberService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<MembersIndexViewModel> GetMembersIndexAsync(int projectId, int userId, CancellationToken cancellationToken)
        {
            string role = await RoleHelper.GetRoleAsync(_db, projectId, userId, cancellationToken);

            if (!RoleHelper.IsOwner(role))
            {
                return null;
            }

            var members = await (from m in _db.ProjectMembers
                                 join u in _db.Users on m.UserId equals u.Id
                                 where m.ProjectId == projectId
                                 select new MemberRow
                                 {
                                     MemberId = m.Id,
                                     UserName = u.Name,
                                     Role = m.Role
                                 }).ToListAsync(cancellationToken);

            var viewModel = new MembersIndexViewModel
            {
                ProjectId = projectId,
                Members = members
            };

            return viewModel;
        }

        public async Task ChangeRoleAsync(int projectId, int memberId, int userId, string newRole, CancellationToken cancellationToken)
        {
            string role = await RoleHelper.GetRoleAsync(_db, projectId, userId, cancellationToken);

            if (!RoleHelper.IsOwner(role) || !ValidRoles.Contains(newRole))
            {
                return;
            }

            var member = await _db.ProjectMembers.FirstOrDefaultAsync(m => m.Id == memberId && m.ProjectId == projectId, cancellationToken);

            if (member == null)
            {
                return;
            }

            member.Role = newRole;
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveMemberAsync(int projectId, int memberId, int userId, CancellationToken cancellationToken)
        {
            string role = await RoleHelper.GetRoleAsync(_db, projectId, userId, cancellationToken);

            if (!RoleHelper.IsOwner(role))
            {
                return;
            }

            var member = await _db.ProjectMembers.FirstOrDefaultAsync(m => m.Id == memberId && m.ProjectId == projectId, cancellationToken);

            if (member == null)
            {
                return;
            }

            _db.ProjectMembers.Remove(member);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<MembersAddViewModel> GetMembersAddAsync(int projectId, int userId, CancellationToken cancellationToken)
        {
            string role = await RoleHelper.GetRoleAsync(_db, projectId, userId, cancellationToken);

            if (!RoleHelper.IsOwner(role))
            {
                return null;
            }

            // کاربرهایی که هنوز عضو این پروژه نیستن (یه کوئری)
            var availableUsers = await _db.Users
                .Where(u => !_db.ProjectMembers.Any(m => m.ProjectId == projectId && m.UserId == u.Id))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var viewModel = new MembersAddViewModel
            {
                ProjectId = projectId,
                AvailableUsers = availableUsers
            };

            return viewModel;
        }

        public async Task AddMemberAsync(int projectId, int currentUserId, int newUserId, string role, CancellationToken cancellationToken)
        {
            string myRole = await RoleHelper.GetRoleAsync(_db, projectId, currentUserId, cancellationToken);

            if (!RoleHelper.IsOwner(myRole) || !ValidRoles.Contains(role))
            {
                return;
            }

            bool alreadyMember = await _db.ProjectMembers
                .AnyAsync(m => m.ProjectId == projectId && m.UserId == newUserId, cancellationToken);

            if (alreadyMember)
            {
                return;
            }

            var newMember = new ProjectMember
            {
                ProjectId = projectId,
                UserId = newUserId,
                Role = role
            };

            _db.ProjectMembers.Add(newMember);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}