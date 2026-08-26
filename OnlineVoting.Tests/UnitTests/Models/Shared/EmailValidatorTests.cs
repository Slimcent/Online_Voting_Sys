using FluentValidation.TestHelper;
using OnlineVoting.Models.Validators.Shared;
using OnlineVoting.Tests.TestData.Constants;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Shared
{
    public class EmailValidatorTests
    {
        private readonly EmailValidator _validator = new();

        [Fact]
        public async Task Validate_ValidEmail_ShouldNotHaveValidationError()
        {
            TestValidationResult<string> result = await _validator.TestValidateAsync(TestValues.ValidEmail);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyEmail_ShouldHaveRequiredValidationError()
        {
            TestValidationResult<string> result = await _validator.TestValidateAsync(string.Empty);

            result.ShouldHaveValidationErrorFor(email => email).WithErrorMessage("Email cannot be empty.");
        }

        [Fact]
        public async Task Validate_InvalidEmail_ShouldHaveFormatValidationError()
        {
            TestValidationResult<string> result = await _validator.TestValidateAsync(TestValues.InvalidEmail);

            result.ShouldHaveValidationErrorFor(email => email).WithErrorMessage("Email format is invalid.");
        }
    }
}