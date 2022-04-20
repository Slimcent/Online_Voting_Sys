namespace OnlineVoting.Models.Entities
{
    public class Vote
    {
        public Guid Id { get; set; }
        public Guid VoterId { get; set; } // StudentId
        public  Guid ContestantId { get; set; } // StudentId
        public string? UserId { get; set; }
        public bool? HasVoted { get; set; }
        public DateTime VotedAt { get; set; } = DateTime.Now;
        public virtual Student? Student { get; set; }
    }
}
