namespace OnlineVoting.Models.Dtos.Request.Email
{
    public class EmailRequestDto
    {
        public string? ToEmail { get; set; }
        public string? FromEmail { get; set; }
        public string? ToName { get; set; }
        public string? FromName { get; set; }
        public string? AppUrl { get; set; }
        public string? VotingCode { get; set; }
        public string? EmailConfirmationToken { get; set; }
        public string? ResetPasswordToken { get; set; }
    }
}
