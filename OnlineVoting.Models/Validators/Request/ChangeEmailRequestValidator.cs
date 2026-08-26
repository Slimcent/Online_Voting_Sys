using FluentValidation;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Validators.Shared;

namespace OnlineVoting.Models.Validators.Request.Email
{
    public class ChangeEmailRequestValidator : AbstractValidator<ChangeEmailRequest>
    {
        public ChangeEmailRequestValidator()
        {
            RuleFor(request => request.Email!)
                .SetValidator(new EmailValidator());

            RuleFor(request => request.NewEmail!)
                .SetValidator(new EmailValidator());

            RuleFor(request => request.RecoveryEmail!)
                .SetValidator(new EmailValidator());
        }
    }
}