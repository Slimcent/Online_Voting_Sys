namespace OnlineVoting.Models.Dtos.Request
{
    public class UserClaimsRequestDto
    {
        public string? Email { get; set; }
        public string? ClaimType { get; set; }
        public string? ClaimValue { get; set; }
    }
}
