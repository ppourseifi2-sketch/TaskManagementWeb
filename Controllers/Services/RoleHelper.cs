using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagementWeb.Data;

namespace TaskManagementWeb.Services
{
    public static class RoleHelper
    {
        public static async Task<string> GetRoleAsync(AppDbContext db, int projectId, int userId, CancellationToken cancellationToken)
        {
            return await db.ProjectMembers
                .Where(m => m.ProjectId == projectId && m.UserId == userId)
                .Select(m => m.Role)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public static bool IsOwner(string role)
        {
            return string.Equals(role, "Owner", StringComparison.OrdinalIgnoreCase);
        }

        public static bool CanEditTasks(string role)
        {
            return IsOwner(role) || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}