using FluentValidation.TestHelper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Request;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Request
{
    public class CreateRoleRequestValidatorTests
    {
        private readonly CreateRoleRequestValidator _validator = new();

        [Fact]
        public async Task Validate_ValidRequest_ShouldNotHaveValidationErrors()
        {
            CreateRoleRequest request = CreateRoleRequestFactory.CreateValid();

            TestValidationResult<CreateRoleRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Validate_EmptyName_ShouldHaveNameValidationError(string name)
        {
            CreateRoleRequest request = CreateRoleRequestFactory.CreateValid();
            request.Name = name;

            TestValidationResult<CreateRoleRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.Name).WithErrorMessage("Name cannot be empty.");
        }

        [Theory]
        [InlineData("A")]
        [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
        public async Task Validate_InvalidNameLength_ShouldHaveNameValidationError(string name)
        {
            CreateRoleRequest request = CreateRoleRequestFactory.CreateValid();
            request.Name = name;

            TestValidationResult<CreateRoleRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.Name).WithErrorMessage("Name must be between 2 and 100 characters.");
        }
    }
}