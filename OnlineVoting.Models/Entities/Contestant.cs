using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class Contestant : ITracker
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid PositionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public virtual Student? Student { get; set; }
        public virtual Position? Position { get; set; }
    }
}
