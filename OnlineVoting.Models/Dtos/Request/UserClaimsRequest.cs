namespace OnlineVoting.Models.Dtos.Request
{
    /// <summary>
    /// Represents the information required to identify a user claim.
    /// </summary>
    public class UserClaimsRequest
    {
        /// <summary>
        /// The email address of the user who owns the claim.
        /// </summary>
        /// <example>user@example.com</example>
        public string? Email { get; set; }

        /// <summary>
        /// The type of the claim.
        /// </summary>
        /// <example>Permission</example>
        public string? ClaimType { get; set; }

        /// <summary>
        /// The value of the claim.
        /// </summary>
        /// <example>CreateElection</example>
        public string? ClaimValue { get; set; }
    }
}