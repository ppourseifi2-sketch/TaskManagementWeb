using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TaskManagementWeb.Data;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AccountService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public Users ValidateUser(string username, string password)
        {
            return _db.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
        }

        public async Task<bool> VerifyCaptcha(string captchaResponse)
        {
            if (string.IsNullOrEmpty(captchaResponse))
            {
                return false;
            }

            string secretKey = _config["Recaptcha:SecretKey"];

            var client = new HttpClient();
            string url = "https://www.google.com/recaptcha/api/siteverify?secret=" + secretKey + "&response=" + captchaResponse;

            var result = await client.GetStringAsync(url);

            return result.Contains("\"success\": true") || result.Contains("\"success\":true");
        }

        public Users GetOrCreateGoogleUser(string email, string name)
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == email);

            if (user == null)
            {
                user = new Users();
                user.Name = name;
                user.Username = email;
                user.Password = "GOOGLE_LOGIN";

                _db.Users.Add(user);
                _db.SaveChanges();
            }

            return user;
        }
    }
}