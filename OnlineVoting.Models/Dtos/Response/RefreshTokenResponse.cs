namespace OnlineVoting.Models.Dtos.Response
{
    public class RefreshTokenResponse
    {
        public required string RefreshToken { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}