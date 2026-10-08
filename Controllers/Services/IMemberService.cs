using System.Threading;
using System.Threading.Tasks;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services
{
    public interface IMemberService
    {
        Task<MembersIndexViewModel> GetMembersIndexAsync(int projectId, int userId, CancellationToken cancellationToken);
        Task ChangeRoleAsync(int projectId, int memberId, int userId, string newRole, CancellationToken cancellationToken);
        Task RemoveMemberAsync(int projectId, int memberId, int userId, CancellationToken cancellationToken);
        Task<MembersAddViewModel> GetMembersAddAsync(int projectId, int userId, CancellationToken cancellationToken);
        Task AddMemberAsync(int projectId, int currentUserId, int newUserId, string role, CancellationToken cancellationToken);
    }
}