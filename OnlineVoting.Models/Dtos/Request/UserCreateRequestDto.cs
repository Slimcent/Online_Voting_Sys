using OnlineVoting.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace OnlineVoting.Models.Dtos.Request
{
    public class UserCreateRequestDto
    {
        [Required(ErrorMessage = "First Name canot be empty"), RegularExpression(@"^[a-zA-Z]+$",
            ErrorMessage = "Only Alphabets allowed"), MaxLength(20), MinLength(2)]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Email is required"), EmailAddress]
        public string? Email { get; set; }

        [Required]
        public UserType UserType { get; set; }

        [Required]
        public string? Role { get; set; }
    }
}
