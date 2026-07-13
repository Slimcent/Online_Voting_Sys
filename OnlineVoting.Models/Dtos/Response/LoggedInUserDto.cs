using OnlineVoting.Models.Dtos.Response.Jwt;

namespace OnlineVoting.Models.Dtos.Response
{
    public class LoggedInUserResponse
    {
        public JwtToken? JwtToken { get; set; }
        public string? UserType { get; set; }
        public string? FullName { get; set; }
    }
}
