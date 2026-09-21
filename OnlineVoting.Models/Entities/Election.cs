using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class Election : ITracker, IAuditable
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string Name { get; set; }
        public long YearId { get; set; }
        public int ElectionTypeId { get; set; }
        public int ElectionStatusId { get; set; }
        public long? FacultyId { get; set; }
        public long? DepartmentId { get; set; }
        public DateTime? ApplicationStartAt { get; set; }
        public DateTime? ApplicationEndAt { get; set; }
        public DateTime? VoterRegistrationStartAt { get; set; }
        public DateTime? VoterRegistrationEndAt { get; set; }
        public DateTime? VotingStartAt { get; set; }
        public DateTime? VotingEndAt { get; set; }
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public virtual Year Year { get; set; } = null!;
        public virtual ElectionType ElectionType { get; set; } = null!;
        public virtual ElectionStatus ElectionStatus { get; set; } = null!;
        public virtual Faculty? Faculty { get; set; }
        public virtual Department? Department { get; set; }
        public ICollection<ElectionPosition> ElectionPositions { get; set; } = [];
        public ICollection<RegisteredVoter> RegisteredVoters { get; set; } = [];
    }
}