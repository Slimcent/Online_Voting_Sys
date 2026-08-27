using FluentValidation.TestHelper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Request;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Request
{
    public class CreateStudentRequestValidatorTests
    {
        private readonly CreateStudentRequestValidator _validator = new();

        [Fact]
        public async Task Validate_ValidRequest_ShouldNotHaveValidationErrors()
        {
            CreateStudentRequest request = CreateStudentRequestFactory.CreateValid();

            TestValidationResult<CreateStudentRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Validate_InvalidDepartmentId_ShouldHaveDepartmentValidationError(int departmentId)
        {
            CreateStudentRequest request = CreateStudentRequestFactory.CreateValid();
            request.DepartmentId = departmentId;

            TestValidationResult<CreateStudentRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.DepartmentId).WithErrorMessage("Department is required.");
        }
    }
}