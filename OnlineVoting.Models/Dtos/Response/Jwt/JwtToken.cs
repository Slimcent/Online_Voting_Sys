namespace OnlineVoting.Models.Dtos.Response.Jwt
{
    public class JwtToken
    {
        public string? Token { get; set; }
        public string? Issuer { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime? Expires { get; set; }
    }
}
