namespace OnlineVoting.Models.Dtos.Response
{
    public class PaymentResponse
    {
    }

    /// <summary>
    /// Represents an invoice created for a position application.
    /// </summary>
    public class InvoiceResponse
    {
        /// <summary>
        /// Gets or sets the invoice identifier.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the invoice number.
        /// </summary>
        public string InvoiceNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the invoice amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the invoice currency.
        /// </summary>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the invoice status.
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the response returned after a payment is initiated.
    /// </summary>
    public class InitiatePaymentResponse
    {
        /// <summary>
        /// Gets or sets the payment transaction identifier.
        /// </summary>
        public string PaymentTransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the payment reference.
        /// </summary>
        public string PaymentReference { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the provider reference.
        /// </summary>
        public string? ProviderReference { get; set; }

        /// <summary>
        /// Gets or sets the checkout URL returned by the payment gateway.
        /// </summary>
        public string? CheckoutUrl { get; set; }

        /// <summary>
        /// Gets or sets the payment gateway code.
        /// </summary>
        public string PaymentGatewayCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the payment status.
        /// </summary>
        public string PaymentStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the payment amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the payment currency.
        /// </summary>
        public string Currency { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the result of a payment verification.
    /// </summary>
    public class PaymentVerificationResponse
    {
        /// <summary>
        /// Gets or sets the payment transaction identifier.
        /// </summary>
        public string PaymentTransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the payment reference.
        /// </summary>
        public string PaymentReference { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the provider reference.
        /// </summary>
        public string? ProviderReference { get; set; }

        /// <summary>
        /// Gets or sets the payment status.
        /// </summary>
        public string PaymentStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the invoice status.
        /// </summary>
        public string InvoiceStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the currency.
        /// </summary>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time the payment was completed.
        /// </summary>
        public DateTime? PaidAt { get; set; }
    }

    /// <summary>
    /// Represents a stored payment transaction.
    /// </summary>
    public class PaymentTransactionResponse
    {
        /// <summary>
        /// Gets or sets the payment transaction identifier.
        /// </summary>
        public string PaymentTransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the invoice identifier.
        /// </summary>
        public string InvoiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the payment reference.
        /// </summary>
        public string PaymentReference { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the provider reference.
        /// </summary>
        public string? ProviderReference { get; set; }

        /// <summary>
        /// Gets or sets the payment gateway.
        /// </summary>
        public string PaymentGateway { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the payment status.
        /// </summary>
        public string PaymentStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the student identifier.
        /// </summary>
        public Guid StudentId { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the payer first name.
        /// </summary>
        public string PayerFirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the payer last name.
        /// </summary>
        public string PayerLastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the payer email.
        /// </summary>
        public string? PayerEmail { get; set; }

        /// <summary>
        /// Gets or sets the registration number.
        /// </summary>
        public string? RegistrationNumber { get; set; }

        /// <summary>
        /// Gets or sets the payment amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the payment currency.
        /// </summary>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the payment failure reason.
        /// </summary>
        public string? FailureReason { get; set; }

        /// <summary>
        /// Gets or sets the date and time the payment was completed.
        /// </summary>
        public DateTime? PaidAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time the payment transaction was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
