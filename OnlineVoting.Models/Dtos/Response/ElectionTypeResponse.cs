namespace OnlineVoting.Models.Dtos.Response
{
    /// <summary>
    /// Represents an election type.
    /// </summary>
    public class ElectionTypeResponse
    {
        /// <summary>
        /// The identifier of the election type.
        /// </summary>
        /// <example>2</example>
        public int Id { get; set; }

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
        /// The identifier of the election scope.
        /// </summary>
        /// <example>2</example>
        public int ElectionScopeId { get; set; }

        /// <summary>
        /// The stable code of the election scope.
        /// </summary>
        /// <example>FACULTY</example>
        public required string ElectionScopeCode { get; set; }

        /// <summary>
        /// The name of the election scope.
        /// </summary>
        /// <example>Faculty</example>
        public required string ElectionScope { get; set; }

        /// <summary>
        /// Indicates whether the election type is active.
        /// </summary>
        /// <example>true</example>
        public bool Active { get; set; }

        /// <summary>
        /// The number of election positions associated with this election type.
        /// </summary>
        /// <example>5</example>
        public int NumberOfElectionPositions { get; set; }

        /// <summary>
        /// The number of aspirants associated with this election type.
        /// </summary>
        /// <example>12</example>
        public int NumberOfAspirants { get; set; }

        /// <summary>
        /// The number of contestants associated with this election type.
        /// </summary>
        /// <example>8</example>
        public int NumberOfContestants { get; set; }
    }
}