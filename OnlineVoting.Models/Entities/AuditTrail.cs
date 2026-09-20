using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace OnlineVoting.Models.Entities
{
    public class AuditTrail
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string? ActorUserId { get; set; }

        public string? ActorUsername { get; set; }

        public string? EndpointName { get; set; }

        public string? EventName { get; set; }

        public string? HttpMethod { get; set; }

        public string? EntityType { get; set; }

        public string? EntityId { get; set; }

        public int OutcomeId { get; set; }

        public string? Description { get; set; }

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public string? CorrelationId { get; set; }

        public DateTime CreatedAt { get; set; }
        public virtual AuditOutcome Outcome { get; set; } = null!;
        public virtual AuditLocation? Location { get; set; }
    }

    public class PendingAuditEntry
    {
        public EntityEntry Entry { get; set; }

        public EntityState State { get; set; }

        public string EntityType { get; set; }

        public string? EntityId { get; set; }

        public Dictionary<string, object?> OldValues { get; set; } = new();

        public Dictionary<string, object?> NewValues { get; set; } = new();
    }
}