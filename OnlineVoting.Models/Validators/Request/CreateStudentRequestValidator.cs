using FluentValidation;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Validators.Shared;

namespace OnlineVoting.Models.Validators.Request
{
    public class CreateStudentRequestValidator
        : CreateUserRequestValidatorBase<CreateStudentRequest>
    {
        public CreateStudentRequestValidator()
        {
            RuleFor(request => request.DepartmentId)
                .GreaterThan(0)
                .WithMessage("Department is required.");
        }
    }
}