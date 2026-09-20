using Microsoft.EntityFrameworkCore;
using Moq;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Dtos.Response.Jwt;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Results;
using OnlineVoting.Tests.TestData.Data;
using OnlineVoting.Tests.TestData.Factories;
using System.Security.Cryptography;
using System.Text;

namespace OnlineVoting.Tests.UnitTests.Services
{
    public class RefreshTokenServiceTests
    {
        [Fact]
        public async Task CreateRefreshToken_WithValidRequest_ShouldCreateRefreshToken()
        {
            RefreshTokenServiceFactory factory = new();

            RefreshTokenContext request = RefreshTokenTestData.CreateContext();
            RefreshToken refreshToken = RefreshTokenTestData.CreateRefreshToken();

            factory.Mapper.Setup(mapper => mapper.Map<RefreshToken>(request))
                .Returns(refreshToken);

            factory.RefreshTokenRepository.Setup(repository => repository.AddAsync(refreshToken, It.IsAny<bool>()))
                .ReturnsAsync(refreshToken);

            Result<RefreshTokenResponse> result = await factory.Service.CreateRefreshToken(request);

            Assert.Equal(ResultStatus.Created, result.Status);
            Assert.NotNull(result.Value);
            Assert.False(string.IsNullOrWhiteSpace(result.Value.RefreshToken));
            Assert.True(result.Value.ExpiresAt > DateTime.UtcNow);
            Assert.Equal(RefreshTokenTestData.UserId, refreshToken.UserId);
            Assert.False(string.IsNullOrWhiteSpace(refreshToken.TokenHash));
            Assert.False(string.IsNullOrWhiteSpace(refreshToken.FamilyId));
            Assert.True(refreshToken.ExpiresAt > refreshToken.CreatedAt);
            Assert.True(refreshToken.FamilyExpiresAt > refreshToken.ExpiresAt);

            factory.RefreshTokenRepository.Verify(repository => repository.AddAsync(refreshToken, It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task ValidateRefreshToken_WithEmptyToken_ShouldReturnUnauthorized()
        {
            RefreshTokenServiceFactory factory = new();

            Result<RefreshToken> result = await factory.Service.ValidateRefreshToken(" ");

            Assert.Equal(ResultStatus.Unauthorized, result.Status);
            Assert.Equal("Invalid refresh token.", result.Error);

            factory.RefreshTokenRepository.Verify(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task ValidateRefreshToken_WithUnknownToken_ShouldReturnUnauthorized()
        {
            RefreshTokenServiceFactory factory = new();

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync((RefreshToken?)null);

            Result<RefreshToken> result = await factory.Service.ValidateRefreshToken(RefreshTokenTestData.RawRefreshToken);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);
            Assert.Equal("Invalid refresh token.", result.Error);
        }

        [Fact]
        public async Task ValidateRefreshToken_WithRevokedToken_ShouldReturnUnauthorized()
        {
            RefreshTokenServiceFactory factory = new();

            RefreshToken token = RefreshTokenTestData.CreateRefreshToken(revokedAt: DateTime.UtcNow);

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(token);

            Result<RefreshToken> result = await factory.Service.ValidateRefreshToken(RefreshTokenTestData.RawRefreshToken);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);
            Assert.Equal("Refresh token has been revoked.", result.Error);
        }

        [Fact]
        public async Task ValidateRefreshToken_WithReusedRotatedToken_ShouldRevokeTokenFamily()
        {
            RefreshTokenServiceFactory factory = new();

            RefreshToken token = RefreshTokenTestData.CreateRefreshToken(revokedAt: DateTime.UtcNow);
            token.ReplacedByTokenHash = "replacement-token-hash";

            RefreshToken activeFamilyToken = RefreshTokenTestData.CreateRefreshToken();

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(token);

            factory.RefreshTokenRepository.Setup(repository => repository.GetByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>(),
                null,
                null,
                null,
                null,
                true))
                .ReturnsAsync(new List<RefreshToken> { activeFamilyToken });

            factory.RefreshTokenRepository.Setup(repository => repository.UpdateRangeAsync(It.IsAny<IEnumerable<RefreshToken>>()))
                .Returns(Task.CompletedTask);

            Result<RefreshToken> result = await factory.Service.ValidateRefreshToken(RefreshTokenTestData.RawRefreshToken);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);
            Assert.Equal("Refresh token has already been used.", result.Error);
            Assert.NotNull(activeFamilyToken.RevokedAt);
            Assert.Equal("Refresh token reuse detected.", activeFamilyToken.RevokedReason);

            factory.RefreshTokenRepository.Verify(repository => repository.UpdateRangeAsync(It.IsAny<IEnumerable<RefreshToken>>()), Times.Once);
        }

        [Fact]
        public async Task ValidateRefreshToken_WithExpiredFamily_ShouldReturnUnauthorized()
        {
            RefreshTokenServiceFactory factory = new();

            RefreshToken token = RefreshTokenTestData.CreateRefreshToken(expiresAt: DateTime.UtcNow.AddDays(1), familyExpiresAt: DateTime.UtcNow.AddMinutes(-1));

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(token);

            Result<RefreshToken> result = await factory.Service.ValidateRefreshToken(RefreshTokenTestData.RawRefreshToken);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);
            Assert.Equal("Refresh token session has expired.", result.Error);
        }

