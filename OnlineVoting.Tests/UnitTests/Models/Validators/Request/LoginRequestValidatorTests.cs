using FluentValidation.TestHelper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Request;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Request
{
    public class LoginRequestValidatorTests
    {
        private readonly LoginRequestValidator _validator = new();

        [Fact]
        public async Task Validate_ValidRequest_ShouldNotHaveValidationErrors()
        {
            LoginRequest request = LoginRequestFactory.CreateValid();

            TestValidationResult<LoginRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyPassword_ShouldHavePasswordValidationError()
        {
            LoginRequest request = LoginRequestFactory.CreateValid();
            request.Password = string.Empty;

            TestValidationResult<LoginRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.Password).WithErrorMessage("Password cannot be empty.");
        }

        [Fact]
        public async Task Validate_InvalidEmail_ShouldHaveEmailValidationError()
        {
            LoginRequest request = LoginRequestFactory.CreateValid();
            request.Email = "invalid-email";

            TestValidationResult<LoginRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.Email);
        }
    }
}