using FluentValidation.TestHelper;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Validators.Request.Email;
using OnlineVoting.Tests.TestData.Constants;
using OnlineVoting.Tests.TestData.Factories.Email;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Request.Email
{
    public class ChangeEmailRequestValidatorTests
    {
        private readonly ChangeEmailRequestValidator _validator = new();

        [Fact]
        public async Task Validate_ValidRequest_ShouldNotHaveValidationErrors()
        {
            ChangeEmailRequest request = ChangeEmailRequestFactory.CreateValid();

            TestValidationResult<ChangeEmailRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_InvalidEmail_ShouldHaveEmailValidationError()
        {
            ChangeEmailRequest request = ChangeEmailRequestFactory.CreateValid();
            request.Email = TestValues.InvalidEmail;

            TestValidationResult<ChangeEmailRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.Email);
        }

        [Fact]
        public async Task Validate_InvalidNewEmail_ShouldHaveNewEmailValidationError()
        {
            ChangeEmailRequest request = ChangeEmailRequestFactory.CreateValid();
            request.NewEmail = TestValues.InvalidEmail;

            TestValidationResult<ChangeEmailRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.NewEmail);
        }

        [Fact]
        public async Task Validate_InvalidRecoveryEmail_ShouldHaveRecoveryEmailValidationError()
        {
            ChangeEmailRequest request = ChangeEmailRequestFactory.CreateValid();
            request.RecoveryEmail = TestValues.InvalidEmail;

            TestValidationResult<ChangeEmailRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.RecoveryEmail);
        }
    }
}