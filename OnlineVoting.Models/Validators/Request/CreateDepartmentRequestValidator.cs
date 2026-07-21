using FluentValidation;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Shared;

namespace OnlineVoting.Models.Validators.Request
{
    public class CreateDepartmentRequestValidator
        : AbstractValidator<CreateDepartmentRequest>
    {
        public CreateDepartmentRequestValidator()
        {
            RuleFor(request => request.FacultyId)
                .GreaterThan(0)
                .WithMessage("Faculty is required.");

            RuleFor(request => request)
                .Must(HaveOneDepartmentInput)
                .WithMessage(
                    "Provide either a department name or a list of department names.");

            RuleFor(request => request)
                .Must(NotHaveBothDepartmentInputs)
                .WithMessage(
                    "Provide either Name or Names, but not both.");

            RuleFor(request => request.Name!)
                .SetValidator(new NameValidator())
                .When(request =>
                    !string.IsNullOrWhiteSpace(request.Name));

            RuleForEach(request => request.Names!)
                .SetValidator(new NameValidator())
                .When(request =>
                    request.Names is { Count: > 0 });
        }

        private static bool HaveOneDepartmentInput(CreateDepartmentRequest request)
        {
            bool hasName = !string.IsNullOrWhiteSpace(request.Name);

            bool hasNames = request.Names is { Count: > 0 };

            return hasName || hasNames;
        }

        private static bool NotHaveBothDepartmentInputs(CreateDepartmentRequest request)
        {
            bool hasName = !string.IsNullOrWhiteSpace(request.Name);

            bool hasNames = request.Names is { Count: > 0 };

            return !(hasName && hasNames);
        }
    }
}