using FluentValidation;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Shared;

namespace OnlineVoting.Models.Validators.Request
{
    public class UserClaimsRequestValidator : AbstractValidator<UserClaimsRequest>
    {
        public UserClaimsRequestValidator()
        {
            RuleFor(request => request.Email)
                .SetValidator(new EmailValidator());

            RuleFor(request => request.ClaimType)
                .NotEmpty()
                .WithMessage("Claim type cannot be empty.");

            RuleFor(request => request.ClaimValue)
                .NotEmpty()
                .WithMessage("Claim value cannot be empty.");
        }
    }
}