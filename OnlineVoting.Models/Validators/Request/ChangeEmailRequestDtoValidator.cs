using FluentValidation;
using OnlineVoting.Models.Dtos.Request.Email;

namespace OnlineVoting.Models.Validators.Request.Email
{
    public class ChangeEmailRequestDtoValidator : AbstractValidator<ChangeEmailRequestDto>
    {
        public ChangeEmailRequestDtoValidator()
        {
            RuleFor(request => request.NewEmail)
                .NotEmpty()
                .WithMessage("New email cannot be empty.");

            RuleFor(request => request.Token)
                .NotEmpty()
                .WithMessage("Token cannot be empty.");
        }
    }
}