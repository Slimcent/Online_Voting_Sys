using FluentValidation;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Shared;

namespace OnlineVoting.Models.Validators.Request
{
    public class CreateVoterRequestValidator : AbstractValidator<CreateVoterRequest>
    {
        public CreateVoterRequestValidator()
        {
            RuleFor(request => request.RegNumber)
                .SetValidator(new RegNumberValidator());
        }
    }
}