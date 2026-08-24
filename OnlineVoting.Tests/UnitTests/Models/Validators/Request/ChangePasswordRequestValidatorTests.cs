using FluentValidation.TestHelper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Request;
using OnlineVoting.Tests.TestData.Constants;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Request
{
    public class ChangePasswordRequestValidatorTests
    {
        private readonly ChangePasswordRequestValidator _validator = new();

        [Fact]
        public async Task Validate_ValidRequest_ShouldNotHaveValidationErrors()
        {
            ChangePasswordRequest request = ChangePasswordRequestFactory.CreateValid();

            TestValidationResult<ChangePasswordRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyCurrentPassword_ShouldHaveCurrentPasswordValidationError()
        {
            ChangePasswordRequest request = ChangePasswordRequestFactory.CreateValid();
            request.CurrentPassword = string.Empty;

            TestValidationResult<ChangePasswordRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.CurrentPassword).WithErrorMessage("Current password cannot be empty.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(TestValues.ShortPassword)]
        [InlineData(TestValues.PasswordWithoutUppercase)]
        [InlineData(TestValues.PasswordWithoutLowercase)]
        [InlineData(TestValues.PasswordWithoutNumber)]
        [InlineData(TestValues.PasswordWithoutSpecialCharacter)]
        public async Task Validate_InvalidNewPassword_ShouldHaveNewPasswordValidationError(string newPassword)
        {
            ChangePasswordRequest request = ChangePasswordRequestFactory.CreateValid();
            request.NewPassword = newPassword;

            TestValidationResult<ChangePasswordRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.NewPassword);
        }

        [Fact]
        public async Task Validate_SameCurrentAndNewPassword_ShouldHaveNewPasswordValidationError()
        {
            ChangePasswordRequest request = ChangePasswordRequestFactory.CreateValid();
            request.NewPassword = request.CurrentPassword;

            TestValidationResult<ChangePasswordRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.NewPassword).WithErrorMessage("New password must be different from the current password.");
        }
    }
}