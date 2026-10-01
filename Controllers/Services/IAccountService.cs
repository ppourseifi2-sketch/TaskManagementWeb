using System.Threading.Tasks;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services
{
    public interface IAccountService
    {
        Users ValidateUser(string username, string password);
        Task<bool> VerifyCaptcha(string captchaResponse);
        Users GetOrCreateGoogleUser(string email, string name);
    }
}