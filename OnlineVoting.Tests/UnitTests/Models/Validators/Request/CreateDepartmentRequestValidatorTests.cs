using FluentValidation.TestHelper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Request;
using OnlineVoting.Tests.TestData.Constants;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Request
{
    public class CreateDepartmentRequestValidatorTests
    {
        private readonly CreateDepartmentRequestValidator _validator = new();

        [Fact]
        public async Task Validate_ValidSingleDepartment_ShouldNotHaveValidationErrors()
        {
            CreateDepartmentRequest request = CreateDepartmentRequestFactory.CreateValidSingle();

            TestValidationResult<CreateDepartmentRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ValidMultipleDepartments_ShouldNotHaveValidationErrors()
        {
            CreateDepartmentRequest request = CreateDepartmentRequestFactory.CreateValidMultiple();

            TestValidationResult<CreateDepartmentRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Validate_InvalidFacultyId_ShouldHaveFacultyValidationError(int facultyId)
        {
            CreateDepartmentRequest request = CreateDepartmentRequestFactory.CreateValidSingle();
            request.FacultyId = facultyId;

            TestValidationResult<CreateDepartmentRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.FacultyId).WithErrorMessage("Faculty is required.");
        }

        [Fact]
        public async Task Validate_NoDepartmentInput_ShouldHaveDepartmentInputValidationError()
        {
            CreateDepartmentRequest request = new()
            {
                FacultyId = 1
            };

            TestValidationResult<CreateDepartmentRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value).WithErrorMessage("Provide either a department name or a list of department names.");
        }

        [Fact]
        public async Task Validate_NameAndNamesProvided_ShouldHaveExclusiveInputValidationError()
        {
            CreateDepartmentRequest request = CreateDepartmentRequestFactory.CreateValidMultiple();
            request.Name = TestValues.ValidName;

            TestValidationResult<CreateDepartmentRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value).WithErrorMessage("Provide either Name or Names, but not both.");
        }

        [Fact]
        public async Task Validate_InvalidSingleDepartmentName_ShouldHaveNameValidationError()
        {
            CreateDepartmentRequest request = CreateDepartmentRequestFactory.CreateValidSingle();
            request.Name = TestValues.TooShortName;

            TestValidationResult<CreateDepartmentRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.Name);
        }

        [Fact]
        public async Task Validate_InvalidDepartmentNameInList_ShouldHaveNamesValidationError()
        {
            CreateDepartmentRequest request = CreateDepartmentRequestFactory.CreateValidMultiple();
            request.Names![1] = TestValues.TooShortName;

            TestValidationResult<CreateDepartmentRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor("Names[1]");
        }
    }
}