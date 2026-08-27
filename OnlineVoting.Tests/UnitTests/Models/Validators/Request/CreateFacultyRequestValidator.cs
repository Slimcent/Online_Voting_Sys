using FluentValidation;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Shared;

namespace OnlineVoting.Models.Validators.Request
{
    public class CreateFacultyRequestValidator : AbstractValidator<CreateFacultyRequest>
    {
        public CreateFacultyRequestValidator()
        {
            RuleFor(request => request)
                .Must(HaveOneFacultyInput)
                .WithMessage("Provide either a faculty name or a list of faculty names.");

            RuleFor(request => request)
                .Must(NotHaveBothFacultyInputs)
                .WithMessage("Provide either Name or Names, but not both.");

            RuleFor(request => request.Name!)
                .SetValidator(new NameValidator())
                .When(request => !string.IsNullOrWhiteSpace(request.Name));

            RuleForEach(request => request.Names!)
                .SetValidator(new NameValidator())
                .When(request => request.Names is { Count: > 0 });
        }

        private static bool HaveOneFacultyInput(CreateFacultyRequest request)
        {
            bool hasName = !string.IsNullOrWhiteSpace(request.Name);
            bool hasNames = request.Names is { Count: > 0 };

            return hasName || hasNames;
        }

        private static bool NotHaveBothFacultyInputs(CreateFacultyRequest request)
        {
            bool hasName = !string.IsNullOrWhiteSpace(request.Name);
            bool hasNames = request.Names is { Count: > 0 };

            return !(hasName && hasNames);
        }
    }
}