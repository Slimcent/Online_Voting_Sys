using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;
using OnlineVoting.Tests.TestData.Data;
using OnlineVoting.Tests.TestData.Factories;
using System.Text.Json;

namespace OnlineVoting.Tests.IntegrationTests.Models.Context
{
    public class VotingDbContextAuditTests
    {
        [Fact]
        public async Task SaveChangesAsync_WithCreatedFaculty_ShouldCreateAuditTrail()
        {
            using AuditDbContextFactory factory = new();

            factory.SetRequestMetadata("Create-Faculty", "POST");

            Faculty faculty = FacultyTestData.CreateFaculty("Media");

            factory.Context.Faculties.Add(faculty);

            await factory.Context.SaveChangesAsync();

            AuditTrail auditTrail = await factory.Context.AuditTrails
                .Include(auditTrail => auditTrail.Outcome)
                .SingleAsync();

            Assert.Equal("user-id", auditTrail.ActorUserId);
            Assert.Equal("super.admin", auditTrail.ActorUsername);
            Assert.Equal("Create-Faculty", auditTrail.EndpointName);
            Assert.Equal("Created", auditTrail.EventName);
            Assert.Equal("POST", auditTrail.HttpMethod);
            Assert.Equal("Faculty", auditTrail.EntityType);
            Assert.Equal(faculty.Id.ToString(), auditTrail.EntityId);
            Assert.Equal("Success", auditTrail.Outcome.Name);
            Assert.Equal($"Faculty {faculty.Id} was created by super.admin.", auditTrail.Description);
            Assert.Equal("127.0.0.1", auditTrail.IpAddress);
            Assert.Equal("Test User Agent", auditTrail.UserAgent);
            Assert.Equal("test-correlation-id", auditTrail.CorrelationId);
            Assert.Null(auditTrail.OldValues);
            Assert.NotNull(auditTrail.NewValues);

            using JsonDocument newValues = JsonDocument.Parse(auditTrail.NewValues);

            Assert.Equal("Media", newValues.RootElement.GetProperty("Name").GetString());
            Assert.True(newValues.RootElement.GetProperty("Active").GetBoolean());
            Assert.False(newValues.RootElement.TryGetProperty("CreatedAt", out _));
            Assert.False(newValues.RootElement.TryGetProperty("UpdatedAt", out _));
            Assert.False(newValues.RootElement.TryGetProperty("CreatedBy", out _));
            Assert.False(newValues.RootElement.TryGetProperty("UpdatedBy", out _));
        }

        [Fact]
        public async Task SaveChangesAsync_WithDetachedUpdatedFaculty_ShouldCaptureDatabaseOldValue()
        {
            using AuditDbContextFactory factory = new();

            Faculty faculty = await CreateAndSaveFaculty(factory, "Media");

            factory.Context.Entry(faculty).State = EntityState.Detached;

            factory.SetRequestMetadata("Update-Faculty", "PUT");

            faculty.Name = "Media Technology";

            factory.Context.Faculties.Update(faculty);

            await factory.Context.SaveChangesAsync();

            AuditTrail auditTrail = await factory.Context.AuditTrails
                .Where(auditTrail => auditTrail.EventName == "Updated")
                .SingleAsync();

            Assert.Equal("Update-Faculty", auditTrail.EndpointName);
            Assert.Equal("Updated", auditTrail.EventName);
            Assert.Equal("PUT", auditTrail.HttpMethod);
            Assert.Equal("Faculty", auditTrail.EntityType);
            Assert.Equal(faculty.Id.ToString(), auditTrail.EntityId);
            Assert.Equal($"Faculty {faculty.Id} was updated by super.admin.", auditTrail.Description);
            Assert.NotNull(auditTrail.OldValues);
            Assert.NotNull(auditTrail.NewValues);

            using JsonDocument oldValues = JsonDocument.Parse(auditTrail.OldValues);
            using JsonDocument newValues = JsonDocument.Parse(auditTrail.NewValues);

            Assert.Equal("Media", oldValues.RootElement.GetProperty("Name").GetString());
            Assert.Equal("Media Technology", newValues.RootElement.GetProperty("Name").GetString());

            Assert.Equal(1, oldValues.RootElement.EnumerateObject().Count());
            Assert.Equal(1, newValues.RootElement.EnumerateObject().Count());
        }

        [Fact]
        public async Task SaveChangesAsync_WithDeletedFaculty_ShouldCaptureOldValues()
        {
            using AuditDbContextFactory factory = new();

            Faculty faculty = await CreateAndSaveFaculty(factory, "Media Sciences");

            factory.Context.Entry(faculty).State = EntityState.Detached;

            factory.SetRequestMetadata("Delete-Faculty", "DELETE");

            factory.Context.Faculties.Remove(faculty);

            await factory.Context.SaveChangesAsync();

            AuditTrail auditTrail = await factory.Context.AuditTrails
                .Where(auditTrail => auditTrail.EventName == "Deleted")
                .SingleAsync();

            Assert.Equal("Delete-Faculty", auditTrail.EndpointName);
            Assert.Equal("Deleted", auditTrail.EventName);
            Assert.Equal("DELETE", auditTrail.HttpMethod);
            Assert.Equal("Faculty", auditTrail.EntityType);
            Assert.Equal(faculty.Id.ToString(), auditTrail.EntityId);
            Assert.Equal($"Faculty {faculty.Id} was deleted by super.admin.", auditTrail.Description);
            Assert.NotNull(auditTrail.OldValues);
            Assert.Null(auditTrail.NewValues);

            using JsonDocument oldValues = JsonDocument.Parse(auditTrail.OldValues);

            Assert.Equal("Media Sciences", oldValues.RootElement.GetProperty("Name").GetString());
            Assert.True(oldValues.RootElement.GetProperty("Active").GetBoolean());
        }

