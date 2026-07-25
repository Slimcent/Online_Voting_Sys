namespace OnlineVoting.Models.Dtos.Request
{
    public class LoginRequest
    {
        /// <summary>
        /// Gets or sets the email address associated with the user account.
        /// </summary>
        /// <example>user@example.com</example>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user's password.
        /// </summary>
        /// <example>Password123!</example>
        public string Password { get; set; }
    }
}