using FluentValidation;

namespace OnlineVoting.Models.Validators.Shared
{
    public class PhoneNumberValidator : AbstractValidator<string>
    {
        public PhoneNumberValidator()
        {
            RuleFor(phoneNumber => phoneNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Phone number cannot be empty.")
                .Matches(@"^0\d{10}$")
                .WithMessage("Phone number is invalid.");
        }
    }
}