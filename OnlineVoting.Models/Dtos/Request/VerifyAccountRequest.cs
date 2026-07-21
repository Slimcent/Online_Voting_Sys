namespace OnlineVoting.Models.Dtos.Request
{
    public class VerifyAccountRequest
    {
        public required string Email { get; set; }

        public required string EmailConfirmationToken { get; set; }

        public required string ResetPasswordToken { get; set; }

        public required string NewPassword { get; set; }
    }
}