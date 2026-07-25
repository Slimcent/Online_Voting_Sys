namespace OnlineVoting.Models.Dtos.Request
{
    /// <summary>
    /// Represents the information required to verify a user account
    /// and set a new password.
    /// </summary>
    public class VerifyAccountRequest
    {
        /// <summary>
        /// The email address associated with the user account.
        /// </summary>
        /// <example>user@example.com</example>
        public required string Email { get; set; }

        /// <summary>
        /// The token used to confirm the user's email address.
        /// </summary>
        /// <example>email-confirmation-token</example>
        public required string EmailConfirmationToken { get; set; }

        /// <summary>
        /// The token that authorizes the password reset.
        /// </summary>
        /// <example>password-reset-token</example>
        public required string ResetPasswordToken { get; set; }

        /// <summary>
        /// The new password to assign to the user account.
        /// </summary>
        /// <example>NewPassword123!</example>
        public required string NewPassword { get; set; }
    }
}