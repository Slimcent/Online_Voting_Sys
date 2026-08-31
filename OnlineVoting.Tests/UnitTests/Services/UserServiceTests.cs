using Microsoft.AspNetCore.Identity;
using Moq;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Dtos.Response.Jwt;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Results;
using OnlineVoting.Tests.TestData.Factories;
using System.Security.Claims;

namespace OnlineVoting.Tests.UnitTests.Services
{
    public class UserServiceTests
    {
        [Fact]
        public async Task UserLogin_UnknownEmail_ShouldRecordFailedLogin()
        {
            UserServiceFactory factory = new();

            LoginRequest request = LoginRequestFactory.CreateValid();

            factory.SetLoginUser(null);

            Result<LoggedInUserResponse> result = await factory.Service.UserLogin(request);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);

            factory.AuditTrailService.Verify(service => service.RecordAuthenticationEvent(ApplicationConstants.Audit.Events.LoginFailed,
                ApplicationConstants.Audit.Outcomes.Failure, ApplicationConstants.Audit.Descriptions.InvalidCredentials,
                null, request.Email.Trim().ToLowerInvariant()), Times.Once);
        }

        [Fact]
        public async Task UserLogin_InactiveUser_ShouldRecordDeniedLogin()
        {
            UserServiceFactory factory = new();

            LoginRequest request = LoginRequestFactory.CreateValid();

            User user = new()
            {
                Id = "user-id",
                Email = request.Email.Trim().ToLowerInvariant(),
                Active = false
            };

            factory.SetLoginUser(user);

            Result<LoggedInUserResponse> result = await factory.Service.UserLogin(request);

            Assert.Equal(ResultStatus.Forbidden, result.Status);

            factory.AuditTrailService.Verify(service => service.RecordAuthenticationEvent(ApplicationConstants.Audit.Events.LoginFailed,
                ApplicationConstants.Audit.Outcomes.Denied, ApplicationConstants.Audit.Descriptions.InactiveAccount,
                user, null), Times.Once);
        }

        [Fact]
        public async Task UserLogin_LockedUser_ShouldRecordRejectedLockedLogin()
        {
            UserServiceFactory factory = new();

            LoginRequest request = LoginRequestFactory.CreateValid();

            User user = new()
            {
                Id = "user-id",
                Email = request.Email.Trim().ToLowerInvariant(),
                Active = true
            };

            factory.SetLoginUser(user);

            factory.UserManager.Setup(manager => manager.IsLockedOutAsync(user))
                .ReturnsAsync(true);

            Result<LoggedInUserResponse> result = await factory.Service.UserLogin(request);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);

