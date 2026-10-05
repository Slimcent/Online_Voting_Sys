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
}