        [Fact]
        public async Task ValidateRefreshToken_WithExpiredToken_ShouldReturnUnauthorized()
        {
            RefreshTokenServiceFactory factory = new();

            RefreshToken token = RefreshTokenTestData.CreateRefreshToken(
                expiresAt: DateTime.UtcNow.AddMinutes(-1),
                familyExpiresAt: DateTime.UtcNow.AddDays(30));

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(token);

            Result<RefreshToken> result = await factory.Service.ValidateRefreshToken(RefreshTokenTestData.RawRefreshToken);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);
            Assert.Equal("Refresh token has expired.", result.Error);
        }

        [Fact]
        public async Task ValidateRefreshToken_WithValidToken_ShouldReturnToken()
        {
            RefreshTokenServiceFactory factory = new();

            RefreshToken token = RefreshTokenTestData.CreateRefreshToken();

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(token);

            Result<RefreshToken> result = await factory.Service.ValidateRefreshToken(RefreshTokenTestData.RawRefreshToken);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Same(token, result.Value);
        }

        [Fact]
        public async Task RotateRefreshToken_WithEmptyToken_ShouldReturnUnauthorized()
        {
            RefreshTokenServiceFactory factory = new();

            RefreshTokenRotationRequest request = RefreshTokenTestData.CreateRotationRequest(" ");

            Result<RefreshTokenResponse> result = await factory.Service.RotateRefreshToken(request);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);
            Assert.Equal("Invalid refresh token.", result.Error);

            factory.RefreshTokenRepository.Verify(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task RotateRefreshToken_WithUnknownToken_ShouldReturnUnauthorized()
        {
            RefreshTokenServiceFactory factory = new();

            RefreshTokenRotationRequest request = RefreshTokenTestData.CreateRotationRequest();

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync((RefreshToken?)null);

            Result<RefreshTokenResponse> result = await factory.Service.RotateRefreshToken(request);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);
            Assert.Equal("Invalid refresh token.", result.Error);
        }

        [Fact]
        public async Task RotateRefreshToken_WithRevokedToken_ShouldRevokeTokenFamily()
        {
            RefreshTokenServiceFactory factory = new();

            RefreshTokenRotationRequest request = RefreshTokenTestData.CreateRotationRequest();

            RefreshToken existingToken = RefreshTokenTestData.CreateRefreshToken(revokedAt: DateTime.UtcNow);
            RefreshToken activeFamilyToken = RefreshTokenTestData.CreateRefreshToken();

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(existingToken);

            factory.RefreshTokenRepository.Setup(repository => repository.GetByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>(),
                null,
                null,
                null,
                null,
                true))
                .ReturnsAsync(new List<RefreshToken> { activeFamilyToken });

            factory.RefreshTokenRepository.Setup(repository => repository.UpdateRangeAsync(It.IsAny<IEnumerable<RefreshToken>>()))
                .Returns(Task.CompletedTask);

