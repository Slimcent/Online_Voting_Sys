namespace OnlineVoting.Models.Dtos.Request.Email
{
    public class VoterEmailDto
    {
        public string? FirstName { get; set; }
        public string? VotingCode { get; set; }
        public string? Email { get; set; }
    }

    public class VoteConfirmationEmailRequest
    {
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string ElectionName { get; set; }
        public required string PositionName { get; set; }
        public DateTime VotedAt { get; set; }
    }
}
