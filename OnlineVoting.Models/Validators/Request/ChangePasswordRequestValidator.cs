using FluentValidation;
using Microsoft.AspNetCore.Identity;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Shared;

namespace OnlineVoting.Models.Validators.Request
{
    public class ChangePasswordRequestValidator
        : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(request => request.CurrentPassword)
                .NotEmpty()
                .WithMessage("Current password cannot be empty.");

            RuleFor(request => request.NewPassword)
                .SetValidator(new PasswordValidator());

            RuleFor(request => request.NewPassword)
                .NotEqual(request => request.CurrentPassword)
                .WithMessage(
                    "New password must be different from the current password.");
        }
    }
}