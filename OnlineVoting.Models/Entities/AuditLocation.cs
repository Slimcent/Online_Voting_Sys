namespace OnlineVoting.Models.Entities
{
    public class AuditLocation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string AuditTrailId { get; set; } = null!;

        public string? IpCountry { get; set; }

        public string? IpRegion { get; set; }

        public string? IpCity { get; set; }

        public double? IpLatitude { get; set; }

        public double? IpLongitude { get; set; }

        public double? DeviceLatitude { get; set; }

        public double? DeviceLongitude { get; set; }

        public double? DeviceAccuracyMeters { get; set; }

        public DateTime? DeviceLocationCapturedAt { get; set; }

        public virtual AuditTrail AuditTrail { get; set; } = null!;
    }
}