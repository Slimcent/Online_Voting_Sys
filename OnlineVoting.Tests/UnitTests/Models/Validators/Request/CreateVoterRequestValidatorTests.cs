using FluentValidation.TestHelper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Request;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Request
{
    public class CreateVoterRequestValidatorTests
    {
        private readonly CreateVoterRequestValidator _validator = new();

        [Fact]
        public async Task Validate_ValidRequest_ShouldNotHaveValidationErrors()
        {
            CreateVoterRequest request = CreateVoterRequestFactory.CreateValid();

            TestValidationResult<CreateVoterRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Validate_EmptyRegNumber_ShouldHaveRegNumberValidationError(string regNumber)
        {
            CreateVoterRequest request = CreateVoterRequestFactory.CreateValid();
            request.RegNumber = regNumber;

            TestValidationResult<CreateVoterRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.RegNumber)
                .WithErrorMessage("Registration number cannot be empty.");
        }
    }
}