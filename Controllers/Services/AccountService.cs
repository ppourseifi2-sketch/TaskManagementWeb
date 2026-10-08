using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskManagementWeb.Data;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountService(AppDbContext db, IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Users> ValidateUserAsync(string username, string password, CancellationToken cancellationToken)
        {
            return await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password, cancellationToken);
        }

        public async Task<bool> VerifyCaptchaAsync(string captchaResponse, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(captchaResponse))
            {
                return false;
            }

            try
            {
                var client = _httpClientFactory.CreateClient();

                var formData = new Dictionary<string, string>
                {
                    { "secret", _config["Recaptcha:SecretKey"] },
                    { "response", captchaResponse }
                };

                var response = await client.PostAsync(
                    "https://www.google.com/recaptcha/api/siteverify",
                    new FormUrlEncodedContent(formData),
                    cancellationToken);

                string json = await response.Content.ReadAsStringAsync(cancellationToken);

                using var document = JsonDocument.Parse(json);

                return document.RootElement.TryGetProperty("success", out var success)
                    && success.ValueKind == JsonValueKind.True;
            }
            catch (HttpRequestException)
            {
                // مثلاً اینترنت قطعه
                return false;
            }
        }

        public async Task<Users> GetOrCreateGoogleUserAsync(string email, string name, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == email, cancellationToken);

            if (user == null)
            {
                user = new Users
                {
                    Name = name,
                    Username = email,
                    Password = "GOOGLE_LOGIN"
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync(cancellationToken);
            }

            return user;
        }
    }
}