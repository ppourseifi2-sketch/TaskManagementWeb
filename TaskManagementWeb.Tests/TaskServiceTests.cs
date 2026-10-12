using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagementWeb.Services;

namespace TaskManagementWeb.Tests
{
    public class TaskServiceTests : IDisposable
    {
        private readonly TestDatabase _db;
        private readonly TaskService _service;

        public TaskServiceTests()
        {
            _db = new TestDatabase();
            _service = new TaskService(_db.Context);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        // ---------- GetTasksIndexAsync ----------

        [Fact]
        public async Task GetTasksIndexAsync_ByNonMember_ReturnsNull()
        {
            var ali = _db.AddUser("Ali");
            var reza = _db.AddUser("Reza");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");

            var result = await _service.GetTasksIndexAsync(project.Id, reza.Id, null, CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetTasksIndexAsync_ReturnsAssignedUserNameOrDash()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");
            _db.AddMember(project.Id, sara.Id, "Member");

            _db.AddTask(project.Id, "Task 1", "To Do", sara.Id);
            _db.AddTask(project.Id, "Task 2", "To Do");

            var result = await _service.GetTasksIndexAsync(project.Id, ali.Id, "", CancellationToken.None);

            Assert.Equal("P", result.ProjectTitle);
            Assert.Equal("Owner", result.MyRole);
            Assert.Equal(2, result.Tasks.Count);
            Assert.Equal("Task 1", result.Tasks[0].Title);
            Assert.Equal("Sara", result.Tasks[0].AssignedUserName);
            Assert.Equal("Task 2", result.Tasks[1].Title);
            Assert.Equal("—", result.Tasks[1].AssignedUserName);
        }

        [Fact]
        public async Task GetTasksIndexAsync_WithStatusFilter_ReturnsOnlyMatchingTasks()
        {
            var ali = _db.AddUser("Ali");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");

            _db.AddTask(project.Id, "A", "To Do");
            _db.AddTask(project.Id, "B", "Done");
            _db.AddTask(project.Id, "C", "Done");

            var result = await _service.GetTasksIndexAsync(project.Id, ali.Id, "Done", CancellationToken.None);

            Assert.Equal("Done", result.StatusFilter);
            Assert.Equal(2, result.Tasks.Count);
            Assert.All(result.Tasks, t => Assert.Equal("Done", t.Status));
        }

        [Fact]
        public async Task GetTasksIndexAsync_DoesNotReturnTasksOfOtherProjects()
        {
            var ali = _db.AddUser("Ali");

            var projectA = _db.AddProject("A", ali.Id);
            var projectB = _db.AddProject("B", ali.Id);
            _db.AddMember(projectA.Id, ali.Id, "Owner");
            _db.AddMember(projectB.Id, ali.Id, "Owner");

            _db.AddTask(projectB.Id, "Task of B", "To Do");

            var result = await _service.GetTasksIndexAsync(projectA.Id, ali.Id, "", CancellationToken.None);

            Assert.Empty(result.Tasks);
        }

        // ---------- DeleteTaskAsync ----------

        [Fact]
        public async Task DeleteTaskAsync_ByAdmin_DeletesTask()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");
            _db.AddMember(project.Id, sara.Id, "Admin");
            var task = _db.AddTask(project.Id, "Task 1", "To Do");

            await _service.DeleteTaskAsync(project.Id, task.Id, sara.Id, CancellationToken.None);

            Assert.Empty(_db.Context.Tasks.AsNoTracking().ToList());
        }

        [Fact]
        public async Task DeleteTaskAsync_ByMember_DoesNotDelete()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");
            _db.AddMember(project.Id, sara.Id, "Member");
            var task = _db.AddTask(project.Id, "Task 1", "To Do");

            await _service.DeleteTaskAsync(project.Id, task.Id, sara.Id, CancellationToken.None);

            Assert.Single(_db.Context.Tasks.AsNoTracking().ToList());
        }

