using FluentValidation;
using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Models.Validators
{
    public class CreatePositionApplicationRequestValidator : AbstractValidator<CreatePositionApplicationRequest>
    {
        public CreatePositionApplicationRequestValidator()
        {
            RuleFor(x => x.ElectionPositionId)
                .NotEmpty()
                .WithMessage("Election position id is required.")
                .Must(id => Guid.TryParse(id, out _))
                .WithMessage("Election position id must be a valid GUID.");

            RuleFor(x => x.IdempotencyKey)
                .NotEmpty()
                .WithMessage("Idempotency key is required.")
                .Must(id => Guid.TryParse(id, out _))
                .WithMessage("Idempotency key must be a valid GUID.");
        }
    }
}