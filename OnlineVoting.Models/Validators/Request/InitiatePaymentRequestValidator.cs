using FluentValidation;
using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Models.Validators
{
    public class InitiatePaymentRequestValidator : AbstractValidator<InitiatePaymentRequest>
    {
        public InitiatePaymentRequestValidator()
        {
            RuleFor(x => x.InvoiceId)
                .NotEmpty()
                .WithMessage("Invoice id is required.")
                .Must(id => Guid.TryParse(id, out _))
                .WithMessage("Invoice id must be a valid GUID.");

            RuleFor(x => x.PaymentGatewayId)
                .GreaterThan(0)
                .WithMessage("Payment gateway id must be greater than 0.");

            RuleFor(x => x.IdempotencyKey)
                .NotEmpty()
                .WithMessage("Idempotency key is required.")
                .MaximumLength(200)
                .WithMessage("Idempotency key cannot exceed 200 characters.");
        }
    }
}