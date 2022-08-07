using System.ComponentModel.DataAnnotations;

namespace OnlineVoting.Models.Dtos.Request.Email
{
    public class ChangeEmailRequestDto
    {
        [Required]
        public string? NewEmail { get; set; }

        [Required]
        public string? Token { get; set; }
    }

    public class ChangeEmailDto
    {
        [Required]
        public string? Email { get; set; }

        [Required]
        public string? NewEmail { get; set; }

        [Required]
        public string? RecoveryEmail { get; set; }
    }
}
