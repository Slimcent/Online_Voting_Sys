namespace OnlineVoting.Models.Dtos.Request.Email
{
    public class CreateUserEmailRequest
    {
        public string? UserId { get; set; }
        public string? FirstName { get; set; }
        public string? Email { get; set; }
        public string? EmailConfirmationToken { get; set; }
        public string? ResetPasswordToken { get; set; }
    }
}
