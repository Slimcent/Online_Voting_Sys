using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class Contestant : ITracker, IAuditable
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PositionApplicationId { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public virtual PositionApplication PositionApplication { get; set; } = null!;
    }
}