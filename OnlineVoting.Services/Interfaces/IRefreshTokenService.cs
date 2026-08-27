using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Dtos.Response.Jwt;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<Result<RefreshTokenResponse>> CreateRefreshToken(RefreshTokenContext request);
        Task<Result<RefreshToken>> ValidateRefreshToken(string refreshToken);
        Task<Result<RefreshTokenResponse>> RotateRefreshToken(RefreshTokenRotationRequest request);
        Task<Result<JwtToken>> RefreshAccessToken();
        Task<Result<string>> RevokeCurrentRefreshToken();
        Task<Result<string>> RevokeAllCurrentUserTokens();
        Task<Result<string>> RevokeRefreshToken(RefreshTokenRevocationRequest request);
        Task<Result<string>> RevokeTokenFamily(string familyId, TokenRevocationContext request);
        Task<Result<string>> RevokeAllUserTokens(string userId, TokenRevocationContext request);
    }
}