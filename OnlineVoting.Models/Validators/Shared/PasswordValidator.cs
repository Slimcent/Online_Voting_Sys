using FluentValidation;

namespace OnlineVoting.Models.Validators.Shared
{
    public class PasswordValidator : AbstractValidator<string>
    {
        public PasswordValidator()
        {
            RuleFor(password => password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Password cannot be empty.")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long.")
                .Matches("[A-Z]")
                .WithMessage(
                    "Password must contain at least one uppercase letter.")
                .Matches("[a-z]")
                .WithMessage(
                    "Password must contain at least one lowercase letter.")
                .Matches("[0-9]")
                .WithMessage(
                    "Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]")
                .WithMessage(
                    "Password must contain at least one special character.");
        }
    }
}