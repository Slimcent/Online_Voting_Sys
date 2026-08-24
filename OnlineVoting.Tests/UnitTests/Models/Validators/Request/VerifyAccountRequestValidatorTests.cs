using FluentValidation.TestHelper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Request;
using OnlineVoting.Tests.TestData.Constants;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Request
{
    public class VerifyAccountRequestValidatorTests
    {
        private readonly VerifyAccountRequestValidator _validator = new();

        [Fact]
        public async Task Validate_ValidRequest_ShouldNotHaveValidationErrors()
        {
            VerifyAccountRequest request = VerifyAccountRequestFactory.CreateValid();

            TestValidationResult<VerifyAccountRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_InvalidEmail_ShouldHaveEmailValidationError()
        {
            VerifyAccountRequest request = VerifyAccountRequestFactory.CreateValid();
            request.Email = TestValues.InvalidEmail;

            TestValidationResult<VerifyAccountRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.Email);
        }

        [Fact]
        public async Task Validate_EmptyEmailConfirmationToken_ShouldHaveValidationError()
        {
            VerifyAccountRequest request = VerifyAccountRequestFactory.CreateValid();
            request.EmailConfirmationToken = string.Empty;

            TestValidationResult<VerifyAccountRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.EmailConfirmationToken)
                .WithErrorMessage("Email confirmation token cannot be empty.");
        }

        [Fact]
        public async Task Validate_EmptyResetPasswordToken_ShouldHaveValidationError()
        {
            VerifyAccountRequest request = VerifyAccountRequestFactory.CreateValid();
            request.ResetPasswordToken = string.Empty;

            TestValidationResult<VerifyAccountRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.ResetPasswordToken).WithErrorMessage("Reset password token cannot be empty.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(TestValues.ShortPassword)]
        [InlineData(TestValues.PasswordWithoutUppercase)]
        [InlineData(TestValues.PasswordWithoutLowercase)]
        [InlineData(TestValues.PasswordWithoutNumber)]
        [InlineData(TestValues.PasswordWithoutSpecialCharacter)]
        public async Task Validate_InvalidNewPassword_ShouldHavePasswordValidationError(string newPassword)
        {
            VerifyAccountRequest request = VerifyAccountRequestFactory.CreateValid();
            request.NewPassword = newPassword;

            TestValidationResult<VerifyAccountRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.NewPassword);
        }
    }
}