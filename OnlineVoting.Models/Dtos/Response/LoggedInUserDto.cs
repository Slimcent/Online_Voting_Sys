using OnlineVoting.Models.Dtos.Response.Jwt;

namespace OnlineVoting.Models.Dtos.Response
{
    public class LoggedInUserDto
    {
        public JwtToken? JwtToken { get; set; }
        public string? UserType { get; set; }
        public string? FullName { get; set; }
    }
}
