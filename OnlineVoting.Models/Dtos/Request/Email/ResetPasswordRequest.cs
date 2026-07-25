using System.ComponentModel.DataAnnotations;

namespace OnlineVoting.Models.Dtos.Request.Email
{
    public class ResetPasswordRequest
    {
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? ResetPasswordToken { get; set; }
        [Required]
        public string? NewPassword { get; set; }
    }
}
