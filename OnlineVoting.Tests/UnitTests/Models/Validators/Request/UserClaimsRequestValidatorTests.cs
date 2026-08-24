using FluentValidation.TestHelper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Request;
using OnlineVoting.Tests.TestData.Constants;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Request
{
    public class UserClaimsRequestValidatorTests
    {
        private readonly UserClaimsRequestValidator _validator = new();

        [Fact]
        public async Task Validate_ValidRequest_ShouldNotHaveValidationErrors()
        {
            UserClaimsRequest request = UserClaimsRequestFactory.CreateValid();

            TestValidationResult<UserClaimsRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_InvalidEmail_ShouldHaveEmailValidationError()
        {
            UserClaimsRequest request = UserClaimsRequestFactory.CreateValid();
            request.Email = TestValues.InvalidEmail;

            TestValidationResult<UserClaimsRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.Email);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Validate_EmptyClaimType_ShouldHaveClaimTypeValidationError(string claimType)
        {
            UserClaimsRequest request = UserClaimsRequestFactory.CreateValid();
            request.ClaimType = claimType;

            TestValidationResult<UserClaimsRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.ClaimType).WithErrorMessage("Claim type cannot be empty.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Validate_EmptyClaimValue_ShouldHaveClaimValueValidationError(string claimValue)
        {
            UserClaimsRequest request = UserClaimsRequestFactory.CreateValid();
            request.ClaimValue = claimValue;

            TestValidationResult<UserClaimsRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.ClaimValue).WithErrorMessage("Claim value cannot be empty.");
        }
    }
}