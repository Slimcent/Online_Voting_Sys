using FluentValidation.TestHelper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Request;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Request
{
    public class RegisterVoterRequestValidatorTests
    {
        private readonly RegisterVoterRequestValidator _validator = new();

        [Fact]
        public void Validate_ReturnsValid_WhenRequestIsValid()
        {
            RegisterVoterRequest request = RegisterVoterRequestFactory.Create("REG001", Guid.NewGuid().ToString());

            TestValidationResult<RegisterVoterRequest> result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_ReturnsError_WhenRegistrationNumberIsEmpty()
        {
            RegisterVoterRequest request = RegisterVoterRequestFactory.Create(string.Empty, Guid.NewGuid().ToString());

            TestValidationResult<RegisterVoterRequest> result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.RegNumber);
        }

        [Fact]
        public void Validate_ReturnsError_WhenElectionIdIsEmpty()
        {
            RegisterVoterRequest request = RegisterVoterRequestFactory.Create("REG001", string.Empty);

            TestValidationResult<RegisterVoterRequest> result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.ElectionId);
        }
    }
}