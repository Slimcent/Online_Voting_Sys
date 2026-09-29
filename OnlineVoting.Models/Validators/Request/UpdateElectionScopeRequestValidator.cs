using FluentValidation;
using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Models.Validators
{
    public class UpdateElectionScopeRequestValidator : AbstractValidator<UpdateElectionScopeRequest>
    {
        public UpdateElectionScopeRequestValidator()
        {
            RuleFor(request => request.Id)
                .GreaterThan(0);

            RuleFor(request => request.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(request => request.Description)
                .MaximumLength(250);
        }
    }
}