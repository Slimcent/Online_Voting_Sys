namespace OnlineVoting.Models.Dtos.Request
{
    public class RefreshTokenContext
    {
        public required string UserId { get; set; }
    }

    public class RefreshTokenRotationRequest
    {
        public required string RefreshToken { get; set; }
    }

    public class RefreshTokenRevocationRequest
    {
        public required string RefreshToken { get; set; }

        public required string Reason { get; set; }
    }

    public class TokenRevocationContext
    {
        public required string Reason { get; set; }
    }
}