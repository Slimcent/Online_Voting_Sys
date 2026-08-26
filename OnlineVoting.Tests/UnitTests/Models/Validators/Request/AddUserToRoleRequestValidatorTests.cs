using FluentValidation.TestHelper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Request;
using OnlineVoting.Tests.TestData.Constants;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Request
{
    public class AddUserToRoleRequestValidatorTests
    {
        private readonly AddUserToRoleRequestValidator _validator = new();

        [Fact]
        public async Task Validate_ValidRequest_ShouldNotHaveValidationErrors()
        {
            AddUserToRoleRequest request = AddUserToRoleRequestFactory.CreateValid();

            TestValidationResult<AddUserToRoleRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_InvalidName_ShouldHaveNameValidationError()
        {
            AddUserToRoleRequest request = AddUserToRoleRequestFactory.CreateValid();
            request.Name = string.Empty;

            TestValidationResult<AddUserToRoleRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.Name);
        }

        [Fact]
        public async Task Validate_InvalidEmail_ShouldHaveEmailValidationError()
        {
            AddUserToRoleRequest request = AddUserToRoleRequestFactory.CreateValid();
            request.Email = TestValues.InvalidEmail;

            TestValidationResult<AddUserToRoleRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.Email);
        }
    }
}