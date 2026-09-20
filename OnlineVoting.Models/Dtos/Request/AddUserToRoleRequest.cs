using System.ComponentModel.DataAnnotations;

namespace OnlineVoting.Models.Dtos.Request
{
    public class AddUserToRoleRequest
    {
        public required string Email { get; set; }

        public required string RoleId { get; set; }
    }
}