using FluentValidation;
using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Models.Validators.Request
{
    public class CreateElectionTypeRequestValidator : AbstractValidator<CreateElectionTypeRequest>
    {
        public CreateElectionTypeRequestValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(request => request.Description)
                .MaximumLength(250);

            RuleFor(request => request.ElectionScopeId)
                .GreaterThan(0)
                .WithMessage("Election scope is required.");
        }
    }
}
