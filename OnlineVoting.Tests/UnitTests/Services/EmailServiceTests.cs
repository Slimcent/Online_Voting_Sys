using Moq;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Enums;
using OnlineVoting.Models.Results;
using OnlineVoting.Tests.TestData.Data;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.UnitTests.Services
{
    public class EmailServiceTests
    {
        [Fact]
        public async Task SendResetPasswordEmail_WithEmptyEmail_ShouldReturnValidationError()
        {
            EmailServiceFactory factory = new();

            Result<string> result = await factory.Service.SendResetPasswordEmail(" ");

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Enter an email", result.Error);
            Assert.Equal(0, factory.Service.SendCount);

            factory.UserManager.Verify(userManager => userManager.FindByEmailAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SendResetPasswordEmail_WithMissingUser_ShouldReturnNotFound()
        {
            EmailServiceFactory factory = new();

            factory.UserManager.Setup(userManager => userManager.FindByEmailAsync("user@example.com")).ReturnsAsync((User?)null);

            Result<string> result = await factory.Service.SendResetPasswordEmail(" user@example.com ");

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("A link to reset your password will be sent to you if an account with this email exist", result.Error);
            Assert.Equal(0, factory.Service.SendCount);

            factory.UserManager.Verify(userManager => userManager.GeneratePasswordResetTokenAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task SendResetPasswordEmail_WithExistingUser_ShouldGenerateTokenAndSendEmail()
        {
            EmailServiceFactory factory = new();

            User user = ClaimsTestData.CreateUser();
            user.FirstName = "Vincent";

            factory.UserManager.Setup(userManager => userManager.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
            factory.UserManager.Setup(userManager => userManager.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");

            Result<string> result = await factory.Service.SendResetPasswordEmail(" user@example.com ");

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("A link to reset your password will be sent to you if an account with this email exist", result.Value);
            Assert.Equal(1, factory.Service.SendCount);
            Assert.NotNull(factory.Service.EmailData);

            factory.UserManager.Verify(userManager => userManager.GeneratePasswordResetTokenAsync(user), Times.Once);
        }

        [Fact]
        public async Task SendCreateUserEmail_ShouldGenerateTokensAndSendEmail()
        {
            EmailServiceFactory factory = new();

            User user = ClaimsTestData.CreateUser();
            user.FirstName = "Vincent";

            UserMailDto request = new()
            {
                User = user,
                FirstName = "Vincent"
            };

            factory.UserManager.Setup(userManager => userManager.GenerateEmailConfirmationTokenAsync(user)).ReturnsAsync("email-confirmation-token");
            factory.UserManager.Setup(userManager => userManager.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-password-token");

            await factory.Service.SendCreateUserEmail(request);

            Assert.Equal(1, factory.Service.SendCount);
            Assert.NotNull(factory.Service.EmailData);

            factory.UserManager.Verify(userManager => userManager.GenerateEmailConfirmationTokenAsync(user), Times.Once);
            factory.UserManager.Verify(userManager => userManager.GeneratePasswordResetTokenAsync(user), Times.Once);
        }

        [Fact]
        public async Task SendVoterEmail_ShouldSendEmail()
        {
            EmailServiceFactory factory = new();

            VoterEmailDto request = new()
            {
                FirstName = "Vincent",
                Email = "user@example.com",
                VotingCode = "VOTE-123"
            };

            await factory.Service.SendVoterEmail(request);

            Assert.Equal(1, factory.Service.SendCount);
            Assert.NotNull(factory.Service.EmailData);
        }
    }
}