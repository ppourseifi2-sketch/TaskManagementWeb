using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagementWeb.Services;

namespace TaskManagementWeb.Tests
{
    public class MemberServiceTests : IDisposable
    {
        private readonly TestDatabase _db;
        private readonly MemberService _service;

        public MemberServiceTests()
        {
            _db = new TestDatabase();
            _service = new MemberService(_db.Context);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        // ---------- GetMembersIndexAsync ----------

        [Fact]
        public async Task GetMembersIndexAsync_ByOwner_ReturnsAllMembers()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");
            _db.AddMember(project.Id, sara.Id, "Member");

            var result = await _service.GetMembersIndexAsync(project.Id, ali.Id, CancellationToken.None);

            var names = result.Members.Select(m => m.UserName).ToList();

            Assert.Equal(project.Id, result.ProjectId);
            Assert.Equal(2, names.Count);
            Assert.Contains("Ali", names);
            Assert.Contains("Sara", names);
        }

        [Fact]
        public async Task GetMembersIndexAsync_ByAdmin_ReturnsNull()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");
            _db.AddMember(project.Id, sara.Id, "Admin");

            var result = await _service.GetMembersIndexAsync(project.Id, sara.Id, CancellationToken.None);

            Assert.Null(result);
        }

        // ---------- ChangeRoleAsync ----------

        [Fact]
        public async Task ChangeRoleAsync_ByOwner_ChangesRole()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");
            var saraMember = _db.AddMember(project.Id, sara.Id, "Member");

            await _service.ChangeRoleAsync(project.Id, saraMember.Id, ali.Id, "Admin", CancellationToken.None);

            var saved = _db.Context.ProjectMembers.AsNoTracking().Single(m => m.Id == saraMember.Id);
            Assert.Equal("Admin", saved.Role);
        }

        [Fact]
        public async Task ChangeRoleAsync_ByAdmin_DoesNotChangeRole()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");
            var reza = _db.AddUser("Reza");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");
            _db.AddMember(project.Id, sara.Id, "Admin");
            var rezaMember = _db.AddMember(project.Id, reza.Id, "Member");

            // سارا ادمینه و حق نداره نقش بقیه رو عوض کنه
            await _service.ChangeRoleAsync(project.Id, rezaMember.Id, sara.Id, "Owner", CancellationToken.None);

            var saved = _db.Context.ProjectMembers.AsNoTracking().Single(m => m.Id == rezaMember.Id);
            Assert.Equal("Member", saved.Role);
        }

        [Fact]
        public async Task ChangeRoleAsync_InvalidRole_DoesNotChangeRole()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");
            var saraMember = _db.AddMember(project.Id, sara.Id, "Member");

            await _service.ChangeRoleAsync(project.Id, saraMember.Id, ali.Id, "Hacker", CancellationToken.None);

            var saved = _db.Context.ProjectMembers.AsNoTracking().Single(m => m.Id == saraMember.Id);
            Assert.Equal("Member", saved.Role);
        }

        [Fact]
        public async Task ChangeRoleAsync_MemberOfAnotherProject_DoesNotChangeRole()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");

            var projectA = _db.AddProject("A", ali.Id);
            var projectB = _db.AddProject("B", sara.Id);
            _db.AddMember(projectA.Id, ali.Id, "Owner");
            _db.AddMember(projectB.Id, sara.Id, "Owner");
            var saraInB = _db.AddMember(projectB.Id, sara.Id, "Member");

            // علی مالک پروژه A هست ولی عضویت مربوط به پروژه B رو می‌خواد تغییر بده
            await _service.ChangeRoleAsync(projectA.Id, saraInB.Id, ali.Id, "Admin", CancellationToken.None);

            var saved = _db.Context.ProjectMembers.AsNoTracking().Single(m => m.Id == saraInB.Id);
            Assert.Equal("Member", saved.Role);
        }

        // ---------- RemoveMemberAsync ----------

        [Fact]
        public async Task RemoveMemberAsync_ByOwner_RemovesMember()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");
            var saraMember = _db.AddMember(project.Id, sara.Id, "Member");

            await _service.RemoveMemberAsync(project.Id, saraMember.Id, ali.Id, CancellationToken.None);

            var count = _db.Context.ProjectMembers.AsNoTracking().Count();
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task RemoveMemberAsync_ByMember_DoesNotRemove()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");

            var project = _db.AddProject("P", ali.Id);
            var aliMember = _db.AddMember(project.Id, ali.Id, "Owner");
            _db.AddMember(project.Id, sara.Id, "Member");

            await _service.RemoveMemberAsync(project.Id, aliMember.Id, sara.Id, CancellationToken.None);

            var count = _db.Context.ProjectMembers.AsNoTracking().Count();
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task RemoveMemberAsync_MemberOfAnotherProject_DoesNotRemove()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");

            var projectA = _db.AddProject("A", ali.Id);
            var projectB = _db.AddProject("B", sara.Id);
            _db.AddMember(projectA.Id, ali.Id, "Owner");
            var saraInB = _db.AddMember(projectB.Id, sara.Id, "Owner");

            await _service.RemoveMemberAsync(projectA.Id, saraInB.Id, ali.Id, CancellationToken.None);

            var count = _db.Context.ProjectMembers.AsNoTracking().Count();
            Assert.Equal(2, count);
        }

        // ---------- GetMembersAddAsync ----------

        [Fact]
        public async Task GetMembersAddAsync_ReturnsOnlyUsersWhoAreNotMembers()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");
            _db.AddUser("Reza");
            _db.AddUser("Mina");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");
            _db.AddMember(project.Id, sara.Id, "Member");

            var result = await _service.GetMembersAddAsync(project.Id, ali.Id, CancellationToken.None);

            var names = result.AvailableUsers.Select(u => u.Name).ToList();

            Assert.Equal(2, names.Count);
            Assert.Contains("Reza", names);
            Assert.Contains("Mina", names);
            Assert.DoesNotContain("Ali", names);
            Assert.DoesNotContain("Sara", names);
        }

        // ---------- AddMemberAsync ----------

        [Fact]
        public async Task AddMemberAsync_ByOwner_AddsMember()
        {
            var ali = _db.AddUser("Ali");
            var reza = _db.AddUser("Reza");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");

            await _service.AddMemberAsync(project.Id, ali.Id, reza.Id, "Member", CancellationToken.None);

            var saved = _db.Context.ProjectMembers.AsNoTracking().Single(m => m.UserId == reza.Id);
            Assert.Equal(project.Id, saved.ProjectId);
            Assert.Equal("Member", saved.Role);
        }

        [Fact]
        public async Task AddMemberAsync_AlreadyMember_DoesNotAddTwice()
        {
            var ali = _db.AddUser("Ali");
            var sara = _db.AddUser("Sara");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");
            _db.AddMember(project.Id, sara.Id, "Member");

            await _service.AddMemberAsync(project.Id, ali.Id, sara.Id, "Admin", CancellationToken.None);

            var count = _db.Context.ProjectMembers.AsNoTracking().Count(m => m.UserId == sara.Id);
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task AddMemberAsync_InvalidRole_DoesNotAdd()
        {
            var ali = _db.AddUser("Ali");
            var reza = _db.AddUser("Reza");

            var project = _db.AddProject("P", ali.Id);
            _db.AddMember(project.Id, ali.Id, "Owner");

            await _service.AddMemberAsync(project.Id, ali.Id, reza.Id, "Hacker", CancellationToken.None);

            var count = _db.Context.ProjectMembers.AsNoTracking().Count();
            Assert.Equal(1, count);
        }
    }
}