            Result<RefreshTokenResponse> result = await factory.Service.RotateRefreshToken(request);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);
            Assert.Equal("Refresh token has already been used.", result.Error);
            Assert.NotNull(activeFamilyToken.RevokedAt);
            Assert.Equal("Refresh token reuse detected.", activeFamilyToken.RevokedReason);

            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(), Times.Never);

            factory.RefreshTokenRepository.Verify(repository => repository.UpdateRangeAsync(It.IsAny<IEnumerable<RefreshToken>>()), Times.Once);
        }

        [Fact]
        public async Task RotateRefreshToken_WithExpiredFamily_ShouldReturnUnauthorized()
        {
            RefreshTokenServiceFactory factory = new();

            RefreshTokenRotationRequest request = RefreshTokenTestData.CreateRotationRequest();

            RefreshToken token = RefreshTokenTestData.CreateRefreshToken(expiresAt: DateTime.UtcNow.AddDays(1), familyExpiresAt: DateTime.UtcNow.AddMinutes(-1));

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(token);

            Result<RefreshTokenResponse> result = await factory.Service.RotateRefreshToken(request);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);
            Assert.Equal("Refresh token session has expired.", result.Error);

            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task RotateRefreshToken_WithExpiredToken_ShouldReturnUnauthorized()
        {
            RefreshTokenServiceFactory factory = new();

            RefreshTokenRotationRequest request = RefreshTokenTestData.CreateRotationRequest();

            RefreshToken token = RefreshTokenTestData.CreateRefreshToken(expiresAt: DateTime.UtcNow.AddMinutes(-1), familyExpiresAt: DateTime.UtcNow.AddDays(30));

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(token);

            Result<RefreshTokenResponse> result = await factory.Service.RotateRefreshToken(request);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);
            Assert.Equal("Refresh token has expired.", result.Error);

            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task RotateRefreshToken_WithValidToken_ShouldRotateToken()
        {
            RefreshTokenServiceFactory factory = new();

            RefreshTokenRotationRequest request = RefreshTokenTestData.CreateRotationRequest();

            RefreshToken existingToken = RefreshTokenTestData.CreateRefreshToken();
            RefreshToken replacementToken = RefreshTokenTestData.CreateRefreshToken();

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(existingToken);

            factory.Mapper.Setup(mapper => mapper.Map<RefreshToken>(It.IsAny<RefreshTokenContext>()))
                .Returns(replacementToken);

            factory.RefreshTokenRepository.Setup(repository => repository.UpdateAsync(existingToken, It.IsAny<bool>()))
                .ReturnsAsync(existingToken);

            factory.RefreshTokenRepository.Setup(repository => repository.AddAsync(replacementToken, It.IsAny<bool>()))
                .ReturnsAsync(replacementToken);

            Result<RefreshTokenResponse> result = await factory.Service.RotateRefreshToken(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.False(string.IsNullOrWhiteSpace(result.Value.RefreshToken));

            Assert.NotNull(existingToken.RevokedAt);
            Assert.Equal("Replaced by a new refresh token.", existingToken.RevokedReason);
            Assert.False(string.IsNullOrWhiteSpace(existingToken.ReplacedByTokenHash));

            Assert.Equal(existingToken.UserId, replacementToken.UserId);
            Assert.Equal(existingToken.FamilyId, replacementToken.FamilyId);
            Assert.Equal(existingToken.FamilyExpiresAt, replacementToken.FamilyExpiresAt);
            Assert.False(string.IsNullOrWhiteSpace(replacementToken.TokenHash));

            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(), Times.Once);
            factory.RefreshTokenRepository.Verify(repository => repository.UpdateAsync(existingToken, It.IsAny<bool>()), Times.Once);
            factory.RefreshTokenRepository.Verify(repository => repository.AddAsync(replacementToken, It.IsAny<bool>()), Times.Once);
            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.CommitTransactionAsync(), Times.Once);
            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.RollbackTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task RotateRefreshToken_WhenConcurrencyConflictOccurs_ShouldRollbackAndRevokeTokenFamily()
        {
            RefreshTokenServiceFactory factory = new();

            RefreshTokenRotationRequest request = RefreshTokenTestData.CreateRotationRequest();

            RefreshToken existingToken = RefreshTokenTestData.CreateRefreshToken();
            RefreshToken replacementToken = RefreshTokenTestData.CreateRefreshToken();
            RefreshToken activeFamilyToken = RefreshTokenTestData.CreateRefreshToken();

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(existingToken);

            factory.Mapper.Setup(mapper => mapper.Map<RefreshToken>(It.IsAny<RefreshTokenContext>()))
                .Returns(replacementToken);

            factory.RefreshTokenRepository.Setup(repository => repository.UpdateAsync(existingToken, It.IsAny<bool>()))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            factory.RefreshTokenRepository.Setup(repository => repository.GetByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>(),
                null,
                null,
                null,
                null,
                true))
                .ReturnsAsync(new List<RefreshToken> { activeFamilyToken });

            factory.RefreshTokenRepository.Setup(repository => repository.UpdateRangeAsync(It.IsAny<IEnumerable<RefreshToken>>()))
                .Returns(Task.CompletedTask);

            Result<RefreshTokenResponse> result = await factory.Service.RotateRefreshToken(request);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);
            Assert.Equal("Refresh token has already been used.", result.Error);
            Assert.NotNull(activeFamilyToken.RevokedAt);
            Assert.Equal("Concurrent refresh token reuse detected.", activeFamilyToken.RevokedReason);

            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(), Times.Once);
            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.RollbackTransactionAsync(), Times.Once);
            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.CommitTransactionAsync(), Times.Never);

            factory.RefreshTokenRepository.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<bool>()), Times.Never);

            factory.RefreshTokenRepository.Verify(repository => repository.UpdateRangeAsync(It.IsAny<IEnumerable<RefreshToken>>()), Times.Once);
        }

        [Fact]
        public async Task RotateRefreshToken_WhenReplacementCreationFails_ShouldRollbackTransaction()
        {
            RefreshTokenServiceFactory factory = new();

            RefreshTokenRotationRequest request = RefreshTokenTestData.CreateRotationRequest();

            RefreshToken existingToken = RefreshTokenTestData.CreateRefreshToken();
            RefreshToken replacementToken = RefreshTokenTestData.CreateRefreshToken();

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(existingToken);

            factory.Mapper.Setup(mapper => mapper.Map<RefreshToken>(It.IsAny<RefreshTokenContext>()))
                .Returns(replacementToken);

            factory.RefreshTokenRepository.Setup(repository => repository.UpdateAsync(existingToken, It.IsAny<bool>()))
                .ReturnsAsync(existingToken);

            factory.RefreshTokenRepository.Setup(repository => repository.AddAsync(replacementToken, It.IsAny<bool>()))
                .ThrowsAsync(new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => factory.Service.RotateRefreshToken(request));

            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(), Times.Once);
            factory.RefreshTokenRepository.Verify(repository => repository.UpdateAsync(existingToken, It.IsAny<bool>()), Times.Once);
            factory.RefreshTokenRepository.Verify(repository => repository.AddAsync(replacementToken, It.IsAny<bool>()), Times.Once);
            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.RollbackTransactionAsync(), Times.Once);
            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.CommitTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task RevokeRefreshToken_WithValidToken_ShouldRevokeToken()
        {
            RefreshTokenServiceFactory factory = new();

            RefreshTokenRevocationRequest request = RefreshTokenTestData.CreateRevocationRequest();
            RefreshToken token = RefreshTokenTestData.CreateRefreshToken();

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(token);

            factory.RefreshTokenRepository.Setup(repository => repository.UpdateAsync(token, It.IsAny<bool>()))
                .ReturnsAsync(token);

            Result<string> result = await factory.Service.RevokeRefreshToken(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(token.RevokedAt);
            Assert.Equal(request.Reason, token.RevokedReason);

            factory.RefreshTokenRepository.Verify(repository => repository.UpdateAsync(token, It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task RevokeRefreshToken_WhenAlreadyRevoked_ShouldReturnSuccessWithoutUpdating()
        {
            RefreshTokenServiceFactory factory = new();

            RefreshTokenRevocationRequest request = RefreshTokenTestData.CreateRevocationRequest();

            RefreshToken token = RefreshTokenTestData.CreateRefreshToken(revokedAt: DateTime.UtcNow);

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(token);

            Result<string> result = await factory.Service.RevokeRefreshToken(request);

            Assert.Equal(ResultStatus.Success, result.Status);

            factory.RefreshTokenRepository.Verify(repository => repository.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task RevokeTokenFamily_WithActiveTokens_ShouldRevokeAllTokens()
        {
            RefreshTokenServiceFactory factory = new();

            TokenRevocationContext request = RefreshTokenTestData.CreateTokenRevocationContext();

            List<RefreshToken> tokens =
            [
                RefreshTokenTestData.CreateRefreshToken(),
                RefreshTokenTestData.CreateRefreshToken()
            ];

            factory.RefreshTokenRepository.Setup(repository => repository.GetByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>(),
                null,
                null,
                null,
                null,
                true))
                .ReturnsAsync(tokens);

            factory.RefreshTokenRepository.Setup(repository => repository.UpdateRangeAsync(It.IsAny<IEnumerable<RefreshToken>>()))
                .Returns(Task.CompletedTask);

            Result<string> result = await factory.Service.RevokeTokenFamily(RefreshTokenTestData.FamilyId, request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Token family revoked successfully.", result.Value);

            Assert.All(tokens, token =>
            {
                Assert.NotNull(token.RevokedAt);
                Assert.Equal(request.Reason, token.RevokedReason);
            });

            factory.RefreshTokenRepository.Verify(repository => repository.UpdateRangeAsync(It.Is<IEnumerable<RefreshToken>>(items =>
                items.Count() == 2)), Times.Once);
        }

        [Fact]
        public async Task RevokeAllUserTokens_WithActiveTokens_ShouldRevokeAllTokens()
        {
            RefreshTokenServiceFactory factory = new();

            TokenRevocationContext request = RefreshTokenTestData.CreateTokenRevocationContext();

            List<RefreshToken> tokens =
            [
                RefreshTokenTestData.CreateRefreshToken(),
                RefreshTokenTestData.CreateRefreshToken()
            ];

            factory.RefreshTokenRepository.Setup(repository => repository.GetByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>(),
                null,
                null,
                null,
                null,
                true))
                .ReturnsAsync(tokens);

            factory.RefreshTokenRepository.Setup(repository =>
                repository.UpdateRangeAsync(It.IsAny<IEnumerable<RefreshToken>>()))
                .Returns(Task.CompletedTask);

            Result<string> result = await factory.Service.RevokeAllUserTokens(RefreshTokenTestData.UserId, request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("All refresh tokens revoked successfully.", result.Value);

            Assert.All(tokens, token =>
            {
                Assert.NotNull(token.RevokedAt);
                Assert.Equal(request.Reason, token.RevokedReason);
            });

            factory.RefreshTokenRepository.Verify(repository => repository.UpdateRangeAsync(It.Is<IEnumerable<RefreshToken>>(items =>
                items.Count() == 2)), Times.Once);
        }

        [Fact]
        public async Task RefreshAccessToken_WithoutRefreshTokenCookie_ShouldReturnUnauthorized()
        {
            RefreshTokenServiceFactory factory = new();

            Result<JwtToken> result = await factory.Service.RefreshAccessToken();

            Assert.Equal(ResultStatus.Unauthorized, result.Status);
            Assert.Equal("Invalid refresh token.", result.Error);

            factory.RefreshTokenRepository.Verify(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()),
                Times.Never);

            factory.UserManager.Verify(userManager => userManager.FindByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RefreshAccessToken_WhenUserDoesNotExist_ShouldReturnUnauthorized()
        {
            RefreshTokenServiceFactory factory = new();

            factory.HttpContext.Request.Headers.Cookie = $"refreshToken={RefreshTokenTestData.RawRefreshToken}";

            RefreshToken token = RefreshTokenTestData.CreateRefreshToken();

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(token);

            factory.UserManager.Setup(userManager => userManager.FindByIdAsync(token.UserId))
                .ReturnsAsync((User?)null);

            factory.RefreshTokenRepository.Setup(repository => repository.GetByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>(),
                null, null, null, null, true))
                .ReturnsAsync(new List<RefreshToken> { token });

            Result<JwtToken> result = await factory.Service.RefreshAccessToken();

            Assert.Equal(ResultStatus.Unauthorized, result.Status);
            Assert.Equal("Invalid refresh token.", result.Error);
            Assert.NotNull(token.RevokedAt);
            Assert.Equal("User no longer exists.", token.RevokedReason);

            factory.RefreshTokenRepository.Verify(repository => repository.UpdateRangeAsync(It.IsAny<IEnumerable<RefreshToken>>()), Times.Once);
        }

        [Fact]
        public async Task RefreshAccessToken_WhenUserIsInactive_ShouldReturnForbidden()
        {
            RefreshTokenServiceFactory factory = new();

            factory.HttpContext.Request.Headers.Cookie = $"refreshToken={RefreshTokenTestData.RawRefreshToken}";

            RefreshToken token = RefreshTokenTestData.CreateRefreshToken();
            User user = RefreshTokenTestData.CreateUser(active: false);

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(token);

            factory.UserManager.Setup(userManager => userManager.FindByIdAsync(token.UserId))
                .ReturnsAsync(user);

            factory.RefreshTokenRepository.Setup(repository => repository.GetByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>(),
                null, null, null, null, true))
                .ReturnsAsync(new List<RefreshToken> { token });

            Result<JwtToken> result = await factory.Service.RefreshAccessToken();

            Assert.Equal(ResultStatus.Forbidden, result.Status);
            Assert.Equal("Account is not active. Contact the administrator.", result.Error);
            Assert.NotNull(token.RevokedAt);
            Assert.Equal("User account is inactive.", token.RevokedReason);

            factory.RefreshTokenRepository.Verify(repository => repository.UpdateRangeAsync(It.IsAny<IEnumerable<RefreshToken>>()), Times.Once);

            factory.JwtAuthenticator.Verify(jwtAuthenticator => jwtAuthenticator.GenerateJwtToken(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RefreshAccessToken_WhenUserHasNoRole_ShouldReturnForbidden()
        {
            RefreshTokenServiceFactory factory = new();

            factory.HttpContext.Request.Headers.Cookie = $"refreshToken={RefreshTokenTestData.RawRefreshToken}";

            RefreshToken token = RefreshTokenTestData.CreateRefreshToken();
            User user = RefreshTokenTestData.CreateUser();

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(token);

            factory.UserManager.Setup(userManager => userManager.FindByIdAsync(token.UserId))
                .ReturnsAsync(user);

            factory.UserManager.Setup(userManager => userManager.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            Result<JwtToken> result = await factory.Service.RefreshAccessToken();

            Assert.Equal(ResultStatus.Forbidden, result.Status);
            Assert.Equal("User has no assigned role.", result.Error);

            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(), Times.Never);

            factory.JwtAuthenticator.Verify(jwtAuthenticator => jwtAuthenticator.GenerateJwtToken(It.IsAny<User>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task RefreshAccessToken_WithValidRefreshToken_ShouldReturnNewAccessToken()
        {
            RefreshTokenServiceFactory factory = new();

            factory.HttpContext.Request.Headers.Cookie = $"refreshToken={RefreshTokenTestData.RawRefreshToken}";

            RefreshToken existingToken = RefreshTokenTestData.CreateRefreshToken();
            RefreshToken replacementToken = RefreshTokenTestData.CreateRefreshToken();
            User user = RefreshTokenTestData.CreateUser();

            string role = "Admin";

            JwtToken jwtToken = new()
            {
                Token = "new-access-token",
                Issuer = "OnlineVoting",
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(30)
            };

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(existingToken);

            factory.UserManager.Setup(userManager => userManager.FindByIdAsync(existingToken.UserId))
                .ReturnsAsync(user);

            factory.UserManager.Setup(userManager => userManager.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { role });

            factory.Mapper.Setup(mapper => mapper.Map<RefreshToken>(It.IsAny<RefreshTokenContext>()))
                .Returns(replacementToken);

            factory.RefreshTokenRepository.Setup(repository => repository.UpdateAsync(existingToken, It.IsAny<bool>()))
                .ReturnsAsync(existingToken);

            factory.RefreshTokenRepository.Setup(repository => repository.AddAsync(replacementToken, It.IsAny<bool>()))
                .ReturnsAsync(replacementToken);

            factory.JwtAuthenticator.Setup(jwtAuthenticator => jwtAuthenticator.GenerateJwtToken(user, role))
                .ReturnsAsync(jwtToken);

            Result<JwtToken> result = await factory.Service.RefreshAccessToken();

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Same(jwtToken, result.Value);
            Assert.Equal("new-access-token", result.Value!.Token);

            Assert.NotNull(existingToken.RevokedAt);
            Assert.Equal("Replaced by a new refresh token.", existingToken.RevokedReason);
            Assert.False(string.IsNullOrWhiteSpace(existingToken.ReplacedByTokenHash));

            Assert.Equal(existingToken.UserId, replacementToken.UserId);
            Assert.Equal(existingToken.FamilyId, replacementToken.FamilyId);
            Assert.Equal(existingToken.FamilyExpiresAt, replacementToken.FamilyExpiresAt);

            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(), Times.Once);

            factory.RefreshTokenRepository.Verify(repository => repository.UpdateAsync(existingToken, It.IsAny<bool>()), Times.Once);

            factory.RefreshTokenRepository.Verify(repository => repository.AddAsync(replacementToken, It.IsAny<bool>()), Times.Once);

            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.CommitTransactionAsync(), Times.Once);

            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.RollbackTransactionAsync(), Times.Never);

            factory.JwtAuthenticator.Verify(jwtAuthenticator => jwtAuthenticator.GenerateJwtToken(user, role), Times.Once);
        }

        [Fact]
        public async Task RevokeCurrentRefreshToken_WithoutRefreshTokenCookie_ShouldReturnUnauthorized()
        {
            RefreshTokenServiceFactory factory = new();

            Result<string> result = await factory.Service.RevokeCurrentRefreshToken();

            Assert.Equal(ResultStatus.Unauthorized, result.Status);
            Assert.Equal("Invalid refresh token.", result.Error);

            factory.RefreshTokenRepository.Verify(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()),
                Times.Never);
        }

        [Fact]
        public async Task RevokeCurrentRefreshToken_WithValidRefreshToken_ShouldRevokeToken()
        {
            RefreshTokenServiceFactory factory = new();

            factory.HttpContext.Request.Headers.Cookie = $"refreshToken={RefreshTokenTestData.RawRefreshToken}";

            RefreshToken token = RefreshTokenTestData.CreateRefreshToken();

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(token);

            factory.RefreshTokenRepository.Setup(repository => repository.UpdateAsync(token, It.IsAny<bool>()))
                .ReturnsAsync(token);

            Result<string> result = await factory.Service.RevokeCurrentRefreshToken();

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Refresh token revoked successfully.", result.Value);

            Assert.NotNull(token.RevokedAt);
            Assert.Equal("User logged out.", token.RevokedReason);

            factory.RefreshTokenRepository.Verify(repository => repository.UpdateAsync(token, It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task RevokeAllCurrentUserTokens_WithoutRefreshTokenCookie_ShouldReturnUnauthorized()
        {
            RefreshTokenServiceFactory factory = new();

            Result<string> result = await factory.Service.RevokeAllCurrentUserTokens();

            Assert.Equal(ResultStatus.Unauthorized, result.Status);
            Assert.Equal("Invalid refresh token.", result.Error);

            factory.RefreshTokenRepository.Verify(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()),
                Times.Never);
        }

        [Fact]
        public async Task RevokeAllCurrentUserTokens_WithValidRefreshToken_ShouldRevokeAllUserTokens()
        {
            RefreshTokenServiceFactory factory = new();

            factory.HttpContext.Request.Headers.Cookie = $"refreshToken={RefreshTokenTestData.RawRefreshToken}";

            RefreshToken currentToken = RefreshTokenTestData.CreateRefreshToken();

            List<RefreshToken> userTokens =
            [
                currentToken,
                RefreshTokenTestData.CreateRefreshToken()
            ];

            factory.RefreshTokenRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                .ReturnsAsync(currentToken);

            factory.RefreshTokenRepository.Setup(repository => repository.GetByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>(),
                null, null, null, null, true))
                .ReturnsAsync(userTokens);

            Result<string> result = await factory.Service.RevokeAllCurrentUserTokens();

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("All refresh tokens revoked successfully.", result.Value);

            Assert.All(userTokens, token =>
            {
                Assert.NotNull(token.RevokedAt);
                Assert.Equal("User logged out from all sessions.", token.RevokedReason);
            });

            factory.RefreshTokenRepository.Verify(repository => repository.UpdateRangeAsync(It.Is<IEnumerable<RefreshToken>>(tokens =>
                tokens.Count() == 2)), Times.Once);
        }
    }
}