using FluentValidation;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Shared;

namespace OnlineVoting.Models.Validators.Request
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(request => request.Email)
                .SetValidator(new EmailValidator());

            RuleFor(request => request.Password)
                .NotEmpty()
                .WithMessage("Password cannot be empty.");
        }
    }
}