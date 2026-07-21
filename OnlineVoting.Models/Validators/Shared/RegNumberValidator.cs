using FluentValidation;

namespace OnlineVoting.Models.Validators.Shared
{
    public class RegNumberValidator : AbstractValidator<string>
    {
        public RegNumberValidator()
        {
            RuleFor(regNumber => regNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Registration number cannot be empty.");
        }
    }
}