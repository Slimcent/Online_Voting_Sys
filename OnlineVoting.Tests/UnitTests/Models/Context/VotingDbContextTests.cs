using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;
using OnlineVoting.Tests.TestData.Data;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.UnitTests.Models.Context
{
    public class VotingDbContextTests
    {
        [Fact]
        public async Task SaveChanges_AddedTrackableEntity_ShouldSetCreatedAndUpdatedAuditFields()
        {
            using AuditDbContextFactory factory = new("adminuser");

            VotingDbContext context = factory.Context;

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");

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
            using AuditDbContextFactory factory = new("adminuser");

            VotingDbContext context = factory.Context;

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");

            context.Faculties.Add(faculty);

            await context.SaveChangesAsync(true);

            DateTime originalCreatedAt = faculty.CreatedAt;
            string? originalCreatedBy = faculty.CreatedBy;

            factory.CurrentUserContext.Username = "editoruser";

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
            using AuditDbContextFactory factory = new(null, null);

            VotingDbContext context = factory.Context;

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");

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
            using AuditDbContextFactory factory = new("adminuser");

            VotingDbContext context = factory.Context;

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");

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

        [Fact]
        public async Task AuditLocation_ShouldPersistWithAuditTrail()
        {
            using AuditDbContextFactory factory = new();

            VotingDbContext context = factory.Context;

            AuditOutcome outcome = await context.AuditOutcomes
                .SingleAsync(auditOutcome => auditOutcome.Name == "Success");

            AuditTrail auditTrail = new()
            {
                ActorUserId = "user-id",
                ActorUsername = "super.admin",
                EndpointName = "Create-Faculty",
                EventName = "Created",
                HttpMethod = "POST",
                EntityType = nameof(Faculty),
                EntityId = "1",
                OutcomeId = outcome.Id,
                IpAddress = "203.0.113.10"
            };

            AuditLocation auditLocation = new()
            {
                AuditTrailId = auditTrail.Id,
                AuditTrail = auditTrail,
                IpCountry = "Germany",
                IpRegion = "North Rhine-Westphalia",
                IpCity = "Paderborn",
                IpLatitude = 51.7189,
                IpLongitude = 8.7575,
                DeviceLatitude = 51.718912,
                DeviceLongitude = 8.757481,
                DeviceAccuracyMeters = 8.5,
                DeviceLocationCapturedAt = DateTime.UtcNow
            };

            context.AuditTrails.Add(auditTrail);
            context.AuditLocations.Add(auditLocation);

            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();

            AuditTrail? savedAuditTrail = await context.AuditTrails.Include(item => item.Location).SingleOrDefaultAsync(item => item.Id == auditTrail.Id);

            Assert.NotNull(savedAuditTrail);
            Assert.NotNull(savedAuditTrail.Location);
            Assert.Equal("Germany", savedAuditTrail.Location.IpCountry);
            Assert.Equal("Paderborn", savedAuditTrail.Location.IpCity);
            Assert.Equal(51.718912, savedAuditTrail.Location.DeviceLatitude);
            Assert.Equal(8.5, savedAuditTrail.Location.DeviceAccuracyMeters);
        }
    }
}