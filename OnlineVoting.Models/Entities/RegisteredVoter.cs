using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class RegisteredVoter : ITracker, IAuditable
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public Guid StudentId { get; set; }
        public string ElectionId { get; set; }
        public string VotingCode { get; set; }
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public virtual Student Student { get; set; } = null!;
        public virtual Election Election { get; set; } = null!;
        public ICollection<Vote> Votes { get; set; } = [];
    }
}
