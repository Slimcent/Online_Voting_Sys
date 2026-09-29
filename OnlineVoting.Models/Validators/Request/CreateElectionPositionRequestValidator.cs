using FluentValidation;
using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Models.Validators.Request
{
    public class CreateElectionPositionRequestValidator : AbstractValidator<CreateElectionPositionRequest>
    {
        public CreateElectionPositionRequestValidator()
        {
            RuleFor(x => x.Id)
                .Must(BeValidGuid)
                .When(x => !string.IsNullOrWhiteSpace(x.Id))
                .WithMessage("Election position id must be a valid GUID.");

            RuleFor(x => x.ElectionId)
                .NotEmpty()
                .Must(BeValidGuid)
                .WithMessage("Election id must be a valid GUID.");

            RuleFor(x => x.PositionId)
                .NotEmpty()
                .Must(BeValidGuid)
                .WithMessage("Position id must be a valid GUID.");

            RuleFor(x => x.ApplicationFee)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.Currency)
                .NotEmpty()
                .MaximumLength(3);
        }

        private static bool BeValidGuid(string? value)
        {
            return Guid.TryParse(value, out Guid _);
        }
    }
}