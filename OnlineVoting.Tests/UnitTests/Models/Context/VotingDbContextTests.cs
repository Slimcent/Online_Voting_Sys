using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Tests.UnitTests.Models.Context
{
    public class VotingDbContextTests
    {
        [Fact]
        public async Task SaveChanges_AddedTrackableEntity_ShouldSetCreatedAndUpdatedAuditFields()
        {
            TestCurrentUserContext currentUserContext = new("adminuser");
            await using VotingDbContext context = CreateContext(currentUserContext);

            Faculty faculty = new()
            {
                Name = "Engineering",
                Activated = true
            };

            context.Faculties.Add(faculty);

            DateTime beforeSave = DateTime.UtcNow;

            await context.SaveChangesAsync(true);

            DateTime afterSave = DateTime.UtcNow;

            Assert.InRange(faculty.CreatedAt, beforeSave, afterSave);
            Assert.InRange(faculty.UpdatedAt, beforeSave, afterSave);
            Assert.Equal("adminuser", faculty.CreatedBy);
            Assert.Equal("adminuser", faculty.UpdatedBy);
        }

        [Fact]
        public async Task SaveChanges_ModifiedTrackableEntity_ShouldUpdateUpdatedAuditFields()
        {
            TestCurrentUserContext currentUserContext = new("adminuser");
            await using VotingDbContext context = CreateContext(currentUserContext);

            Faculty faculty = new()
            {
                Name = "Engineering",
                Activated = true
            };

            context.Faculties.Add(faculty);
            await context.SaveChangesAsync(true);

            DateTime originalCreatedAt = faculty.CreatedAt;
            string? originalCreatedBy = faculty.CreatedBy;

            currentUserContext.Username = "editoruser";
            faculty.Name = "Updated Engineering";

            DateTime beforeUpdate = DateTime.UtcNow;

            await context.SaveChangesAsync(true);

            DateTime afterUpdate = DateTime.UtcNow;

            Assert.Equal(originalCreatedAt, faculty.CreatedAt);
            Assert.Equal(originalCreatedBy, faculty.CreatedBy);
            Assert.InRange(faculty.UpdatedAt, beforeUpdate, afterUpdate);
            Assert.Equal("editoruser", faculty.UpdatedBy);
        }

        [Fact]
        public async Task SaveChanges_NoCurrentUser_ShouldAllowNullAuditUser()
        {
            TestCurrentUserContext currentUserContext = new(null);
            await using VotingDbContext context = CreateContext(currentUserContext);

            Faculty faculty = new()
            {
                Name = "Engineering",
                Activated = true
            };

            context.Faculties.Add(faculty);

            await context.SaveChangesAsync(true);

            Assert.Null(faculty.CreatedBy);
            Assert.Null(faculty.UpdatedBy);
            Assert.NotEqual(default, faculty.CreatedAt);
            Assert.NotEqual(default, faculty.UpdatedAt);
        }

        [Fact]
        public async Task SaveChanges_UnchangedTrackableEntity_ShouldNotChangeAuditFields()
        {
            TestCurrentUserContext currentUserContext = new("adminuser");
            await using VotingDbContext context = CreateContext(currentUserContext);

            Faculty faculty = new()
            {
                Name = "Engineering",
                Activated = true
            };

            context.Faculties.Add(faculty);
            await context.SaveChangesAsync(true);

            DateTime createdAt = faculty.CreatedAt;
            DateTime updatedAt = faculty.UpdatedAt;
            string? createdBy = faculty.CreatedBy;
            string? updatedBy = faculty.UpdatedBy;

            await context.SaveChangesAsync(true);

            Assert.Equal(createdAt, faculty.CreatedAt);
            Assert.Equal(updatedAt, faculty.UpdatedAt);
            Assert.Equal(createdBy, faculty.CreatedBy);
            Assert.Equal(updatedBy, faculty.UpdatedBy);
        }

        private static VotingDbContext CreateContext(ICurrentUserContext currentUserContext)
        {
            DbContextOptions<VotingDbContext> options = new DbContextOptionsBuilder<VotingDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new VotingDbContext(options, currentUserContext);
        }

        private sealed class TestCurrentUserContext : ICurrentUserContext
        {
            public TestCurrentUserContext(string? username)
            {
                Username = username;
            }

            public string? Username { get; set; }
        }
    }
}