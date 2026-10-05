using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Models.Dtos.Request
{
    public class VoteRequest
    {
        public string? ContestantRegNo { get; set; }
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
        public string? ElectionId { get; set; }

        /// <summary>
        /// Gets or sets the student identifier used to filter registered voters.
        /// </summary>
        public Guid? StudentId { get; set; }

        /// <summary>
        /// Gets or sets the active status used to filter registered voters.
        /// </summary>
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
        public required string RegNumber { get; set; }

        /// <summary>
        /// Gets or sets the election identifier.
        /// </summary>
        public required string ElectionId { get; set; }
    }
}