using FluentValidation;
using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Models.Validators.Request
{
    public class UpdateAddressRequestValidator
        : AbstractValidator<UpdateAddressRequest>
    {
        public UpdateAddressRequestValidator()
        {
            RuleFor(request => request.PlotNo)
                .GreaterThan(0)
                .WithMessage("Plot number must be greater than zero.");

            RuleFor(request => request.StreetName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Street name cannot be empty.")
                .Length(2, 20)
                .WithMessage(
                    "Street name must be between 2 and 20 characters.");

            RuleFor(request => request.City)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("City name cannot be empty.")
                .Length(2, 20)
                .WithMessage(
                    "City name must be between 2 and 20 characters.");

            RuleFor(request => request.State)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("State name cannot be empty.")
                .Length(2, 20)
                .WithMessage(
                    "State name must be between 2 and 20 characters.");

            RuleFor(request => request.Nationality)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Nationality cannot be empty.")
                .Length(4, 20)
                .WithMessage(
                    "Nationality must be between 4 and 20 characters.");
        }
    }
}