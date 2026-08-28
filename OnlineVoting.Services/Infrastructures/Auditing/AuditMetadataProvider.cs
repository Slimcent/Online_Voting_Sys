using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OnlineVoting.Models.Configurations;
using OnlineVoting.Models.Interfaces;
using System.Globalization;

namespace OnlineVoting.Services.Infrastructures.Auditing
{
    public class AuditMetadataProvider : IAuditMetadataProvider
    {
        private const string DeviceLatitudeHeader = "X-Device-Latitude";
        private const string DeviceLongitudeHeader = "X-Device-Longitude";
        private const string DeviceAccuracyHeader = "X-Device-Accuracy";
        private const string DeviceLocationCapturedAtHeader = "X-Device-Location-Captured-At";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUserContext _currentUserContext;

        public AuditMetadataProvider(IHttpContextAccessor httpContextAccessor, ICurrentUserContext currentUserContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _currentUserContext = currentUserContext;
        }

        public string? GetActorUserId()
        {
            return _currentUserContext.UserId;
        }

        public string? GetActorUsername()
        {
            return _currentUserContext.Username;
        }

        public string? GetIpCountry()
        {
            return GetContextItem(RequestContextKeys.IpCountry)?.ToString();
        }

        public string? GetIpRegion()
        {
            return GetContextItem(RequestContextKeys.IpRegion)?.ToString();
        }

        public string? GetIpCity()
        {
            return GetContextItem(RequestContextKeys.IpCity)?.ToString();
        }

        public double? GetIpLatitude()
        {
            return GetDoubleContextItem(RequestContextKeys.IpLatitude);
        }

        public double? GetIpLongitude()
        {
            return GetDoubleContextItem(RequestContextKeys.IpLongitude);
        }

        public string? GetHttpMethod()
        {
            return _httpContextAccessor.HttpContext?.Request.Method;
        }

        public string? GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
        }

        public double? GetDeviceLatitude()
        {
            return GetCoordinateHeader(DeviceLatitudeHeader, -90, 90);
        }

        public double? GetDeviceLongitude()
        {
            return GetCoordinateHeader(DeviceLongitudeHeader, -180, 180);
        }

        public string? GetEndpointName()
        {
            Endpoint? endpoint = _httpContextAccessor.HttpContext?.GetEndpoint();

            return endpoint?
                .Metadata
                .GetMetadata<IEndpointNameMetadata>()?
                .EndpointName;
        }
                
        public string? GetIpAddress()
        {
            string? ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

            if (ipAddress == "::1")
                return "127.0.0.1";

            return ipAddress;
        }

        public string? GetCorrelationId()
        {
            HttpContext? httpContext = _httpContextAccessor.HttpContext;

            return httpContext?.Items[RequestContextKeys.CorrelationId]?.ToString()
                ?? httpContext?.TraceIdentifier;
        }

        public double? GetDeviceAccuracyMeters()
        {
            string? value = _httpContextAccessor.HttpContext?
                .Request.Headers[DeviceAccuracyHeader]
                .FirstOrDefault();

            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double accuracy))
                return null;

            return accuracy >= 0 ? accuracy : null;
        }

        public DateTime? GetDeviceLocationCapturedAt()
        {
            string? value = _httpContextAccessor.HttpContext?
                .Request.Headers[DeviceLocationCapturedAtHeader]
                .FirstOrDefault();

            if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out DateTimeOffset capturedAt))
            {
                return null;
            }

            return capturedAt.UtcDateTime;
        }

        private object? GetContextItem(string key)
        {
            HttpContext? httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
                return null;

            httpContext.Items.TryGetValue(key, out object? value);

            return value;
        }

        private double? GetDoubleContextItem(string key)
        {
            object? value = GetContextItem(key);

            if (value is double doubleValue)
                return doubleValue;

            return null;
        }

        private double? GetCoordinateHeader(string headerName, double minimum, double maximum)
        {
            string? value = _httpContextAccessor.HttpContext?.Request.Headers[headerName].FirstOrDefault();

            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double coordinate))
                return null;

            if (coordinate < minimum || coordinate > maximum)
                return null;

            return coordinate;
        }
    }
}