        [Fact]
        public async Task DeleteTaskAsync_TaskOfAnotherProject_DoesNotDelete()
        {
            var ali = _db.AddUser("Ali");

            var projectA = _db.AddProject("A", ali.Id);
            var projectB = _db.AddProject("B", ali.Id);
            _db.AddMember(projectA.Id, ali.Id, "Owner");
            _db.AddMember(projectB.Id, ali.Id, "Owner");

            var taskOfB = _db.AddTask(projectB.Id, "Task of B", "To Do");

            // projectId = A ولی taskId مال B
            await _service.DeleteTaskAsync(projectA.Id, taskOfB.Id, ali.Id, CancellationToken.None);

            Assert.Single(_db.Context.Tasks.AsNoTracking().ToList());
        }

        // ---------- SaveTaskAsync ----------

        [Fact]
        public async Task SaveTaskAsync_NewTask_ByAdmin_CreatesTask()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");
            _db.AddMember(project.Id, sara.Id, "Admin");

            await _service.SaveTaskAsync(project.Id, null, sara.Id, "New task", "Desc", "In Progress", ali.Id, CancellationToken.None);

            var saved = _db.Context.Tasks.AsNoTracking().Single();
            Assert.Equal("New task", saved.Title);
            Assert.Equal("In Progress", saved.Status);
            Assert.Equal(project.Id, saved.ProjectId);
            Assert.Equal(ali.Id, saved.AssignedUserId);
        }

        [Fact]
        public async Task SaveTaskAsync_ByMember_DoesNotCreateTask()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");
            _db.AddMember(project.Id, sara.Id, "Member");

            await _service.SaveTaskAsync(project.Id, null, sara.Id, "New task", "Desc", "To Do", null, CancellationToken.None);

            Assert.Empty(_db.Context.Tasks.AsNoTracking().ToList());
        }

        [Fact]
        public async Task SaveTaskAsync_ExistingTask_UpdatesFields()
        {
            var ali = _db.AddUser("Ali");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");
            var task = _db.AddTask(project.Id, "Old", "To Do");

            await _service.SaveTaskAsync(project.Id, task.Id, ali.Id, "New", "New desc", "Done", ali.Id, CancellationToken.None);

            var saved = _db.Context.Tasks.AsNoTracking().Single();
            Assert.Equal("New", saved.Title);
            Assert.Equal("New desc", saved.Description);
            Assert.Equal("Done", saved.Status);
            Assert.Equal(ali.Id, saved.AssignedUserId);
        }

        // ---------- GetTaskForEditAsync ----------

        [Fact]
        public async Task GetTaskForEditAsync_ByMember_ReturnsNull()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");
            _db.AddMember(project.Id, sara.Id, "Member");

            var result = await _service.GetTaskForEditAsync(project.Id, null, sara.Id, CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetTaskForEditAsync_NewTask_ReturnsEmptyTaskAndOnlyProjectMembers()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");
            var reza = _db.AddUser("Reza");   // عضو پروژه نیست

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");
            _db.AddMember(project.Id, sara.Id, "Member");

            var result = await _service.GetTaskForEditAsync(project.Id, null, ali.Id, CancellationToken.None);

            Assert.Equal(0, result.Task.Id);

            var names = result.ProjectUsers.Select(u => u.Name).ToList();
            Assert.Equal(2, names.Count);
            Assert.Contains("Ali", names);
            Assert.Contains("Sara", names);
            Assert.DoesNotContain("Reza", names);
        }

        [Fact]
        public async Task GetTaskForEditAsync_TaskOfAnotherProject_ReturnsNull()
        {
            var ali = _db.AddUser("Ali");

            var projectA = _db.AddProject("A", ali.Id);
            var projectB = _db.AddProject("B", ali.Id);
            _db.AddMember(projectA.Id, ali.Id, "Owner");
            _db.AddMember(projectB.Id, ali.Id, "Owner");

            var taskOfB = _db.AddTask(projectB.Id, "Task of B", "To Do");

            var result = await _service.GetTaskForEditAsync(projectA.Id, taskOfB.Id, ali.Id, CancellationToken.None);

            Assert.Null(result);
        }
    }
}