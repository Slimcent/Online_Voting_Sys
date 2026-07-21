using FluentValidation;
using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Models.Validators.Shared
{
    public abstract class CreateUserRequestValidatorBase<TRequest> : AbstractValidator<TRequest> where TRequest : CreateUserRequest
    {
        protected CreateUserRequestValidatorBase()
        {
            RuleFor(request => request.FirstName)
                .SetValidator(new NameValidator());

            RuleFor(request => request.LastName)
                .SetValidator(new NameValidator());

            RuleFor(request => request.Email)
                .SetValidator(new EmailValidator());

            RuleFor(request => request.PhoneNumber)
                .SetValidator(new PhoneNumberValidator());

            RuleFor(request => request.GenderId)
                .GreaterThan(0)
                .WithMessage("Gender is required.");

            RuleFor(request => request.UserType)
                .GreaterThan(0)
                .WithMessage("User type is required.");

            RuleFor(request => request.Role)
                .NotEmpty()
                .WithMessage("Role cannot be empty.");
        }
    }
}