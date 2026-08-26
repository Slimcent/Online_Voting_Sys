using FluentValidation.TestHelper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Request;
using OnlineVoting.Tests.TestData.Constants;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Request
{
    public class CreateWithNameRequestValidatorTests
    {
        private readonly CreateWithNameRequestValidator _validator = new();

        [Fact]
        public async Task Validate_ValidRequest_ShouldNotHaveValidationErrors()
        {
            CreateWithNameRequest request = CreateWithNameRequestFactory.CreateValid();

            TestValidationResult<CreateWithNameRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(TestValues.TooShortName)]
        public async Task Validate_InvalidName_ShouldHaveNameValidationError(string name)
        {
            CreateWithNameRequest request = CreateWithNameRequestFactory.CreateValid();
            request.Name = name;

            TestValidationResult<CreateWithNameRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.Name);
        }
    }
}