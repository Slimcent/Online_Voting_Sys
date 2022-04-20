namespace OnlineVoting.Models.Entities
{
    public class RegisteredVoter
    {
        public Guid Id { get; set; }
        public Guid? StudentId { get; set; }
        public string? VotingCode { get; set; }
        public virtual Student? Student { get; set; }
    }
}
