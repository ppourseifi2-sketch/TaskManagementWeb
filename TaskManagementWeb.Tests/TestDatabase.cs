using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskManagementWeb.Data;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Tests
{
    public class TestDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;

        public AppDbContext Context { get; }

        public TestDatabase()
        {
            // دیتابیس تو حافظه؛ تا وقتی Connection بازه وجود داره
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            Context = new AppDbContext(options);
            Context.Database.EnsureCreated();
        }

        public Users AddUser(string name)
        {
            var user = new Users
            {
                Name = name,
                Username = name.ToLower(),
                Password = "123"
            };

            Context.Users.Add(user);
            Context.SaveChanges();

            return user;
        }

        public Projects AddProject(string title, int ownerId)
        {
            var project = new Projects
            {
                Title = title,
                Description = "Description",
                OwnerId = ownerId,
                CreatedAt = DateTime.Now
            };

            Context.Projects.Add(project);
            Context.SaveChanges();

            return project;
        }

        public ProjectMember AddMember(int projectId, int userId, string role)
        {
            var member = new ProjectMember
            {
                ProjectId = projectId,
                UserId = userId,
                Role = role
            };

            Context.ProjectMembers.Add(member);
            Context.SaveChanges();

            return member;
        }

        public ProjectTask AddTask(int projectId, string title, string status, int? assignedUserId = null)
        {
            var task = new ProjectTask
            {
                Title = title,
                Description = "Description",
                Status = status,
                ProjectId = projectId,
                AssignedUserId = assignedUserId
            };

            Context.Tasks.Add(task);
            Context.SaveChanges();

            return task;
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
        }
    }
}