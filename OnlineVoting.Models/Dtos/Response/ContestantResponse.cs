namespace OnlineVoting.Models.Dtos.Response
{
    /// <summary>
    /// Represents a contestant.
    /// </summary>
    public class ContestantResponse
    {
        /// <summary>
        /// Gets or sets the contestant identifier.
        /// </summary>
        public string ContestantId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the election name.
        /// </summary>
        public string ElectionName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the position application identifier.
        /// </summary>
        public string PositionApplicationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the contestant's full name.
        /// </summary>
        public string ContestantName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the contestant's registration number.
        /// </summary>
        public string? RegistrationNumber { get; set; }

        /// <summary>
        /// Gets or sets the position name.
        /// </summary>
        public string PositionName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the contestant is active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the date and time the contestant was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
