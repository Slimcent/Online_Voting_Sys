namespace OnlineVoting.Models.Configurations
{
    public class AuditTrailContext
    {
        public string? EventName { get; set; }

        public string? EntityType { get; set; }

        public string? EntityId { get; set; }

        public string? Description { get; set; }

        public object? OldValues { get; set; }

        public object? NewValues { get; set; }
    }
}