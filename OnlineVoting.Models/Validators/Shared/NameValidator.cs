using FluentValidation;

namespace OnlineVoting.Models.Validators.Shared
{
    public class NameValidator : AbstractValidator<string>
    {
        public NameValidator()
        {
            RuleFor(name => name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Name cannot be empty.")
                .Length(2, 100)
                .WithMessage("Name must be between 2 and 100 characters.");
        }
    }
}