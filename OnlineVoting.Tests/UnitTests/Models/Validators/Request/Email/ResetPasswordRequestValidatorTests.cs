using FluentValidation.TestHelper;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Validators.Request.Email;
using OnlineVoting.Tests.TestData.Factories.Email;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Request.Email
{
    public class ResetPasswordRequestValidatorTests
    {
        private readonly ResetPasswordRequestValidator _validator = new();

        [Fact]
        public async Task Validate_ValidRequest_ShouldNotHaveValidationErrors()
        {
            ResetPasswordRequest request = ResetPasswordRequestFactory.CreateValid();

            TestValidationResult<ResetPasswordRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Validate_EmptyEmail_ShouldHaveEmailValidationError(string email)
        {
            ResetPasswordRequest request = ResetPasswordRequestFactory.CreateValid();
            request.Email = email;

            TestValidationResult<ResetPasswordRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.Email)
                .WithErrorMessage("Email cannot be empty.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Validate_EmptyResetPasswordToken_ShouldHaveTokenValidationError(string token)
        {
            ResetPasswordRequest request = ResetPasswordRequestFactory.CreateValid();
            request.ResetPasswordToken = token;

            TestValidationResult<ResetPasswordRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.ResetPasswordToken)
                .WithErrorMessage("Reset password token cannot be empty.");
        }

        [Fact]
        public async Task Validate_InvalidNewPassword_ShouldHaveNewPasswordValidationError()
        {
            ResetPasswordRequest request = ResetPasswordRequestFactory.CreateValid();
            request.NewPassword = "password";

            TestValidationResult<ResetPasswordRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.NewPassword);
        }
    }
}