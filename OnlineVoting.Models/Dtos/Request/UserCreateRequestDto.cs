using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVoting.Models.Dtos.Request
{
    public class UserCreateRequestDto
    {
        [Required(ErrorMessage = "First Name canot be empty"), RegularExpression(@"^[a-zA-Z]+$",
            ErrorMessage = "Only Alphabets allowed"), MaxLength(20), MinLength(2)]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last Name canot be empty"), RegularExpression(@"^[a-zA-Z]+$",
            ErrorMessage = "Only Alphabets allowed"), MaxLength(20), MinLength(2)]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Email is required"), EmailAddress]
        public string? Email { get; set; }

        [RegularExpression(@"^[0]\d{10}$", ErrorMessage = "Invalid Phone Number")]
        public string? PhoneNumber { get; set; }
        public string? ClaimValue { get; set; }
    }
}
