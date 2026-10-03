using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class IdempotencyRecord : ITracker
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Key { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public string RequestHash { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ResourceId { get; set; }
        public int? StatusCode { get; set; }
        public string? Response { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}