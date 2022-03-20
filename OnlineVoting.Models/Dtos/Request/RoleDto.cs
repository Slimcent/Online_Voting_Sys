using System.ComponentModel.DataAnnotations;

namespace OnlineVoting.Models.Dtos.Request
{
    public class RoleDto
    {
        [Required(ErrorMessage = "Role Name cannot be empty"), MinLength(2), MaxLength(30)]
        public string? Name { get; set; }
    }
}
