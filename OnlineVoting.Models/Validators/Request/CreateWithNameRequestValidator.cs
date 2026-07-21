using FluentValidation;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Shared;

namespace OnlineVoting.Models.Validators.Request
{
    public class CreateWithNameRequestValidator : AbstractValidator<CreateWithNameRequest>
    {
        public CreateWithNameRequestValidator()
        {
            RuleFor(request => request.Name)
                .SetValidator(new NameValidator());
        }
    }
}