using FluentValidation;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Validators.Shared;

namespace OnlineVoting.Models.Validators.Request.Email
{
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(request => request.Email)
                .NotEmpty()
                .WithMessage("Email cannot be empty.");

            RuleFor(request => request.ResetPasswordToken)
                .NotEmpty()
                .WithMessage("Reset password token cannot be empty.");

            RuleFor(request => request.NewPassword!)
                .SetValidator(new PasswordValidator());
        }
    }
}