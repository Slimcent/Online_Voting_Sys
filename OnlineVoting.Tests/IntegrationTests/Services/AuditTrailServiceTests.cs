using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Tests.TestData.Data;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.IntegrationTests.Services
{
    public class AuditTrailServiceTests
    {
        [Fact]
        public async Task GetAuditTrails_WithoutFilters_ShouldReturnPagedAuditTrails()
        {
            using AuditTrailServiceFactory factory = new();

            await AuditTrailTestData.SeedAuditTrails(factory.DbContextFactory.Context);

            AuditTrailRequest request = new AuditTrailRequest
            {
                PageNumber = 1,
                PageSize = 2
            };

            Result<PagedResponse<AuditTrailResponse>> result = await factory.Service.GetAuditTrails(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Same(factory.MappedResponse, result.Value);
            Assert.NotNull(factory.MappedAuditTrails);
            Assert.Equal(2, factory.MappedAuditTrails.Count);
            Assert.Equal(4, factory.MappedAuditTrails.MetaData.TotalCount);
        }

        [Fact]
        public async Task GetAuditTrails_WithActorAndEntityFilters_ShouldReturnMatchingAuditTrail()
        {
            using AuditTrailServiceFactory factory = new();

            await AuditTrailTestData.SeedAuditTrails(factory.DbContextFactory.Context);

            AuditTrailRequest request = new AuditTrailRequest
            {
                ActorUsername = "super.admin",
                EventName = "Updated",
                EntityType = "Faculty",
                EntityId = "1"
            };

            Result<PagedResponse<AuditTrailResponse>> result = await factory.Service.GetAuditTrails(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(factory.MappedAuditTrails);
            Assert.Equal(1, factory.MappedAuditTrails.MetaData.TotalCount);

            AuditTrail auditTrail = Assert.Single(factory.MappedAuditTrails);

            Assert.Equal("super.admin", auditTrail.ActorUsername);
            Assert.Equal("Update-Faculty", auditTrail.EndpointName);
            Assert.Equal("Updated", auditTrail.EventName);
            Assert.Equal("Faculty", auditTrail.EntityType);
            Assert.Equal("1", auditTrail.EntityId);
        }

        [Fact]
        public async Task GetAuditTrails_WithOutcomeCorrelationAndIpFilters_ShouldReturnMatchingAuditTrail()
        {
            using AuditTrailServiceFactory factory = new();

            await AuditTrailTestData.SeedAuditTrails(factory.DbContextFactory.Context);

            AuditTrailRequest request = new AuditTrailRequest
            {
                Outcome = "Success",
                CorrelationId = "delete-correlation-id",
                IpAddress = "10.0.0.2"
            };

            Result<PagedResponse<AuditTrailResponse>> result = await factory.Service.GetAuditTrails(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(factory.MappedAuditTrails);
            Assert.Equal(1, factory.MappedAuditTrails.MetaData.TotalCount);

            AuditTrail auditTrail = Assert.Single(factory.MappedAuditTrails);

            Assert.Equal("Delete-Faculty", auditTrail.EndpointName);
            Assert.Equal("audit.admin", auditTrail.ActorUsername);
            Assert.Equal("10.0.0.2", auditTrail.IpAddress);
            Assert.Equal("delete-correlation-id", auditTrail.CorrelationId);
            Assert.Equal("Success", auditTrail.Outcome.Name);
        }

        [Fact]
        public async Task GetAuditTrails_WithEndpointFilter_ShouldReturnMatchingAuditTrail()
        {
            using AuditTrailServiceFactory factory = new();

            await AuditTrailTestData.SeedAuditTrails(factory.DbContextFactory.Context);

            AuditTrailRequest request = new AuditTrailRequest
            {
                EndpointName = "Create-Department"
            };

            Result<PagedResponse<AuditTrailResponse>> result = await factory.Service.GetAuditTrails(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(factory.MappedAuditTrails);
            Assert.Equal(1, factory.MappedAuditTrails.MetaData.TotalCount);

            AuditTrail auditTrail = Assert.Single(factory.MappedAuditTrails);

            Assert.Equal("Create-Department", auditTrail.EndpointName);
            Assert.Equal("Department", auditTrail.EntityType);
            Assert.Equal("3", auditTrail.EntityId);
        }

        [Fact]
        public async Task GetAuditTrails_WithFutureFromDate_ShouldReturnEmptyPage()
        {
            using AuditTrailServiceFactory factory = new();

            await AuditTrailTestData.SeedAuditTrails(factory.DbContextFactory.Context);

            AuditTrailRequest request = new AuditTrailRequest
            {
                From = DateTime.UtcNow.AddMinutes(1)
            };

            Result<PagedResponse<AuditTrailResponse>> result = await factory.Service.GetAuditTrails(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(factory.MappedAuditTrails);
            Assert.Empty(factory.MappedAuditTrails);
            Assert.Equal(0, factory.MappedAuditTrails.MetaData.TotalCount);
        }

        [Fact]
        public async Task GetAuditTrails_WithSecondPage_ShouldApplyPagination()
        {
            using AuditTrailServiceFactory factory = new();

            await AuditTrailTestData.SeedAuditTrails(factory.DbContextFactory.Context);

            AuditTrailRequest request = new AuditTrailRequest
            {
                PageNumber = 2,
                PageSize = 2
            };

            Result<PagedResponse<AuditTrailResponse>> result = await factory.Service.GetAuditTrails(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(factory.MappedAuditTrails);
            Assert.Equal(2, factory.MappedAuditTrails.Count);
            Assert.Equal(4, factory.MappedAuditTrails.MetaData.TotalCount);
        }

        [Fact]
        public async Task GetAuditTrails_WithLocation_ShouldIncludeAuditLocation()
        {
            using AuditTrailServiceFactory factory = new();

            await AuditTrailTestData.SeedAuditTrails(factory.DbContextFactory.Context);

            AuditTrailRequest request = new AuditTrailRequest
            {
                EndpointName = "Create-Faculty"
            };

            Result<PagedResponse<AuditTrailResponse>> result = await factory.Service.GetAuditTrails(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(factory.MappedAuditTrails);

            AuditTrail auditTrail = Assert.Single(factory.MappedAuditTrails);

            Assert.NotNull(auditTrail.Location);
            Assert.Equal("Germany", auditTrail.Location.IpCountry);
            Assert.Equal("North Rhine-Westphalia", auditTrail.Location.IpRegion);
            Assert.Equal("Paderborn", auditTrail.Location.IpCity);
            Assert.Equal(51.7189, auditTrail.Location.IpLatitude);
            Assert.Equal(8.7575, auditTrail.Location.IpLongitude);
            Assert.Equal(51.7190, auditTrail.Location.DeviceLatitude);
            Assert.Equal(8.7576, auditTrail.Location.DeviceLongitude);
            Assert.Equal(10.5, auditTrail.Location.DeviceAccuracyMeters);
        }

        [Theory]
        [InlineData(ApplicationConstants.Audit.Events.LoginFailed, ApplicationConstants.Audit.Outcomes.Failure)]
        [InlineData(ApplicationConstants.Audit.Events.AccountLocked, ApplicationConstants.Audit.Outcomes.Denied)]
        [InlineData(ApplicationConstants.Audit.Events.LoginRejectedLocked, ApplicationConstants.Audit.Outcomes.Denied)]
        [InlineData(ApplicationConstants.Audit.Events.LoginSucceeded, ApplicationConstants.Audit.Outcomes.Success)]
        public async Task RecordAuthenticationEvent_WithUser_ShouldPersistAuditTrail(string eventName, string outcome)
        {
            using AuditTrailServiceFactory factory = new();

            factory.DbContextFactory.SetRequestMetadata("Login", "POST");

            User user = new()
            {
                Id = "user-id",
                Email = "user@example.com"
            };

            await factory.Service.RecordAuthenticationEvent(eventName, outcome, "Authentication event.", user);

            AuditTrail auditTrail = await factory.DbContextFactory.Context.AuditTrails
                .Include(auditTrail => auditTrail.Outcome)
                .Include(auditTrail => auditTrail.Location)
                .SingleAsync();

            Assert.Equal(eventName, auditTrail.EventName);
            Assert.Equal(outcome, auditTrail.Outcome.Name);
            Assert.Equal(user.Id, auditTrail.ActorUserId);
            Assert.Equal(user.Email, auditTrail.ActorUsername);
            Assert.Equal(ApplicationConstants.Audit.EntityTypes.User, auditTrail.EntityType);
            Assert.Equal(user.Id, auditTrail.EntityId);
            Assert.Equal("Login", auditTrail.EndpointName);
            Assert.Equal("POST", auditTrail.HttpMethod);
            Assert.NotNull(auditTrail.Location);
        }

        [Fact]
        public async Task RecordAuthenticationEvent_WithoutUser_ShouldPersistAttemptedUsername()
        {
            using AuditTrailServiceFactory factory = new(username: null, userId: null);

            string attemptedUsername = "unknown@example.com";

            await factory.Service.RecordAuthenticationEvent(
                ApplicationConstants.Audit.Events.LoginFailed,
                ApplicationConstants.Audit.Outcomes.Failure,
                ApplicationConstants.Audit.Descriptions.InvalidCredentials,
                attemptedUsername: attemptedUsername);

            AuditTrail auditTrail = await factory.DbContextFactory.Context.AuditTrails
                .Include(auditTrail => auditTrail.Outcome)
                .SingleAsync();

            Assert.Null(auditTrail.ActorUserId);
            Assert.Equal(attemptedUsername, auditTrail.ActorUsername);
            Assert.Equal(ApplicationConstants.Audit.Events.LoginFailed, auditTrail.EventName);
            Assert.Equal(ApplicationConstants.Audit.Outcomes.Failure, auditTrail.Outcome.Name);
            Assert.Equal(ApplicationConstants.Audit.EntityTypes.User, auditTrail.EntityType);
            Assert.Null(auditTrail.EntityId);
        }
    }
}