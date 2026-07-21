using FluentValidation;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Shared;

namespace OnlineVoting.Models.Validators.Request
{
    public class AddUserToRoleRequestValidator : AbstractValidator<AddUserToRoleRequest>
    {
        public AddUserToRoleRequestValidator()
        {
            RuleFor(request => request.Name)
                .SetValidator(new NameValidator());

            RuleFor(request => request.Email)
                .SetValidator(new EmailValidator());
        }
    }
}