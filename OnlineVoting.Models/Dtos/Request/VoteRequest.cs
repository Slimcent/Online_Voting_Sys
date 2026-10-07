using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Models.Dtos.Request
{
    /// <summary>
    /// Represents the information required to submit a vote using registration numbers.
    /// </summary>
    public class VoteRequest
    {
        /// <summary>
        /// Gets or sets the contestant's registration number.
        /// </summary>
        /// <example>REG-2026-0002</example>
        public string? ContestantRegNo { get; set; }

        /// <summary>
        /// Gets or sets the voter's registration number.
        /// </summary>
        /// <example>REG-2026-0001</example>
        public string? VoterRegNo { get; set; }
    }

    /// <summary>
    /// Represents the filters used to retrieve registered voters.
    /// </summary>
    public class RegisteredVoterRequest : RequestParameters
    {
        /// <summary>
        /// Gets or sets the election identifier used to filter registered voters.
        /// </summary>
        /// <example>4bb71f06-31c6-4a0e-b3ef-a9e9a28f6761</example>
        public string? ElectionId { get; set; }

        /// <summary>
        /// Gets or sets the student identifier used to filter registered voters.
        /// </summary>
        /// <example>bbfcbcf1-45e0-49f9-b706-06ce6e7d8794</example>
        public Guid? StudentId { get; set; }

        /// <summary>
        /// Gets or sets the active status used to filter registered voters.
        /// </summary>
        /// <example>true</example>
        public bool? Active { get; set; }
    }

    /// <summary>
    /// Request used to register a student as a voter for an election.
    /// </summary>
    public class RegisterVoterRequest
    {
        /// <summary>
        /// Gets or sets the student's registration number.
        /// </summary>
        /// <example>REG-2026-0001</example>
        public required string RegNumber { get; set; }

        /// <summary>
        /// Gets or sets the election identifier.
        /// </summary>
        /// <example>4bb71f06-31c6-4a0e-b3ef-a9e9a28f6761</example>
        public required string ElectionId { get; set; }
    }

    /// <summary>
    /// Represents the information required to cast a vote.
    /// </summary>
    public class CastVoteRequest
    {
        /// <summary>
        /// Gets or sets the registered voter identifier.
        /// </summary>
        /// <example>3da40668-0c76-4192-a676-357255122ed1</example>
        public required string RegisteredVoterId { get; set; }

        /// <summary>
        /// Gets or sets the voting code assigned to the registered voter.
        /// </summary>
        /// <example>VOTE-7e3f6e5a9c9a4dc2a59a1055ec4e8fd2</example>
        public required string VotingCode { get; set; }

        /// <summary>
        /// Gets or sets the election position identifier.
        /// </summary>
        /// <example>311ca126-29b2-41d8-b9f4-1ce45133afe0</example>
        public required string ElectionPositionId { get; set; }

        /// <summary>
        /// Gets or sets the contestant identifier.
        /// </summary>
        /// <example>f0f99dd8-0dc8-49d3-956e-d4304078e193</example>
        public required string ContestantId { get; set; }
    }

    /// <summary>
    /// Represents the filters used to retrieve the authenticated user's voting history.
    /// </summary>
    public class VoteHistoryRequest : RequestParameters
    {
        /// <summary>
        /// Gets or sets the election identifier.
        /// </summary>
        /// <example>4bb71f06-31c6-4a0e-b3ef-a9e9a28f6761</example>
        public string? ElectionId { get; set; }

        /// <summary>
        /// Gets or sets the election position identifier.
        /// </summary>
        /// <example>311ca126-29b2-41d8-b9f4-1ce45133afe0</example>
        public string? ElectionPositionId { get; set; }
    }

    /// <summary>
    /// Represents the filters used to retrieve election results.
    /// </summary>
    public class ElectionResultRequest : RequestParameters
    {
        /// <summary>
        /// Gets or sets the election identifier used to filter results.
        /// </summary>
        /// <example>4bb71f06-31c6-4a0e-b3ef-a9e9a28f6761</example>
        public string? ElectionId { get; set; }

        /// <summary>
        /// Gets or sets the election position identifier used to filter results.
        /// </summary>
        /// <example>311ca126-29b2-41d8-b9f4-1ce45133afe0</example>
        public string? ElectionPositionId { get; set; }
    }
}