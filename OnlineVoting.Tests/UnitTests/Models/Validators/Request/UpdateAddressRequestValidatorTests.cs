using FluentValidation.TestHelper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Request;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.UnitTests.Models.Validators.Request
{
    public class UpdateAddressRequestValidatorTests
    {
        private readonly UpdateAddressRequestValidator _validator = new();

        [Fact]
        public async Task Validate_ValidRequest_ShouldNotHaveValidationErrors()
        {
            UpdateAddressRequest request = UpdateAddressRequestFactory.CreateValid();

            TestValidationResult<UpdateAddressRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Validate_InvalidPlotNumber_ShouldHavePlotNumberValidationError(int plotNo)
        {
            UpdateAddressRequest request = UpdateAddressRequestFactory.CreateValid();
            request.PlotNo = plotNo;

            TestValidationResult<UpdateAddressRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.PlotNo).WithErrorMessage("Plot number must be greater than zero.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("A")]
        [InlineData("This street name is definitely too long")]
        public async Task Validate_InvalidStreetName_ShouldHaveStreetNameValidationError(string streetName)
        {
            UpdateAddressRequest request = UpdateAddressRequestFactory.CreateValid();
            request.StreetName = streetName;

            TestValidationResult<UpdateAddressRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.StreetName);
        }

        [Theory]
        [InlineData("")]
        [InlineData("A")]
        [InlineData("This city name is definitely too long")]
        public async Task Validate_InvalidCity_ShouldHaveCityValidationError(string city)
        {
            UpdateAddressRequest request = UpdateAddressRequestFactory.CreateValid();
            request.City = city;

            TestValidationResult<UpdateAddressRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.City);
        }

        [Theory]
        [InlineData("")]
        [InlineData("A")]
        [InlineData("This state name is definitely too long")]
        public async Task Validate_InvalidState_ShouldHaveStateValidationError(string state)
        {
            UpdateAddressRequest request = UpdateAddressRequestFactory.CreateValid();
            request.State = state;

            TestValidationResult<UpdateAddressRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.State);
        }

        [Theory]
        [InlineData("")]
        [InlineData("USA")]
        [InlineData("This nationality name is too long")]
        public async Task Validate_InvalidNationality_ShouldHaveNationalityValidationError(string nationality)
        {
            UpdateAddressRequest request = UpdateAddressRequestFactory.CreateValid();
            request.Nationality = nationality;

            TestValidationResult<UpdateAddressRequest> result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(value => value.Nationality);
        }
    }
}