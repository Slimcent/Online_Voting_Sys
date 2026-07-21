using System.ComponentModel.DataAnnotations;

namespace OnlineVoting.Models.Dtos.Request
{
    public class AddUserToRoleRequest : CreateWithNameRequest
    {
        public required string Email { get; set; }

    }
}