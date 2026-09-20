using System.Text.Json;

namespace OnlineVoting.Models.Dtos.Response
{
    /// <summary>
    /// Represents an audit trail record describing an operation performed in the system.
    /// </summary>
    public class AuditTrailResponse
    {
        /// <summary>
        /// The unique identifier of the audit trail record.
        /// </summary>
        /// <example>23e6f088-c24c-4488-8332-8bea2a923d46</example>
        public string Id { get; set; }

        /// <summary>
        /// The identifier of the user who performed the operation.
        /// </summary>
        /// <example>475d1d1c-4659-4140-b1f0-105e56b27b5d</example>
        public string? ActorUserId { get; set; }

        /// <summary>
        /// The username of the user who performed the operation.
        /// </summary>
        /// <example>super.admin</example>
        public string? ActorUsername { get; set; }

        /// <summary>
        /// The name of the API endpoint where the operation was performed.
        /// </summary>
        /// <example>Delete-Faculty</example>
        public string? EndpointName { get; set; }

        /// <summary>
        /// The type of operation performed on the entity.
        /// </summary>
        /// <example>Deleted</example>
        public string? EventName { get; set; }

        /// <summary>
        /// The HTTP method used for the request.
        /// </summary>
        /// <example>DELETE</example>
        public string? HttpMethod { get; set; }

        /// <summary>
        /// The type of entity affected by the operation.
        /// </summary>
        /// <example>Faculty</example>
        public string? EntityType { get; set; }

        /// <summary>
        /// The identifier of the entity affected by the operation.
        /// </summary>
        /// <example>6</example>
        public string? EntityId { get; set; }

        /// <summary>
        /// The outcome of the audited operation.
        /// </summary>
        /// <example>Success</example>
        public string? Outcome { get; set; }

        /// <summary>
        /// Additional information describing the audited operation.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The entity values before the operation was performed.
        /// </summary>
        public Dictionary<string, JsonElement>? OldValues { get; set; }

        /// <summary>
        /// The entity values after the operation was performed.
        /// </summary>
        public Dictionary<string, JsonElement>? NewValues { get; set; }

        /// <summary>
        /// The IP address from which the request originated.
        /// </summary>
        /// <example>127.0.0.1</example>
        public string? IpAddress { get; set; }

        /// <summary>
        /// The user agent of the client that performed the request.
        /// </summary>
        /// <example>Mozilla/5.0</example>
        public string? UserAgent { get; set; }

        /// <summary>
        /// The correlation identifier used to trace the request across the application.
        /// </summary>
        /// <example>9be8769f-53de-45f7-923a-7df0e17ae6a2</example>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// The location information associated with the audited request.
        /// </summary>
        public AuditLocationResponse? Location { get; set; }

        /// <summary>
        /// The UTC date and time when the audit trail record was created.
        /// </summary>
        /// <example>2026-08-27T23:42:39Z</example>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Represents location information associated with an audit trail record.
    /// </summary>
    public class AuditLocationResponse
    {
        /// <summary>
        /// The country estimated from the request IP address.
        /// </summary>
        /// <example>Germany</example>
        public string? IpCountry { get; set; }

        /// <summary>
        /// The region estimated from the request IP address.
        /// </summary>
        /// <example>North Rhine-Westphalia</example>
        public string? IpRegion { get; set; }

        /// <summary>
        /// The city estimated from the request IP address.
        /// </summary>
        /// <example>Paderborn</example>
        public string? IpCity { get; set; }

        /// <summary>
        /// The approximate latitude estimated from the request IP address.
        /// </summary>
        /// <example>51.7189</example>
        public double? IpLatitude { get; set; }

        /// <summary>
        /// The approximate longitude estimated from the request IP address.
        /// </summary>
        /// <example>8.7575</example>
        public double? IpLongitude { get; set; }

        /// <summary>
        /// The latitude reported by the client device.
        /// </summary>
        /// <example>51.718912</example>
        public double? DeviceLatitude { get; set; }

        /// <summary>
        /// The longitude reported by the client device.
        /// </summary>
        /// <example>8.757481</example>
        public double? DeviceLongitude { get; set; }

        /// <summary>
        /// The accuracy of the device location in metres.
        /// </summary>
        /// <example>8.5</example>
        public double? DeviceAccuracyMeters { get; set; }

        /// <summary>
        /// The UTC date and time when the client device captured its location.
        /// </summary>
        /// <example>2026-08-28T14:30:00Z</example>
        public DateTime? DeviceLocationCapturedAt { get; set; }
    }

    /// <summary>
    /// Represents the result returned from an IP geolocation lookup.
    /// </summary>
    public class IpGeolocationResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the geolocation lookup was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the message returned by the geolocation provider.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the country associated with the IP address.
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// Gets or sets the region associated with the IP address.
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// Gets or sets the city associated with the IP address.
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Gets or sets the approximate latitude associated with the IP address.
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the approximate longitude associated with the IP address.
        /// </summary>
        public double? Longitude { get; set; }
    }
}