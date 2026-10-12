using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using TaskManagementWeb.Data;
using TaskManagementWeb.Services;
using TaskManagementWeb.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AppDbConnection")));
    builder.Services.AddScoped<IProjectService, ProjectService>();
    builder.Services.AddScoped<ITaskService, TaskService>();
    builder.Services.AddScoped<IMemberService, MemberService>();
    builder.Services.AddScoped<IAccountService, AccountService>();
    builder.Services.AddHttpClient();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
    })
    .AddCookie("External")
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        options.SignInScheme = "External";
        options.CallbackPath = "/signin-google";
    });

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    
    for (int i = 0; i < 10; i++)
    {
        try
        {
            db.Database.EnsureCreated();

            if (!db.Users.Any())
            {
                var admin = new Users();
                admin.Name = "مدیر سیستم";
                admin.Username = "admin";
                admin.Password = "admin123";

                db.Users.Add(admin);
                db.SaveChanges();
            }

            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Database is not ready yet: " + ex.Message);
            Thread.Sleep(3000);
        }
    }
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();