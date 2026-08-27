using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Dtos.Response.Jwt;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using VotingSystem.Logger;

namespace OnlineVoting.Services.Implementation
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private const int RefreshTokenSize = 64;
        private const int RefreshTokenLifetimeDays = 30;
        private const int RefreshTokenFamilyLifetimeDays = 90;
        private readonly IServiceFactory _serviceFactory;
        private readonly IRepository<RefreshToken> _refreshTokenRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggerMessage _loggerMessage;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;
        private readonly IJwtAuthenticator _jwtAuthenticator;

        public RefreshTokenService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _refreshTokenRepo = _unitOfWork.GetRepository<RefreshToken>();
            _mapper = _serviceFactory.GetService<IMapper>();
            _loggerMessage = _serviceFactory.GetService<ILoggerMessage>();
            _httpContextAccessor = _serviceFactory.GetService<IHttpContextAccessor>();
            _userManager = _serviceFactory.GetService<UserManager<User>>();
            _jwtAuthenticator = _serviceFactory.GetService<IJwtAuthenticator>();
        }

        public async Task<Result<JwtToken>> RefreshAccessToken()
        {
            string? refreshToken = GetRefreshTokenCookie();

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                _loggerMessage.LogWarn("Access token refresh failed because no refresh token cookie was provided.");

                return Result<JwtToken>.Unauthorized("Invalid refresh token.");
            }

            Result<RefreshToken> validationResult = await ValidateRefreshToken(refreshToken);

            if (!validationResult.IsSuccess)
            {
                _loggerMessage.LogWarn("Access token refresh failed during refresh token validation because validate refresh token failed.");

                return Result<JwtToken>.FromFailure(validationResult);
            }

            RefreshToken token = validationResult.Value!;

            User? user = await _userManager.FindByIdAsync(token.UserId);

            if (user is null)
            {
                _loggerMessage.LogWarn($"Access token refresh failed because user {token.UserId} was not found.");

                TokenRevocationContext revocationRequest = new()
                {
                    Reason = "User no longer exists."
                };

                await RevokeTokenFamily(token.FamilyId, revocationRequest);

                return Result<JwtToken>.Unauthorized("Invalid refresh token.");
            }

            if (!user.Active)
            {
                _loggerMessage.LogWarn($"Access token refresh denied because user {user.Id} is inactive.");

                TokenRevocationContext revocationRequest = new()
                {
                    Reason = "User account is inactive."
                };

                await RevokeAllUserTokens(user.Id, revocationRequest);

                return Result<JwtToken>.Forbidden("Account is not active. Contact the administrator.");
            }

            List<string> userRoles = (await _userManager.GetRolesAsync(user)).ToList();

            string? userRole = userRoles.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(userRole))
            {
                _loggerMessage.LogWarn($"Access token refresh failed because user {user.Id} has no assigned role.");

                return Result<JwtToken>.Forbidden("User has no assigned role.");
            }

            RefreshTokenRotationRequest rotationRequest = new()
            {
                RefreshToken = refreshToken
            };

            Result<RefreshTokenResponse> rotationResult = await RotateRefreshToken(rotationRequest);

            if (!rotationResult.IsSuccess)
            {
                _loggerMessage.LogWarn($"Access token refresh failed during refresh token rotation for user {user.Id} because token rotation failed.");

                return Result<JwtToken>.FromFailure(rotationResult);
            }

            JwtToken jwtToken = await _jwtAuthenticator.GenerateJwtToken(user, userRole);

            _loggerMessage.LogInfo($"Access token refreshed successfully for user {user.Id}.");

            return Result<JwtToken>.Success(jwtToken);
        }

        public async Task<Result<string>> RevokeCurrentRefreshToken()
        {
            string? refreshToken = GetRefreshTokenCookie();

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                _loggerMessage.LogWarn("Refresh token revocation failed because no refresh token cookie was provided.");

                return Result<string>.Unauthorized("Invalid refresh token.");
            }

            RefreshTokenRevocationRequest request = new()
            {
                RefreshToken = refreshToken,
                Reason = "User logged out."
            };

            Result<string> result = await RevokeRefreshToken(request);

            if (!result.IsSuccess)
                return result;

            DeleteRefreshTokenCookie();

            return result;
        }

        public async Task<Result<string>> RevokeAllCurrentUserTokens()
        {
            string? refreshToken = GetRefreshTokenCookie();

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                _loggerMessage.LogWarn("All refresh token revocation failed because no refresh token cookie was provided.");

                return Result<string>.Unauthorized("Invalid refresh token.");
            }

            Result<RefreshToken> validationResult = await ValidateRefreshToken(refreshToken);

            if (!validationResult.IsSuccess)
                return Result<string>.FromFailure(validationResult);

            RefreshToken token = validationResult.Value!;

            TokenRevocationContext request = new()
            {
                Reason = "User logged out from all sessions."
            };

            Result<string> result = await RevokeAllUserTokens(token.UserId, request);

            if (!result.IsSuccess)
                return result;

            DeleteRefreshTokenCookie();

            _loggerMessage.LogInfo($"User {token.UserId} logged out from all sessions.");

            return result;
        }

        public async Task<Result<RefreshTokenResponse>> CreateRefreshToken(RefreshTokenContext request)
        {
            DateTime now = DateTime.UtcNow;

            string refreshToken = GenerateToken();
            string tokenHash = HashToken(refreshToken);

            RefreshToken token = _mapper.Map<RefreshToken>(request);

            token.TokenHash = tokenHash;
            token.FamilyId = Guid.NewGuid().ToString("N");
            token.CreatedAt = now;
            token.ExpiresAt = now.AddDays(RefreshTokenLifetimeDays);
            token.FamilyExpiresAt = now.AddDays(RefreshTokenFamilyLifetimeDays);
            token.CreatedByIp = GetIpAddress();
            token.UserAgent = GetUserAgent();

            await _refreshTokenRepo.AddAsync(token);

            SetRefreshTokenCookie(refreshToken, token.ExpiresAt);

            _loggerMessage.LogInfo($"Refresh token created for user {request.UserId} in family {token.FamilyId}.");

            RefreshTokenResponse response = new()
            {
                RefreshToken = refreshToken,
                ExpiresAt = token.ExpiresAt
            };

            return Result<RefreshTokenResponse>.Created(response);
        }

        public async Task<Result<RefreshToken>> ValidateRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                _loggerMessage.LogWarn("Refresh token validation failed because no token was provided.");

                return Result<RefreshToken>.Unauthorized("Invalid refresh token.");
            }

            string tokenHash = HashToken(refreshToken);

            RefreshToken? token = await _refreshTokenRepo.GetSingleByAsync(x => x.TokenHash == tokenHash);

            if (token is null)
            {
                _loggerMessage.LogWarn("Refresh token validation failed because the token was not found.");

                return Result<RefreshToken>.Unauthorized("Invalid refresh token.");
            }

            if (token.IsRevoked)
            {
                if (!string.IsNullOrWhiteSpace(token.ReplacedByTokenHash))
                {
                    _loggerMessage.LogWarn($"Refresh token reuse detected for user {token.UserId} in family {token.FamilyId}.");

                    TokenRevocationContext revocationRequest = new()
                    {
                        Reason = "Refresh token reuse detected."
                    };

                    await RevokeTokenFamily(token.FamilyId, revocationRequest);

                    return Result<RefreshToken>.Unauthorized("Refresh token has already been used.");
                }

                _loggerMessage.LogWarn($"Revoked refresh token presented for user {token.UserId} in family {token.FamilyId}.");

                return Result<RefreshToken>.Unauthorized("Refresh token has been revoked.");
            }

            if (token.IsFamilyExpired)
            {
                _loggerMessage.LogWarn($"Refresh token family {token.FamilyId} has expired for user {token.UserId}.");

                return Result<RefreshToken>.Unauthorized("Refresh token session has expired.");
            }

            if (token.IsExpired)
            {
                _loggerMessage.LogWarn($"Expired refresh token presented for user {token.UserId}.");

                return Result<RefreshToken>.Unauthorized("Refresh token has expired.");
            }

            _loggerMessage.LogInfo($"Refresh token validated successfully for user {token.UserId} in family {token.FamilyId}.");

            return Result<RefreshToken>.Success(token);
        }

        public async Task<Result<RefreshTokenResponse>> RotateRefreshToken(RefreshTokenRotationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                _loggerMessage.LogWarn("Refresh token rotation failed because no token was provided.");

                return Result<RefreshTokenResponse>.Unauthorized("Invalid refresh token.");
            }

            string tokenHash = HashToken(request.RefreshToken);

            RefreshToken? existingToken = await _refreshTokenRepo.GetSingleByAsync(x => x.TokenHash == tokenHash);

            if (existingToken is null)
            {
                _loggerMessage.LogWarn("Refresh token rotation failed because the token was not found.");

                return Result<RefreshTokenResponse>.Unauthorized("Invalid refresh token.");
            }

            if (existingToken.IsRevoked)
            {
                _loggerMessage.LogWarn($"Refresh token reuse detected for user {existingToken.UserId} in family {existingToken.FamilyId}.");

                TokenRevocationContext revocationRequest = new()
                {
                    Reason = "Refresh token reuse detected."
                };

                await RevokeTokenFamily(existingToken.FamilyId, revocationRequest);

                return Result<RefreshTokenResponse>.Unauthorized("Refresh token has already been used.");
            }

            if (existingToken.IsFamilyExpired)
            {
                _loggerMessage.LogWarn($"Refresh token rotation failed because family {existingToken.FamilyId} has expired for user {existingToken.UserId}.");

                return Result<RefreshTokenResponse>.Unauthorized("Refresh token session has expired.");
            }

            if (existingToken.IsExpired)
            {
                _loggerMessage.LogWarn($"Refresh token rotation failed because the token has expired for user {existingToken.UserId}.");

                return Result<RefreshTokenResponse>.Unauthorized("Refresh token has expired.");
            }

            DateTime now = DateTime.UtcNow;

            string newRefreshToken = GenerateToken();
            string newTokenHash = HashToken(newRefreshToken);

            DateTime newExpiresAt = now.AddDays(RefreshTokenLifetimeDays);

            if (newExpiresAt > existingToken.FamilyExpiresAt)
                newExpiresAt = existingToken.FamilyExpiresAt;

            existingToken.RevokedAt = now;
            existingToken.RevokedByIp = GetIpAddress();
            existingToken.RevokedReason = "Replaced by a new refresh token.";
            existingToken.ReplacedByTokenHash = newTokenHash;

            RefreshTokenContext replacementContext = new()
            {
                UserId = existingToken.UserId
            };

            RefreshToken replacementToken = _mapper.Map<RefreshToken>(replacementContext);

            replacementToken.TokenHash = newTokenHash;
            replacementToken.FamilyId = existingToken.FamilyId;
            replacementToken.CreatedAt = now;
            replacementToken.ExpiresAt = newExpiresAt;
            replacementToken.FamilyExpiresAt = existingToken.FamilyExpiresAt;
            replacementToken.CreatedByIp = GetIpAddress();
            replacementToken.UserAgent = GetUserAgent();

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _refreshTokenRepo.UpdateAsync(existingToken);

                await _refreshTokenRepo.AddAsync(replacementToken);

                await _unitOfWork.CommitTransactionAsync();

                SetRefreshTokenCookie(newRefreshToken, replacementToken.ExpiresAt);
            }
            catch (DbUpdateConcurrencyException)
            {
                await _unitOfWork.RollbackTransactionAsync();

                _loggerMessage.LogWarn($"Concurrent refresh token reuse detected for user {existingToken.UserId} in family {existingToken.FamilyId}.");

                TokenRevocationContext revocationRequest = new()
                {
                    Reason = "Concurrent refresh token reuse detected."
                };

                await RevokeTokenFamily(existingToken.FamilyId, revocationRequest);

                return Result<RefreshTokenResponse>.Unauthorized("Refresh token has already been used.");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();

                throw;
            }

            _loggerMessage.LogInfo($"Refresh token rotated successfully for user {existingToken.UserId} in family {existingToken.FamilyId}.");

            RefreshTokenResponse response = new()
            {
                RefreshToken = newRefreshToken,
                ExpiresAt = replacementToken.ExpiresAt
            };

            return Result<RefreshTokenResponse>.Success(response);
        }

        public async Task<Result<string>> RevokeRefreshToken(RefreshTokenRevocationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                _loggerMessage.LogWarn("Refresh token revocation failed because no token was provided.");

                return Result<string>.Unauthorized("Invalid refresh token.");
            }

            string tokenHash = HashToken(request.RefreshToken);

            RefreshToken? token = await _refreshTokenRepo.GetSingleByAsync(x => x.TokenHash == tokenHash);

            if (token is null)
            {
                _loggerMessage.LogWarn("Refresh token revocation failed because the token was not found.");

                return Result<string>.Unauthorized("Invalid refresh token.");
            }

            if (token.IsRevoked)
            {
                _loggerMessage.LogInfo($"Refresh token was already revoked for user {token.UserId} in family {token.FamilyId}.");

                return Result<string>.Success("Refresh token has already been revoked.");
            }

            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = GetIpAddress();
            token.RevokedReason = request.Reason;

            await _refreshTokenRepo.UpdateAsync(token);

            _loggerMessage.LogInfo($"Refresh token revoked for user {token.UserId} in family {token.FamilyId}. Reason: {request.Reason}");

            return Result<string>.Success("Refresh token revoked successfully.");
        }

        public async Task<Result<string>> RevokeTokenFamily(string familyId, TokenRevocationContext request)
        {
            IEnumerable<RefreshToken> refreshTokens = await _refreshTokenRepo.GetByAsync(predicate: token => token.FamilyId == familyId && !token.RevokedAt.HasValue, tracking: true);

            List<RefreshToken> activeTokens = refreshTokens.ToList();

            if (activeTokens.Count == 0)
            {
                _loggerMessage.LogInfo($"No active refresh tokens found for family {familyId}.");

                return Result<string>.Success("Token family already revoked.");
            }

            DateTime now = DateTime.UtcNow;
            string? ipAddress = GetIpAddress();

            foreach (RefreshToken token in activeTokens)
            {
                token.RevokedAt = now;
                token.RevokedByIp = ipAddress;
                token.RevokedReason = request.Reason;
            }

            await _refreshTokenRepo.UpdateRangeAsync(activeTokens);

            _loggerMessage.LogWarn($"Refresh token family {familyId} revoked. Reason: {request.Reason}");

            return Result<string>.Success("Token family revoked successfully.");
        }

        public async Task<Result<string>> RevokeAllUserTokens(string userId, TokenRevocationContext request)
        {
            IEnumerable<RefreshToken> refreshTokens = await _refreshTokenRepo.GetByAsync(token =>
                token.UserId == userId && !token.RevokedAt.HasValue, tracking: true);

            List<RefreshToken> activeTokens = refreshTokens.ToList();

            if (activeTokens.Count == 0)
            {
                _loggerMessage.LogInfo($"No active refresh tokens found for user {userId}.");

                return Result<string>.Success("No active refresh tokens found.");
            }

            DateTime now = DateTime.UtcNow;
            string? ipAddress = GetIpAddress();

            foreach (RefreshToken token in activeTokens)
            {
                token.RevokedAt = now;
                token.RevokedByIp = ipAddress;
                token.RevokedReason = request.Reason;
            }

            await _refreshTokenRepo.UpdateRangeAsync(activeTokens);

            _loggerMessage.LogWarn($"All refresh tokens revoked for user {userId}. Reason: {request.Reason}");

            return Result<string>.Success("All refresh tokens revoked successfully.");
        }

        private static string GenerateToken()
        {
            byte[] randomBytes = RandomNumberGenerator.GetBytes(RefreshTokenSize);

            return Convert.ToBase64String(randomBytes);
        }

        private static string HashToken(string token)
        {
            byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
            byte[] hashBytes = SHA256.HashData(tokenBytes);

            return Convert.ToHexString(hashBytes);
        }

        private string? GetIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        }

        private string? GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
        }

        private string? GetRefreshTokenCookie()
        {
            return _httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];
        }

        private void SetRefreshTokenCookie(string refreshToken, DateTime expiresAt)
        {
            CookieOptions cookieOptions = new()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = expiresAt,
                IsEssential = true
            };

            _httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        private void DeleteRefreshTokenCookie()
        {
            CookieOptions cookieOptions = new()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                IsEssential = true
            };

            _httpContextAccessor.HttpContext?.Response.Cookies.Delete("refreshToken", cookieOptions);
        }
    }
}