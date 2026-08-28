namespace OnlineVoting.Models.Interfaces
{
    public interface IAuditMetadataProvider
    {
        string? GetActorUserId();

        string? GetActorUsername();

        string? GetEndpointName();

        string? GetHttpMethod();

        string? GetIpAddress();

        string? GetUserAgent();

        string? GetCorrelationId();

        double? GetDeviceLatitude();

        double? GetDeviceLongitude();

        double? GetDeviceAccuracyMeters();
        string? GetIpCountry();

        string? GetIpRegion();

        string? GetIpCity();

        double? GetIpLatitude();

        double? GetIpLongitude();

        DateTime? GetDeviceLocationCapturedAt();
    }
}