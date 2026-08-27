using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Tests.TestData.Data
{
    public static class RefreshTokenTestData
    {
        public const string RawRefreshToken = "test-refresh-token";
        public const string UserId = "user-id";
        public const string FamilyId = "family-id";

        public static RefreshTokenContext CreateContext(string userId = UserId)
        {
            return new RefreshTokenContext
            {
                UserId = userId
            };
        }

        public static RefreshTokenRotationRequest CreateRotationRequest(string refreshToken = RawRefreshToken)
        {
            return new RefreshTokenRotationRequest
            {
                RefreshToken = refreshToken
            };
        }

        public static RefreshTokenRevocationRequest CreateRevocationRequest(string refreshToken = RawRefreshToken, string reason = "Test revocation.")
        {
            return new RefreshTokenRevocationRequest
            {
                RefreshToken = refreshToken,
                Reason = reason
            };
        }

        public static TokenRevocationContext CreateTokenRevocationContext(string reason = "Test revocation.")
        {
            return new TokenRevocationContext
            {
                Reason = reason
            };
        }

        public static RefreshToken CreateRefreshToken(string userId = UserId, string familyId = FamilyId, DateTime? expiresAt = null,
            DateTime? familyExpiresAt = null, DateTime? revokedAt = null)
        {
            return new RefreshToken
            {
                TokenHash = string.Empty,
                FamilyId = familyId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(30),
                FamilyExpiresAt = familyExpiresAt ?? DateTime.UtcNow.AddDays(90),
                RevokedAt = revokedAt,
                UserId = userId
            };
        }

        public static User CreateUser(string userId = UserId, bool active = true)
        {
            return new User
            {
                Id = userId,
                UserName = "user@example.com",
                Email = "user@example.com",
                Active = active
            };
        }
    }
}