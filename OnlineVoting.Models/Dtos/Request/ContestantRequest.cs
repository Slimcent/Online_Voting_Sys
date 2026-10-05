using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Models.Dtos.Request
{
    /// <summary>
    /// Represents a request to retrieve contestants.
    /// </summary>
    public class ContestantRequest : RequestParameters
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
        /// Gets or sets the election position identifier used to filter contestants.
        /// </summary>
        public string? ElectionPositionId { get; set; }

        /// <summary>
        /// Gets or sets whether to filter contestants by active status.
        /// </summary>
        public bool? Active { get; set; }
    }
}