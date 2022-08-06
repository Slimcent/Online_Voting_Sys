using OnlineVoting.Models.Entities;

namespace OnlineVoting.Models.Dtos.Request.Email
{
    public class UserMailDto
    {
        public string? FirstName { get; set; }
        public User? User { get; set; }
    }
}
