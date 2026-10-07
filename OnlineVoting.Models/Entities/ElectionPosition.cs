using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class ElectionPosition : ITracker, IAuditable
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ElectionId { get; set; } = string.Empty;
        public string PositionId { get; set; } = string.Empty;
        public decimal ApplicationFee { get; set; }
        public string Currency { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public virtual Election Election { get; set; } = null!;
        public virtual Position Position { get; set; } = null!;
        public ICollection<PositionApplication> Applications { get; set; } = [];
    }
}
