using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Models.Dtos.Request
{

    /// <summary>
    /// Represents the information required to create or update an election type.
    /// </summary>
    public class CreateElectionTypeRequest
    {
        /// <summary>
        /// The identifier of the election type.
        /// </summary>
        /// <example>2</example>
        public int? Id { get; set; }

        /// <summary>
        /// The name of the election type.
        /// </summary>
        /// <example>Faculty Election</example>
        public required string Name { get; set; }

        /// <summary>
        /// A short description of the election type.
        /// </summary>
        /// <example>An election conducted within a faculty.</example>
        public string? Description { get; set; }

        /// <summary>
        /// The identifier of the election scope associated with the election type.
        /// </summary>
        /// <example>2</example>
        public int ElectionScopeId { get; set; }
    }

    public class ElectionTypeRequest : RequestParameters
    {
    }
}