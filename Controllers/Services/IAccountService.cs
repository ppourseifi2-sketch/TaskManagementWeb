using System.Threading;
using System.Threading.Tasks;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services
{
    public interface IAccountService
    {
        Task<Users> ValidateUserAsync(string username, string password, CancellationToken cancellationToken);
        Task<bool> VerifyCaptchaAsync(string captchaResponse, CancellationToken cancellationToken);
        Task<Users> GetOrCreateGoogleUserAsync(string email, string name, CancellationToken cancellationToken);
    }
}