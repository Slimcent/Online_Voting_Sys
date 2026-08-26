using FluentValidation.TestHelper;
using OnlineVoting.Models.Validators.Shared;
using OnlineVoting.Tests.TestData.Constants;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Shared
{
    public class NameValidatorTests
    {
        private readonly NameValidator _validator = new();

        [Fact]
        public async Task Validate_ValidName_ShouldNotHaveValidationErrors()
        {
            TestValidationResult<string> result = await _validator.TestValidateAsync(TestValues.ValidName);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyName_ShouldHaveRequiredValidationError()
        {
            TestValidationResult<string> result = await _validator.TestValidateAsync(string.Empty);

            result.ShouldHaveValidationErrorFor(name => name).WithErrorMessage("Name cannot be empty.");
        }

        [Theory]
        [InlineData("J")]
        public async Task Validate_NameOutsideAllowedLength_ShouldHaveLengthValidationError(string name)
        {
            TestValidationResult<string> result = await _validator.TestValidateAsync(name);

            result.ShouldHaveValidationErrorFor(value => value).WithErrorMessage("Name must be between 2 and 100 characters.");
        }

        [Fact]
        public async Task Validate_NameLongerThanMaximum_ShouldHaveLengthValidationError()
        {
            TestValidationResult<string> result = await _validator.TestValidateAsync(TestValues.TooLongName);

            result.ShouldHaveValidationErrorFor(name => name).WithErrorMessage("Name must be between 2 and 100 characters.");
        }
    }
}
