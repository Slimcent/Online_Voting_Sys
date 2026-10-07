namespace OnlineVoting.Models.Dtos.Response
{
    /// <summary>
    /// Represents the response returned after a position application and its invoice are created.
    /// </summary>
    public class CreatePositionApplicationResponse
    {
        /// <summary>
        /// Gets or sets the created position application identifier.
        /// </summary>
        public string PositionApplicationId { get; set; }

        /// <summary>
        /// Gets or sets the generated invoice identifier.
        /// </summary>
        public string InvoiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the generated invoice number.
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
        /// Gets or sets the current position application status.
        /// </summary>
        public string ApplicationStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current invoice status.
        /// </summary>
        public string InvoiceStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a position application.
    /// </summary>
    public class PositionApplicationResponse
    {
        /// <summary>
        /// Gets or sets the position application identifier.
        /// </summary>
        public string PositionApplicationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the election position identifier.
        /// </summary>
        public string ElectionPositionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the election name.
        /// </summary>
        public string ElectionName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the position name.
        /// </summary>
        public string PositionName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current application status.
        /// </summary>
        public string ApplicationStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the invoice identifier.
        /// </summary>
        public string? InvoiceId { get; set; }

        /// <summary>
        /// Gets or sets the invoice number.
        /// </summary>
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// Gets or sets the invoice amount.
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Gets or sets the invoice currency.
        /// </summary>
        public string? Currency { get; set; }

        /// <summary>
        /// Gets or sets the current invoice status.
        /// </summary>
        public string? InvoiceStatus { get; set; }

        /// <summary>
        /// Gets or sets the student identifier.
        /// </summary>
        public Guid StudentId { get; set; }

        /// <summary>
        /// Gets or sets the student's registration number.
        /// </summary>
        public string? RegistrationNumber { get; set; }

        /// <summary>
        /// Gets or sets the student's full name.
        /// </summary>
        public string StudentName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the student's email address.
        /// </summary>
        public string? StudentEmail { get; set; }

        /// <summary>
        /// Gets or sets the date and time the position application was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the contestant created from the position application.
        /// </summary>
        public ContestantResponse? Contestant { get; set; }
    }
}