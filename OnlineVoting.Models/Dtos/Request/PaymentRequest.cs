using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Models.Dtos.Request
{
    public class PaymentRequest
    {
    }

    /// <summary>
    /// Represents a request to create an invoice for a position application.
    /// </summary>
    public class CreateInvoiceRequest
    {
        /// <summary>
        /// Gets or sets the position application identifier.
        /// </summary>
        public string PositionApplicationId { get; set; }

        /// <summary>
        /// Gets or sets the student identifier.
        /// </summary>
        public Guid StudentId { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public required string UserId { get; set; }

        /// <summary>
        /// Gets or sets the payer first name.
        /// </summary>
        public required string PayerFirstName { get; set; }

        /// <summary>
        /// Gets or sets the payer last name.
        /// </summary>
        public required string PayerLastName { get; set; }

        /// <summary>
        /// Gets or sets the payer email address.
        /// </summary>
        public string? PayerEmail { get; set; }

        /// <summary>
        /// Gets or sets the student registration number.
        /// </summary>
        public string? RegistrationNumber { get; set; }

        /// <summary>
        /// Gets or sets the invoice amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the invoice currency.
        /// </summary>
        public required string Currency { get; set; }
    }

    /// <summary>
    /// Represents the information required to initiate payment for an invoice.
    /// </summary>
    public class InitiatePaymentRequest
    {
        /// <summary>
        /// Gets or sets the invoice identifier.
        /// </summary>
        /// <example>78ca06f4-5e51-4e6e-9868-d55a4c6c37d0</example>
        public required string InvoiceId { get; set; }

        /// <summary>
        /// Gets or sets the payment gateway identifier.
        /// </summary>
        /// <example>1</example>
        public int PaymentGatewayId { get; set; }

        /// <summary>
        /// Gets or sets the idempotency key used to prevent duplicate payment initiation requests.
        /// </summary>
        /// <example>payment-78ca06f4-5e51-4e6e-9868-d55a4c6c37d0-001</example>
        public required string IdempotencyKey { get; set; }
    }

    public class InvoiceRequest : RequestParameters
    {
        /// <summary>
        /// Gets or sets the invoice status identifier used to filter invoices.
        /// </summary>
        public int? InvoiceStatusId { get; set; }

        /// <summary>
        /// Gets or sets the student identifier used to filter invoices.
        /// </summary>
        public Guid? StudentId { get; set; }

        /// <summary>
        /// Gets or sets the position application identifier used to filter invoices.
        /// </summary>
        public string? PositionApplicationId { get; set; }
    }

    public class PaymentTransactionRequest : RequestParameters
    {
        /// <summary>
        /// Gets or sets the payment gateway identifier used to filter payment transactions.
        /// </summary>
        public int? PaymentGatewayId { get; set; }

        /// <summary>
        /// Gets or sets the payment status identifier used to filter payment transactions.
        /// </summary>
        public int? PaymentStatusId { get; set; }

        /// <summary>
        /// Gets or sets the invoice identifier used to filter payment transactions.
        /// </summary>
        public string? InvoiceId { get; set; }

        /// <summary>
        /// Gets or sets the student identifier used to filter payment transactions.
        /// </summary>
        public Guid? StudentId { get; set; }

        /// <summary>
        /// Gets or sets the user identifier used to filter payment transactions.
        /// </summary>
        public string? UserId { get; set; }
    }
}
