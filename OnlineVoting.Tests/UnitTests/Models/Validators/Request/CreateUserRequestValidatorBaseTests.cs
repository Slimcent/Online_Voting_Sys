using FluentValidation.TestHelper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Shared;
using OnlineVoting.Tests.TestData.Constants;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Request
{
    public class CreateUserRequestValidatorBaseTests
    {
        private readonly TestCreateUserRequestValidator _validator = new();

        [Fact]
        public async Task Validate_ValidRequest_ShouldNotHaveValidationErrors()
        {
            CreateUserRequest request = CreateUserRequestFactory.CreateValid();

            TestValidationResult<CreateUserRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_InvalidFirstName_ShouldHaveFirstNameValidationError()
        {
            CreateUserRequest request = CreateUserRequestFactory.CreateValid();
            request.FirstName = TestValues.TooShortName;

            TestValidationResult<CreateUserRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.FirstName);
        }

        [Fact]
        public async Task Validate_InvalidLastName_ShouldHaveLastNameValidationError()
        {
            CreateUserRequest request = CreateUserRequestFactory.CreateValid();
            request.LastName = TestValues.TooShortName;

            TestValidationResult<CreateUserRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.LastName);
        }

        [Fact]
        public async Task Validate_InvalidEmail_ShouldHaveEmailValidationError()
        {
            CreateUserRequest request = CreateUserRequestFactory.CreateValid();
            request.Email = TestValues.InvalidEmail;

            TestValidationResult<CreateUserRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.Email);
        }

        [Fact]
        public async Task Validate_InvalidPhoneNumber_ShouldHavePhoneNumberValidationError()
        {
            CreateUserRequest request = CreateUserRequestFactory.CreateValid();
            request.PhoneNumber = TestValues.TooShortPhoneNumber;

            TestValidationResult<CreateUserRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.PhoneNumber);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Validate_InvalidGenderId_ShouldHaveGenderValidationError(int genderId)
        {
            CreateUserRequest request = CreateUserRequestFactory.CreateValid();
            request.GenderId = genderId;

            TestValidationResult<CreateUserRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.GenderId).WithErrorMessage("Gender is required.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Validate_InvalidUserType_ShouldHaveUserTypeValidationError(int userType)
        {
            CreateUserRequest request = CreateUserRequestFactory.CreateValid();
            request.UserType = userType;

            TestValidationResult<CreateUserRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.UserType).WithErrorMessage("User type is required.");
        }

        [Fact]
        public async Task Validate_EmptyRole_ShouldHaveRoleValidationError()
        {
            CreateUserRequest request = CreateUserRequestFactory.CreateValid();
            request.Role = string.Empty;

            TestValidationResult<CreateUserRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.Role).WithErrorMessage("Role cannot be empty.");
        }

        private sealed class TestCreateUserRequestValidator : CreateUserRequestValidatorBase<CreateUserRequest>
        {
        }
    }
}