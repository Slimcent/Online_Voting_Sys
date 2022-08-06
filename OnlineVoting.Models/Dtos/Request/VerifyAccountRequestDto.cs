using System.ComponentModel.DataAnnotations;

namespace OnlineVoting.Models.Dtos.Request
{
    public class VerifyAccountRequestDto
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? EmailConfirmationToken { get; set; }
        [Required]
        public string? ResetPasswordToken { get; set; }
        [Required]
        public string? NewPassword { get; set; }
    }
}
