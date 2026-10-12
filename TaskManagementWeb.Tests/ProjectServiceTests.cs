using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagementWeb.Services;

namespace TaskManagementWeb.Tests
{
    public class ProjectServiceTests : IDisposable
    {
        private readonly TestDatabase _db;
        private readonly ProjectService _service;

        public ProjectServiceTests()
        {
            _db = new TestDatabase();
            _service = new ProjectService(_db.Context);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public async Task CreateProjectAsync_CreatesProjectAndOwnerMembership()
        {
            // Arrange
            var ali = _db.AddUser("Ali");

            // Act
            await _service.CreateProjectAsync("Project 1", "Description", ali.Id, CancellationToken.None);

            // Assert
            var project = _db.Context.Projects.AsNoTracking().Single();
            var membership = _db.Context.ProjectMembers.AsNoTracking().Single();

            Assert.Equal("Project 1", project.Title);
            Assert.Equal(project.Id, membership.ProjectId);
            Assert.Equal(ali.Id, membership.UserId);
            Assert.Equal("Owner", membership.Role);
        }

        [Fact]
        public async Task GetMyProjectsAsync_ReturnsOnlyProjectsOfThatUser()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");

            var aliProject = _db.AddProject("Ali Project", ali.Id);
            var saraProject = _db.AddProject("Sara Project", sara.Id);

            _db.AddMember(aliProject.Id, ali.Id, "Owner");
            _db.AddMember(saraProject.Id, sara.Id, "Owner");

            var rows = await _service.GetMyProjectsAsync(ali.Id, CancellationToken.None);

            Assert.Single(rows);
            Assert.Equal("Ali Project", rows[0].Title);
            Assert.Equal("Owner", rows[0].Role);
        }

        [Fact]
        public async Task GetProjectForEditAsync_ByNonMember_ReturnsNull()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");

            var result = await _service.GetProjectForEditAsync(project.Id, sara.Id, CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateProjectAsync_ByOwner_ChangesTitleAndDescription()
        {
            var ali = _db.AddUser("Ali");

            var project = _db.AddProject("Old", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");

            await _service.UpdateProjectAsync(project.Id, ali.Id, "New", "New description", CancellationToken.None);

            var saved = _db.Context.Projects.AsNoTracking().Single();
            Assert.Equal("New", saved.Title);
            Assert.Equal("New description", saved.Description);
        }

        [Fact]
        public async Task UpdateProjectAsync_ByMember_DoesNotChangeAnything()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");

            var project = _db.AddProject("Old", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");
            _db.AddMember(project.Id, sara.Id, "Member");

            await _service.UpdateProjectAsync(project.Id, sara.Id, "Hacked", "Hacked", CancellationToken.None);

            var saved = _db.Context.Projects.AsNoTracking().Single();
            Assert.Equal("Old", saved.Title);
        }

        [Fact]
        public async Task DeleteProjectAsync_ByOwner_DeletesProjectTasksAndMembers()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");
            _db.AddMember(project.Id, sara.Id, "Member");
            _db.AddTask(project.Id, "Task 1", "To Do");

            await _service.DeleteProjectAsync(project.Id, ali.Id, CancellationToken.None);

            Assert.Empty(_db.Context.Projects.AsNoTracking().ToList());
            Assert.Empty(_db.Context.Tasks.AsNoTracking().ToList());
            Assert.Empty(_db.Context.ProjectMembers.AsNoTracking().ToList());
        }

        [Fact]
        public async Task DeleteProjectAsync_ByAdmin_DoesNothing()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");
            _db.AddMember(project.Id, sara.Id, "Admin");
            _db.AddTask(project.Id, "Task 1", "To Do");

            await _service.DeleteProjectAsync(project.Id, sara.Id, CancellationToken.None);

            Assert.Single(_db.Context.Projects.AsNoTracking().ToList());
            Assert.Single(_db.Context.Tasks.AsNoTracking().ToList());
            Assert.Equal(2, _db.Context.ProjectMembers.AsNoTracking().Count());
        }
    }
}