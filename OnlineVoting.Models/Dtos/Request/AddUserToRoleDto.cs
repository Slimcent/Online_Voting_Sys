using System.ComponentModel.DataAnnotations;

namespace OnlineVoting.Models.Dtos.Request
{
    public class AddUserToRoleDto
    {
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Role Name cannot be empty"), MinLength(2), MaxLength(50)]
        public string? Name { get; set; }
    }
}
