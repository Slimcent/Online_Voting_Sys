using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Models.Dtos.Request
{
    /// <summary>
    /// Represents a request to create a position application.
    /// </summary>
    public class CreatePositionApplicationRequest
    {
        /// <summary>
        /// Gets or sets the election position identifier the student is applying for.
        /// </summary>
        public string ElectionPositionId { get; set; }

        /// <summary>
        /// Gets or sets the idempotency key used to prevent duplicate processing of the request.
        /// </summary>
        public required string IdempotencyKey { get; set; }
    }

    /// <summary>
    /// Represents a request to retrieve position applications.
    /// </summary>
    public class PositionApplicationRequest : RequestParameters
    {
        /// <summary>
        /// Gets or sets the position application status identifier used to filter applications.
        /// </summary>
        public int? PositionApplicationStatusId { get; set; }
    }

    /// <summary>
    /// Represents a request to approve or reject a position application.
    /// </summary>
    public class ApproveOrRejectPositionApplicationRequest
    {
        /// <summary>
        /// Gets or sets the position application identifier.
        /// </summary>
        public string PositionApplicationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the target position application status identifier.
        /// </summary>
        public int PositionApplicationStatusId { get; set; }

        /// <summary>
        /// Gets or sets the optional reason for the decision.
        /// </summary>
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Represents a request to retrieve position applications with contestants.
    /// </summary>
    public class PositionApplicationWithContestantRequest : RequestParameters
    {
        /// <summary>
        /// Gets or sets the election identifier used to filter contestants.
        /// </summary>
        public string? ElectionId { get; set; }

        /// <summary>
        /// Gets or sets the position identifier used to filter contestants.
        /// </summary>
        public string? PositionId { get; set; }

        /// <summary>
        /// Gets or sets whether to filter by active contestants.
        /// </summary>
        public bool? ContestantActive { get; set; }
    }
}