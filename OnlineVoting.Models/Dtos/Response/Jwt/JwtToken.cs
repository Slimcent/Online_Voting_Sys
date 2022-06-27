namespace OnlineVoting.Models.Dtos.Response.Jwt
{
    public class JwtToken
    {
        public string? Token { get; set; }
        public DateTime Issued { get; set; }
        public DateTime? Expires { get; set; }
    }
}
