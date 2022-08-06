using OnlineVoting.Models.Entities;

namespace OnlineVoting.Models.Dtos.Request
{
    public class UserMailDto
    {
        public string? FirstName { get; set; }
        public User? User { get; set;}
    }
}
