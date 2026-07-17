namespace OnlineVoting.Services.Infrastructures.Authorization.Jwt
{
    public class JwtSettings
    {
        public string? Secret { get; set; }
        public string Expires { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
    }
}
