using FluentValidation.TestHelper;
using OnlineVoting.Models.Validators.Shared;
using OnlineVoting.Tests.TestData.Constants;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Shared
{
    public class PasswordValidatorTests
    {
        private readonly PasswordValidator _validator = new();

        [Fact]
        public async Task Validate_ValidPassword_ShouldNotHaveValidationErrors()
        {
            TestValidationResult<string> result = await _validator.TestValidateAsync(TestValues.ValidPassword);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyPassword_ShouldHaveRequiredValidationError()
        {
            TestValidationResult<string> result = await _validator.TestValidateAsync(string.Empty);

            result.ShouldHaveValidationErrorFor(password => password).WithErrorMessage("Password cannot be empty.");
        }

        [Theory]
        [InlineData(TestValues.ShortPassword, "Password must be at least 8 characters long.")]
        [InlineData(TestValues.PasswordWithoutUppercase, "Password must contain at least one uppercase letter.")]
        [InlineData(TestValues.PasswordWithoutLowercase, "Password must contain at least one lowercase letter.")]
        [InlineData(TestValues.PasswordWithoutNumber, "Password must contain at least one number.")]
        [InlineData(TestValues.PasswordWithoutSpecialCharacter, "Password must contain at least one special character.")]
        public async Task Validate_InvalidPassword_ShouldHaveExpectedValidationError(string password, string expectedErrorMessage)
        {
            TestValidationResult<string> result = await _validator.TestValidateAsync(password);

            result.ShouldHaveValidationErrorFor(value => value).WithErrorMessage(expectedErrorMessage);
        }
    }
}