using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Tests.TestData.Contexts
{
    public sealed class TestAuditMetadataProvider : IAuditMetadataProvider
    {
        public string? ActorUserId { get; set; } = "user-id";

        public string? ActorUsername { get; set; } = "super.admin";

        public string? EndpointName { get; set; } = "Test-Endpoint";

        public string? HttpMethod { get; set; } = "POST";

        public string? IpAddress { get; set; } = "127.0.0.1";

        public string? UserAgent { get; set; } = "Test User Agent";

        public string? CorrelationId { get; set; } = "test-correlation-id";
        public double? DeviceLatitude { get; set; }

        public double? DeviceLongitude { get; set; }

        public double? DeviceAccuracyMeters { get; set; }
        public string? IpCountry { get; set; }

        public string? IpRegion { get; set; }

        public string? IpCity { get; set; }

        public double? IpLatitude { get; set; }

        public double? IpLongitude { get; set; }

        public DateTime? DeviceLocationCapturedAt { get; set; }

        public string? GetActorUserId()
        {
            return ActorUserId;
        }

        public string? GetActorUsername()
        {
            return ActorUsername;
        }

        public string? GetEndpointName()
        {
            return EndpointName;
        }

        public string? GetHttpMethod()
        {
            return HttpMethod;
        }

        public string? GetIpAddress()
        {
            return IpAddress;
        }

        public string? GetUserAgent()
        {
            return UserAgent;
        }

        public string? GetCorrelationId()
        {
            return CorrelationId;
        }

        public double? GetDeviceLatitude()
        {
            return DeviceLatitude;
        }

        public double? GetDeviceLongitude()
        {
            return DeviceLongitude;
        }

        public double? GetDeviceAccuracyMeters()
        {
            return DeviceAccuracyMeters;
        }

        public string? GetIpCountry()
        {
            return IpCountry;
        }

        public string? GetIpRegion()
        {
            return IpRegion;
        }

        public string? GetIpCity()
        {
            return IpCity;
        }

        public double? GetIpLatitude()
        {
            return IpLatitude;
        }

        public double? GetIpLongitude()
        {
            return IpLongitude;
        }
                
        public DateTime? GetDeviceLocationCapturedAt()
        {
            return DeviceLocationCapturedAt;
        }
    }
}
