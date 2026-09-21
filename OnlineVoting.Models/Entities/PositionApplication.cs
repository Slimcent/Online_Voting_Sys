using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class PositionApplication : ITracker, IAuditable
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public Guid StudentId { get; set; }
        public string ElectionPositionId { get; set; } = string.Empty;
        public int PositionApplicationStatusId { get; set; }
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public virtual Student Student { get; set; } = null!;
        public virtual ElectionPosition ElectionPosition { get; set; } = null!;
        public virtual PositionApplicationStatus PositionApplicationStatus { get; set; } = null!;
        public virtual Contestant? Contestant { get; set; }
    }
}
