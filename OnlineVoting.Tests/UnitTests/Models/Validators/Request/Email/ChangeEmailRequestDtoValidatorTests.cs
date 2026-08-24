using FluentValidation.TestHelper;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Validators.Request.Email;
using OnlineVoting.Tests.TestData.Factories.Email;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Request.Email
{
    public class ChangeEmailRequestDtoValidatorTests
    {
        private readonly ChangeEmailRequestDtoValidator _validator = new();

        [Fact]
        public async Task Validate_ValidRequest_ShouldNotHaveValidationErrors()
        {
            ChangeEmailRequestDto request = ChangeEmailRequestDtoFactory.CreateValid();

            TestValidationResult<ChangeEmailRequestDto> result = await _validator.TestValidateAsync(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Validate_EmptyNewEmail_ShouldHaveNewEmailValidationError(string newEmail)
        {
            ChangeEmailRequestDto request = ChangeEmailRequestDtoFactory.CreateValid();
            request.NewEmail = newEmail;

            TestValidationResult<ChangeEmailRequestDto> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.NewEmail).WithErrorMessage("New email cannot be empty.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Validate_EmptyToken_ShouldHaveTokenValidationError(string token)
        {
            ChangeEmailRequestDto request = ChangeEmailRequestDtoFactory.CreateValid();
            request.Token = token;

            TestValidationResult<ChangeEmailRequestDto> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.Token).WithErrorMessage("Token cannot be empty.");
        }
    }
}