using FluentValidation.TestHelper;
using OnlineVoting.Models.Validators.Shared;
using OnlineVoting.Tests.TestData.Constants;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Shared
{
    public class RegNumberValidatorTests
    {
        private readonly RegNumberValidator _validator = new();

        [Fact]
        public async Task Validate_ValidRegistrationNumber_ShouldNotHaveValidationErrors()
        {
            TestValidationResult<string> result = await _validator.TestValidateAsync(TestValues.ValidRegistrationNumber);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyRegistrationNumber_ShouldHaveRequiredValidationError()
        {
            TestValidationResult<string> result = await _validator.TestValidateAsync(string.Empty);

            result.ShouldHaveValidationErrorFor(regNumber => regNumber).WithErrorMessage("Registration number cannot be empty.");
        }
    }
}