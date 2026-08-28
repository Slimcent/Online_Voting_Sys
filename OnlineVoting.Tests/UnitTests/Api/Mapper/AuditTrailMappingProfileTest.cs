using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Api.Mapper;

namespace OnlineVoting.Tests.UnitTests.Api.Mapper
{
    public class AuditTrailMappingProfileTest
    {
        [Fact]
        public void AuditTrail_WithLocation_ShouldMapToAuditTrailResponse()
        {
            MapperConfiguration configuration = new(
                config => config.AddProfile<AuditMappingProfile>(),
                NullLoggerFactory.Instance);

            IMapper mapper = configuration.CreateMapper();

            DateTime capturedAt = DateTime.UtcNow;

            AuditTrail auditTrail = new()
            {
                Id = Guid.NewGuid().ToString(),
                ActorUserId = "user-id",
                ActorUsername = "super.admin",
                EndpointName = "Create-Faculty",
                EventName = "Created",
                HttpMethod = "POST",
                EntityType = "Faculty",
                EntityId = "1",
                OldValues = "{\"Active\":true,\"Name\":\"Media Sciences\"}",
                NewValues = "{\"Active\":true,\"Name\":\"Media Technology\"}",
                Outcome = new AuditOutcome
                {
                    Id = 1,
                    Name = "Success"
                },
                IpAddress = "127.0.0.1",
                UserAgent = "Test User Agent",
                CorrelationId = "test-correlation-id",
                CreatedAt = DateTime.UtcNow,
                Location = new AuditLocation
                {
                    IpCountry = "Germany",
                    IpRegion = "North Rhine-Westphalia",
                    IpCity = "Paderborn",
                    IpLatitude = 51.7189,
                    IpLongitude = 8.7575,
                    DeviceLatitude = 51.7190,
                    DeviceLongitude = 8.7576,
                    DeviceAccuracyMeters = 10.5,
                    DeviceLocationCapturedAt = capturedAt
                }
            };

            AuditTrailResponse response = mapper.Map<AuditTrailResponse>(auditTrail);

            Assert.Equal("Success", response.Outcome);
            Assert.NotNull(response.Location);
            Assert.Equal("Germany", response.Location.IpCountry);
            Assert.Equal("North Rhine-Westphalia", response.Location.IpRegion);
            Assert.Equal("Paderborn", response.Location.IpCity);
            Assert.Equal(51.7189, response.Location.IpLatitude);
            Assert.Equal(8.7575, response.Location.IpLongitude);
            Assert.Equal(51.7190, response.Location.DeviceLatitude);
            Assert.Equal(8.7576, response.Location.DeviceLongitude);
            Assert.Equal(10.5, response.Location.DeviceAccuracyMeters);
            Assert.Equal(capturedAt, response.Location.DeviceLocationCapturedAt);
            Assert.NotNull(response.OldValues);
            Assert.NotNull(response.NewValues);
            Assert.True(response.OldValues["Active"].GetBoolean());
            Assert.Equal("Media Sciences", response.OldValues["Name"].GetString());
            Assert.True(response.NewValues["Active"].GetBoolean());
            Assert.Equal("Media Technology", response.NewValues["Name"].GetString());
        }

        [Fact]
        public void AuditTrail_CreatedEntity_ShouldMapNullOldValues()
        {
            MapperConfiguration configuration = new(
                config => config.AddProfile<AuditMappingProfile>(),
                NullLoggerFactory.Instance);

            IMapper mapper = configuration.CreateMapper();

            AuditTrail auditTrail = new()
            {
                Id = Guid.NewGuid().ToString(),
                EventName = "Created",
                Outcome = new AuditOutcome
                {
                    Id = 1,
                    Name = "Success"
                },
                OldValues = null,
                NewValues = "{\"Active\":true,\"Name\":\"Media Sciences\"}"
            };

            AuditTrailResponse response = mapper.Map<AuditTrailResponse>(auditTrail);

            Assert.Null(response.OldValues);
            Assert.NotNull(response.NewValues);
            Assert.True(response.NewValues["Active"].GetBoolean());
            Assert.Equal("Media Sciences", response.NewValues["Name"].GetString());
        }

        [Fact]
        public void AuditTrail_DeletedEntity_ShouldMapNullNewValues()
        {
            MapperConfiguration configuration = new(
                config => config.AddProfile<AuditMappingProfile>(),
                NullLoggerFactory.Instance);

            IMapper mapper = configuration.CreateMapper();

            AuditTrail auditTrail = new()
            {
                Id = Guid.NewGuid().ToString(),
                EventName = "Deleted",
                Outcome = new AuditOutcome
                {
                    Id = 1,
                    Name = "Success"
                },
                OldValues = "{\"Active\":true,\"Name\":\"Media Sciences\"}",
                NewValues = null
            };

            AuditTrailResponse response = mapper.Map<AuditTrailResponse>(auditTrail);

            Assert.NotNull(response.OldValues);
            Assert.True(response.OldValues["Active"].GetBoolean());
            Assert.Equal("Media Sciences", response.OldValues["Name"].GetString());
            Assert.Null(response.NewValues);
        }
    }
}
