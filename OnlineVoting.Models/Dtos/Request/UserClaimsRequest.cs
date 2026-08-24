namespace OnlineVoting.Models.Dtos.Request
{
    /// <summary>
    /// Represents the information required to update a user claim.
    /// </summary>
    public class UserClaimsRequest
    {
        /// <summary>
        /// The email address of the user who owns the claim.
        /// </summary>
        /// <example>user@example.com</example>
        public required string Email { get; set; }

        /// <summary>
        /// The type of the claim being updated.
        /// </summary>
        /// <example>Permission</example>
        public required string ClaimType { get; set; }

        /// <summary>
        /// The new value of the claim.
        /// </summary>
        /// <example>ManageElection</example>
        public required string ClaimValue { get; set; }

        /// <summary>
        /// The current claim value that should be replaced.
        /// </summary>
        /// <example>CreateElection</example>
        public string? OldValue { get; set; }
    }
}