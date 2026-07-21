using FluentValidation;

namespace OnlineVoting.Models.Validators.Shared
{
    public class EmailValidator : AbstractValidator<string>
    {
        public EmailValidator()
        {
            RuleFor(email => email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Email cannot be empty.")
                .EmailAddress()
                .WithMessage("Email format is invalid.");
        }
    }
}