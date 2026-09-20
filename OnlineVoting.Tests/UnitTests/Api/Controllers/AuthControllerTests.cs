using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OnlineVoting.Api.Controllers;
using OnlineVoting.Models.Dtos.Response.Jwt;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Tests.UnitTests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IUserService> _userService;
        private readonly Mock<IEmailService> _emailService;
        private readonly Mock<IRefreshTokenService> _refreshTokenService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _userService = new Mock<IUserService>();
            _emailService = new Mock<IEmailService>();
            _refreshTokenService = new Mock<IRefreshTokenService>();

            _controller = new AuthController( _userService.Object, _emailService.Object, _refreshTokenService.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task RefreshToken_WithSuccessfulResult_ShouldReturnOk()
        {
            JwtToken jwtToken = new()
            {
                Token = "new-access-token",
                Issuer = "OnlineVoting",
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(30)
            };

            _refreshTokenService.Setup(service => service.RefreshAccessToken())
                .ReturnsAsync(Result<JwtToken>.Success(jwtToken));

            IActionResult result = await _controller.RefreshToken();

            Assert.IsType<OkObjectResult>(result);

            _refreshTokenService.Verify(service => service.RefreshAccessToken(), Times.Once);
        }

        [Fact]
        public async Task RefreshToken_WithUnauthorizedResult_ShouldReturnUnauthorized()
        {
            _refreshTokenService.Setup(service => service.RefreshAccessToken())
                .ReturnsAsync(Result<JwtToken>.Unauthorized("Invalid refresh token."));

            IActionResult result = await _controller.RefreshToken();

            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
            ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

            Assert.Equal(StatusCodes.Status401Unauthorized, objectResult.StatusCode);
            Assert.Equal("Invalid refresh token.", problemDetails.Detail);

            _refreshTokenService.Verify(service => service.RefreshAccessToken(), Times.Once);
        }

        [Fact]
        public async Task Logout_WithSuccessfulResult_ShouldReturnOk()
        {
            _refreshTokenService.Setup(service => service.RevokeCurrentRefreshToken())
                .ReturnsAsync(Result<string>.Success("Refresh token revoked successfully."));

            IActionResult result = await _controller.Logout();

            Assert.IsType<OkObjectResult>(result);

            _refreshTokenService.Verify(service => service.RevokeCurrentRefreshToken(), Times.Once);
        }

        [Fact]
        public async Task Logout_WithUnauthorizedResult_ShouldReturnUnauthorized()
        {
            _refreshTokenService.Setup(service => service.RevokeCurrentRefreshToken())
                .ReturnsAsync(Result<string>.Unauthorized("Invalid refresh token."));

            IActionResult result = await _controller.Logout();

            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
            ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

            Assert.Equal(StatusCodes.Status401Unauthorized, objectResult.StatusCode);
            Assert.Equal("Invalid refresh token.", problemDetails.Detail);

            _refreshTokenService.Verify(service => service.RevokeCurrentRefreshToken(), Times.Once);
        }

        [Fact]
        public async Task LogoutAll_WithSuccessfulResult_ShouldReturnOk()
        {
            _refreshTokenService.Setup(service => service.RevokeAllCurrentUserTokens())
                .ReturnsAsync(Result<string>.Success("All refresh tokens revoked successfully."));

            IActionResult result = await _controller.LogoutAll();

            Assert.IsType<OkObjectResult>(result);

            _refreshTokenService.Verify(service => service.RevokeAllCurrentUserTokens(), Times.Once);
        }

        [Fact]
        public async Task LogoutAll_WithUnauthorizedResult_ShouldReturnUnauthorized()
        {
            _refreshTokenService.Setup(service => service.RevokeAllCurrentUserTokens())
                .ReturnsAsync(Result<string>.Unauthorized("Invalid refresh token."));

            IActionResult result = await _controller.LogoutAll();

            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
            ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

            Assert.Equal(StatusCodes.Status401Unauthorized, objectResult.StatusCode);
            Assert.Equal("Invalid refresh token.", problemDetails.Detail);

            _refreshTokenService.Verify(service => service.RevokeAllCurrentUserTokens(), Times.Once);
        }
    }
}