using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Models.Dtos.Response
{
    public class VoterResponse
    {
    }

    /// <summary>
    /// Represents a registered voter.
    /// </summary>
    public class RegisteredVoterResponse
    {
        /// <summary>
        /// Gets or sets the registered voter identifier.
        /// </summary>
        public string RegisteredVoterId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the student identifier.
        /// </summary>
        public Guid StudentId { get; set; }

        /// <summary>
        /// Gets or sets the student's registration number.
        /// </summary>
        public string RegistrationNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the student's full name.
        /// </summary>
        public string StudentName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the election identifier.
        /// </summary>
        public string ElectionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the election name.
        /// </summary>
        public string ElectionName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the voting code.
        /// </summary>
        public string VotingCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the voter registration is active.
        /// </summary>
        public bool Active { get; set; }
    }

    /// <summary>
    /// Represents a vote cast by the authenticated voter.
    /// </summary>
    public class VoteHistoryResponse
    {
        /// <summary>
        /// Gets or sets the vote identifier.
        /// </summary>
        public required string VoteId { get; set; }

        /// <summary>
        /// Gets or sets the election identifier.
        /// </summary>
        public required string ElectionId { get; set; }

        /// <summary>
        /// Gets or sets the election name.
        /// </summary>
        public required string ElectionName { get; set; }

        /// <summary>
        /// Gets or sets the election position identifier.
        /// </summary>
        public required string ElectionPositionId { get; set; }

        /// <summary>
        /// Gets or sets the position name.
        /// </summary>
        public required string PositionName { get; set; }

        /// <summary>
        /// Gets or sets the contestant identifier.
        /// </summary>
        public required string ContestantId { get; set; }

        /// <summary>
        /// Gets or sets the name of the contestant voted for.
        /// </summary>
        public required string ContestantName { get; set; }

        /// <summary>
        /// Gets or sets the date and time the vote was cast.
        /// </summary>
        public DateTime VotedAt { get; set; }
    }

    /// <summary>
    /// Represents the result of a contestant in an election position.
    /// </summary>
    public class ElectionResultResponse
    {
        /// <summary>
        /// Gets or sets the election identifier.
        /// </summary>
        public required string ElectionId { get; set; }

        /// <summary>
        /// Gets or sets the election name.
        /// </summary>
        public required string ElectionName { get; set; }

        /// <summary>
        /// Gets or sets the election position identifier.
        /// </summary>
        public required string ElectionPositionId { get; set; }

        /// <summary>
        /// Gets or sets the position name.
        /// </summary>
        public required string PositionName { get; set; }

        /// <summary>
        /// Gets or sets the contestant identifier.
        /// </summary>
        public required string ContestantId { get; set; }

        /// <summary>
        /// Gets or sets the contestant's name.
        /// </summary>
        public required string ContestantName { get; set; }

        /// <summary>
        /// Gets or sets the number of votes received by the contestant.
        /// </summary>
        public int VoteCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of votes cast for the election position.
        /// </summary>
        public int TotalVotes { get; set; }

        /// <summary>
        /// Gets or sets the percentage of votes received by the contestant.
        /// </summary>
        public decimal Percentage { get; set; }
    }
}