        [Fact]
        public async Task SaveChangesAsync_WithOnlyTrackerChanges_ShouldNotCreateUpdatedAuditTrail()
        {
            using AuditDbContextFactory factory = new();

            Faculty faculty = await CreateAndSaveFaculty(factory);

            factory.Context.Entry(faculty).State = EntityState.Detached;

            factory.Context.Faculties.Update(faculty);

            await factory.Context.SaveChangesAsync();

            int updatedAuditTrailCount = await factory.Context.AuditTrails
                .CountAsync(auditTrail => auditTrail.EventName == "Updated");

            Assert.Equal(0, updatedAuditTrailCount);
        }

        [Fact]
        public async Task SaveChangesAsync_WhenAuditTrailIsModified_ShouldThrowInvalidOperationException()
        {
            using AuditDbContextFactory factory = new();

            await CreateAndSaveFaculty(factory);

            AuditTrail auditTrail = await factory.Context.AuditTrails.SingleAsync();

            auditTrail.Description = "Changed audit record";

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => factory.Context.SaveChangesAsync());

            Assert.Equal("Audit trail records cannot be modified or deleted.", exception.Message);
        }

        [Fact]
        public async Task SaveChangesAsync_WhenAuditTrailIsDeleted_ShouldThrowInvalidOperationException()
        {
            using AuditDbContextFactory factory = new();

            await CreateAndSaveFaculty(factory);

            AuditTrail auditTrail = await factory.Context.AuditTrails.SingleAsync();

            factory.Context.AuditTrails.Remove(auditTrail);

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => factory.Context.SaveChangesAsync());

            Assert.Equal("Audit trail records cannot be modified or deleted.", exception.Message);
        }

        [Fact]
        public async Task CreatedFaculty_WithDeviceLocation_ShouldCreateAuditLocation()
        {
            using AuditDbContextFactory factory = new();

            VotingDbContext context = factory.Context;

            DateTime capturedAt = DateTime.UtcNow;

            factory.AuditMetadataProvider.DeviceLatitude = 51.718912;
            factory.AuditMetadataProvider.DeviceLongitude = 8.757481;
            factory.AuditMetadataProvider.DeviceAccuracyMeters = 8.5;
            factory.AuditMetadataProvider.DeviceLocationCapturedAt = capturedAt;

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");

            context.Faculties.Add(faculty);

            await context.SaveChangesAsync();

            AuditTrail auditTrail = await context.AuditTrails.Include(auditTrail => auditTrail.Location).SingleAsync();

            Assert.NotNull(auditTrail.Location);
            Assert.Equal(51.718912, auditTrail.Location.DeviceLatitude);
            Assert.Equal(8.757481, auditTrail.Location.DeviceLongitude);
            Assert.Equal(8.5, auditTrail.Location.DeviceAccuracyMeters);
            Assert.Equal(capturedAt, auditTrail.Location.DeviceLocationCapturedAt);
        }

        [Fact]
        public async Task CreatedFaculty_WithIpLocation_ShouldCreateAuditLocation()
        {
            using AuditDbContextFactory factory = new();

            VotingDbContext context = factory.Context;

            factory.AuditMetadataProvider.IpCountry = "Germany";
            factory.AuditMetadataProvider.IpRegion = "North Rhine-Westphalia";
            factory.AuditMetadataProvider.IpCity = "Paderborn";
            factory.AuditMetadataProvider.IpLatitude = 51.7189;
            factory.AuditMetadataProvider.IpLongitude = 8.7575;

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");

            context.Faculties.Add(faculty);

            await context.SaveChangesAsync();

            AuditTrail auditTrail = await context.AuditTrails.Include(auditTrail => auditTrail.Location).SingleAsync();

            Assert.NotNull(auditTrail.Location);
            Assert.Equal("Germany", auditTrail.Location.IpCountry);
            Assert.Equal("North Rhine-Westphalia", auditTrail.Location.IpRegion);
            Assert.Equal("Paderborn", auditTrail.Location.IpCity);
            Assert.Equal(51.7189, auditTrail.Location.IpLatitude);
            Assert.Equal(8.7575, auditTrail.Location.IpLongitude);
            Assert.Null(auditTrail.Location.DeviceLatitude);
            Assert.Null(auditTrail.Location.DeviceLongitude);
        }

        private static async Task<Faculty> CreateAndSaveFaculty(AuditDbContextFactory factory, string name = "Media", bool active = true)
        {
            Faculty faculty = FacultyTestData.CreateFaculty(name, active);

            factory.Context.Faculties.Add(faculty);

            await factory.Context.SaveChangesAsync();

            return faculty;
        }
    }
}