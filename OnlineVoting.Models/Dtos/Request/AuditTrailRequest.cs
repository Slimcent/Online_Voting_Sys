using OnlineVoting.Models.Pagination;

/// <summary>
/// Represents the filtering and pagination parameters used to retrieve audit trail records.
/// </summary>
public class AuditTrailRequest : RequestParameters
{
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
    /// <example>Update-Faculty</example>
    public string? EndpointName { get; set; }

    /// <summary>
    /// The type of operation performed on the entity.
    /// </summary>
    /// <example>Updated</example>
    public string? EventName { get; set; }

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
    /// The correlation identifier associated with the request.
    /// </summary>
    /// <example>9be8769f-53de-45f7-923a-7df0e17ae6a2</example>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// The IP address from which the request originated.
    /// </summary>
    /// <example>127.0.0.1</example>
    public string? IpAddress { get; set; }

    /// <summary>
    /// The start date and time for filtering audit trail records.
    /// </summary>
    /// <example>2026-08-27T00:00:00Z</example>
    public DateTime? From { get; set; }

    /// <summary>
    /// The end date and time for filtering audit trail records.
    /// </summary>
    /// <example>2026-08-28T00:00:00Z</example>
    public DateTime? To { get; set; }
}