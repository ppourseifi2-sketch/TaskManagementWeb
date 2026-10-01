using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services
{
    public interface IMemberService
    {
        MembersIndexViewModel GetMembersIndex(int projectId, int userId);
        void ChangeRole(int projectId, int memberId, int userId, string newRole);
        void RemoveMember(int projectId, int memberId, int userId);
        MembersAddViewModel GetMembersAdd(int projectId, int userId);
        void AddMember(int projectId, int currentUserId, int newUserId, string role);
    }
}