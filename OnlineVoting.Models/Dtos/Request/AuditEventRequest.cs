namespace OnlineVoting.Models.Dtos.Request
{
    public class AuditEventRequest
    {
        public string EventName { get; set; } = string.Empty;

        public string Outcome { get; set; } = string.Empty;

        public string? ActorUserId { get; set; }

        public string? ActorUsername { get; set; }

        public string? EntityType { get; set; }

        public string? EntityId { get; set; }

        public string? Description { get; set; }
    }
}