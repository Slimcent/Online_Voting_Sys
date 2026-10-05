using FluentValidation;
using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Models.Validators.Request
{
    public class CreateElectionPositionsRequestValidator : AbstractValidator<CreateElectionPositionsRequest>
    {
        public CreateElectionPositionsRequestValidator()
        {
            RuleFor(x => x.ElectionId)
                .NotEmpty()
                .Must(BeValidGuid)
                .WithMessage("Election id must be a valid GUID.");

            RuleFor(x => x.ElectionPositions)
                .NotEmpty()
                .WithMessage("At least one election position must be provided.");

            RuleFor(x => x.ElectionPositions)
                .Must(HaveUniquePositions)
                .WithMessage("The same position cannot be added more than once.");

            RuleForEach(x => x.ElectionPositions)
                .SetValidator(new CreateElectionPositionItemRequestValidator());
        }

        private static bool BeValidGuid(string value)
        {
            return Guid.TryParse(value, out Guid _);
        }

        private static bool HaveUniquePositions(List<CreateElectionPositionItemRequest>? electionPositions)
        {
            if (electionPositions is null || electionPositions.Count == 0)
                return true;

            return electionPositions
                .Select(x => x.PositionId.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() == electionPositions.Count;
        }
    }

    public class CreateElectionPositionItemRequestValidator : AbstractValidator<CreateElectionPositionItemRequest>
    {
        public CreateElectionPositionItemRequestValidator()
        {
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

        private static bool BeValidGuid(string value)
        {
            return Guid.TryParse(value, out Guid _);
        }
    }
}