using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Interfaces;

public class Vote : ITracker, IAuditable
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string RegisteredVoterId { get; set; } = string.Empty;
    public string ContestantId { get; set; } = string.Empty;
    public string ElectionPositionId { get; set; } = string.Empty;
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public virtual RegisteredVoter RegisteredVoter { get; set; } = null!;
    public virtual Contestant Contestant { get; set; } = null!;
    public virtual ElectionPosition ElectionPosition { get; set; } = null!;
}