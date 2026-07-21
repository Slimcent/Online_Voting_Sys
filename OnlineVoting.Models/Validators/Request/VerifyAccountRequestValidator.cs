using FluentValidation;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Shared;

namespace OnlineVoting.Models.Validators.Request
{
    public class VerifyAccountRequestValidator
        : AbstractValidator<VerifyAccountRequest>
    {
        public VerifyAccountRequestValidator()
        {
            RuleFor(request => request.Email)
                .SetValidator(new EmailValidator());

            RuleFor(request => request.EmailConfirmationToken)
                .NotEmpty()
                .WithMessage("Email confirmation token cannot be empty.");

            RuleFor(request => request.ResetPasswordToken)
                .NotEmpty()
                .WithMessage("Reset password token cannot be empty.");

            RuleFor(request => request.NewPassword)
                .SetValidator(new PasswordValidator());
        }
    }
}