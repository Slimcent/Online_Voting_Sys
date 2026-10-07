using FluentValidation;
using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Models.Validators.Request
{
    public class RegisterVoterRequestValidator : AbstractValidator<RegisterVoterRequest>
    {
        public RegisterVoterRequestValidator()
        {
            RuleFor(x => x.RegNumber)
                .NotEmpty()
                .WithMessage("Registration number is required.");

            RuleFor(x => x.ElectionId)
                .NotEmpty()
                .WithMessage("Election id is required.");
        }
    }
}