using FluentValidation.TestHelper;
using OnlineVoting.Models.Validators.Shared;
using OnlineVoting.Tests.TestData.Constants;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Shared
{
    public class PhoneNumberValidatorTests
    {
        private readonly PhoneNumberValidator _validator = new();

        [Fact]
        public async Task Validate_ValidPhoneNumber_ShouldNotHaveValidationErrors()
        {
            TestValidationResult<string> result = await _validator.TestValidateAsync(TestValues.ValidPhoneNumber);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyPhoneNumber_ShouldHaveRequiredValidationError()
        {
            TestValidationResult<string> result = await _validator.TestValidateAsync(string.Empty);

            result.ShouldHaveValidationErrorFor(phoneNumber => phoneNumber).WithErrorMessage("Phone number cannot be empty.");
        }

        [Theory]
        [InlineData(TestValues.PhoneNumberWithoutLeadingZero)]
        [InlineData(TestValues.TooShortPhoneNumber)]
        [InlineData(TestValues.TooLongPhoneNumber)]
        [InlineData(TestValues.PhoneNumberWithLetters)]
        public async Task Validate_InvalidPhoneNumber_ShouldHaveFormatValidationError(string phoneNumber)
        {
            TestValidationResult<string> result = await _validator.TestValidateAsync(phoneNumber);

            result.ShouldHaveValidationErrorFor(value => value).WithErrorMessage("Phone number is invalid.");
        }
    }
}