            factory.AuditTrailService.Verify(service => service.RecordAuthenticationEvent(ApplicationConstants.Audit.Events.LoginRejectedLocked,
                ApplicationConstants.Audit.Outcomes.Denied, ApplicationConstants.Audit.Descriptions.LoginRejectedLocked,
                user, null), Times.Once);
        }

        [Fact]
        public async Task UserLogin_InvalidPassword_ShouldRecordFailedLogin()
        {
            UserServiceFactory factory = new();

            LoginRequest request = LoginRequestFactory.CreateValid();

            User user = new()
            {
                Id = "user-id",
                Email = request.Email.Trim().ToLowerInvariant(),
                Active = true
            };

            factory.SetLoginUser(user);

            factory.UserManager.Setup(manager => manager.IsLockedOutAsync(user))
                .ReturnsAsync(false);

            factory.SignInManager.Setup(manager => manager.CheckPasswordSignInAsync(user, request.Password, true))
                .ReturnsAsync(SignInResult.Failed);

            Result<LoggedInUserResponse> result = await factory.Service.UserLogin(request);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);

            factory.AuditTrailService.Verify(service => service.RecordAuthenticationEvent(ApplicationConstants.Audit.Events.LoginFailed,
                ApplicationConstants.Audit.Outcomes.Failure, ApplicationConstants.Audit.Descriptions.InvalidCredentials,
                user, null), Times.Once);
        }

        [Fact]
        public async Task UserLogin_LockoutTriggered_ShouldRecordAccountLocked()
        {
            UserServiceFactory factory = new();

            LoginRequest request = LoginRequestFactory.CreateValid();

            User user = new()
            {
                Id = "user-id",
                Email = request.Email.Trim().ToLowerInvariant(),
                Active = true
            };

            factory.SetLoginUser(user);

            factory.UserManager.Setup(manager => manager.IsLockedOutAsync(user))
                .ReturnsAsync(false);

            factory.SignInManager.Setup(manager => manager.CheckPasswordSignInAsync(user, request.Password, true))
                .ReturnsAsync(SignInResult.LockedOut);

            Result<LoggedInUserResponse> result = await factory.Service.UserLogin(request);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);

            factory.AuditTrailService.Verify(service => service.RecordAuthenticationEvent(ApplicationConstants.Audit.Events.AccountLocked,
                ApplicationConstants.Audit.Outcomes.Denied, ApplicationConstants.Audit.Descriptions.AccountLocked,
                user, null), Times.Once);
        }

        [Fact]
        public async Task UserLogin_SuccessfulLogin_ShouldRecordSuccessfulLogin()
        {
            UserServiceFactory factory = new();

            LoginRequest request = LoginRequestFactory.CreateValid();

            User user = new()
            {
                Id = "user-id",
                Email = request.Email.Trim().ToLowerInvariant(),
                FirstName = "Test",
                LastName = "User",
                Active = true,
                UserType = new UserType
                {
                    Name = "Student"
                }
            };

            Role role = new()
            {
                Name = "Student"
            };

            JwtToken jwtToken = new()
            {
                Token = "access-token",
                Issuer = "test",
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(30)
            };

            RefreshTokenResponse refreshTokenResponse = new()
            {
                RefreshToken = "refresh-token",
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            IList<string> roles = new List<string>
            {
                "Student"
            };

            IList<Claim> userClaims = new List<Claim>();
            IList<Claim> roleClaims = new List<Claim>();

            factory.SetLoginUser(user);

            factory.UserManager.Setup(manager => manager.IsLockedOutAsync(user))
                .ReturnsAsync(false);

            factory.SignInManager.Setup(manager => manager.CheckPasswordSignInAsync(user, request.Password, true))
                .ReturnsAsync(SignInResult.Success);

            factory.UserManager.Setup(manager => manager.GetRolesAsync(user))
                .ReturnsAsync(roles);

            factory.UserManager.Setup(manager => manager.GetClaimsAsync(user))
                .ReturnsAsync(userClaims);

            factory.RoleManager.Setup(manager => manager.FindByNameAsync("Student"))
                .ReturnsAsync(role);

            factory.RoleManager.Setup(manager => manager.GetClaimsAsync(role))
                .ReturnsAsync(roleClaims);

            factory.JwtAuthenticator.Setup(authenticator => authenticator.GenerateJwtToken(user, "Student", null, null))
                .ReturnsAsync(jwtToken);

            factory.RefreshTokenService.Setup(service => service.CreateRefreshToken(It.IsAny<RefreshTokenContext>()))
                .ReturnsAsync(Result<RefreshTokenResponse>.Success(refreshTokenResponse));

            Result<LoggedInUserResponse> result = await factory.Service.UserLogin(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(jwtToken, result.Value.JwtToken);
            Assert.Equal("Student", result.Value.UserType);
            Assert.Equal("Test User", result.Value.FullName);

            factory.AuditTrailService.Verify(service => service.RecordAuthenticationEvent(ApplicationConstants.Audit.Events.LoginSucceeded,
                ApplicationConstants.Audit.Outcomes.Success, ApplicationConstants.Audit.Descriptions.LoginSucceeded,
                user, null), Times.Once);
        }
    }
}