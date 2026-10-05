using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Models.Dtos.Request
{
    /// <summary>
    /// Represents the filters used when retrieving elections.
    /// </summary>
    public class ElectionRequest : RequestParameters
    {
        /// <summary>
        /// Filters elections by academic year.
        /// </summary>
        /// <example>2</example>
        public long? YearId { get; set; }

        /// <summary>
        /// Filters elections by election type.
        /// </summary>
        /// <example>1</example>
        public int? ElectionTypeId { get; set; }

        /// <summary>
        /// Filters elections by election status.
        /// </summary>
        /// <example>1</example>
        public int? ElectionStatusId { get; set; }

        /// <summary>
        /// Filters elections by faculty.
        /// </summary>
        /// <example>1</example>
        public long? FacultyId { get; set; }

        /// <summary>
        /// Filters elections by department.
        /// </summary>
        /// <example>4</example>
        public long? DepartmentId { get; set; }

        /// <summary>
        /// Filters elections by active status.
        /// </summary>
        /// <example>true</example>
        public bool? Active { get; set; }
    }

    public class ElectionStatusRequest : RequestParameters
    {
    }

    /// <summary>
    /// Represents the information required to update an election status.
    /// </summary>
    public class UpdateElectionStatusRequest
    {
        /// <summary>
        /// The identifier of the election status.
        /// </summary>
        /// <example>2</example>
        public int Id { get; set; }

        /// <summary>
        /// The name of the election status.
        /// </summary>
        /// <example>Registration Open</example>
        public required string Name { get; set; }

        /// <summary>
        /// A short description of the election status.
        /// </summary>
        /// <example>Applications are currently being accepted.</example>
        public string? Description { get; set; }
    }

    /// <summary>
    /// Represents the information required to create or update an election.
    /// </summary>
    public class CreateElectionRequest
    {
        /// <summary>
        /// The identifier of the election.
        /// </summary>
        /// <example>4eeead52-a962-452b-bb14-79ce1dc01fe2</example>
        public string? Id { get; set; }

        /// <summary>
        /// The name of the election.
        /// </summary>
        /// <example>2026 Faculty of Engineering Election</example>
        public required string Name { get; set; }

        /// <summary>
        /// The identifier of the academic year.
        /// </summary>
        /// <example>2</example>
        public long YearId { get; set; }

        /// <summary>
        /// The identifier of the election type.
        /// </summary>
        /// <example>2</example>
        public int ElectionTypeId { get; set; }

        /// <summary>
        /// The identifier of the election status.
        /// </summary>
        /// <example>1</example>
        public int ElectionStatusId { get; set; }

        /// <summary>
        /// The identifier of the faculty associated with the election.
        /// </summary>
        /// <example>1</example>
        public long? FacultyId { get; set; }

        /// <summary>
        /// The identifier of the department associated with the election.
        /// </summary>
        /// <example>1</example>
        public long? DepartmentId { get; set; }

        /// <summary>
        /// The date and time when applications open.
        /// </summary>
        /// <example>2026-10-01T08:00:00</example>
        public string? ApplicationStartAt { get; set; }

        /// <summary>
        /// The date and time when applications close.
        /// </summary>
        /// <example>2026-10-07T18:00:00</example>
        public string? ApplicationEndAt { get; set; }

        /// <summary>
        /// The date and time when voter registration opens.
        /// </summary>
        /// <example>2026-10-08T08:00:00</example>
        public string? VoterRegistrationStartAt { get; set; }

        /// <summary>
        /// The date and time when voter registration closes.
        /// </summary>
        /// <example>2026-10-12T18:00:00</example>
        public string? VoterRegistrationEndAt { get; set; }

        /// <summary>
        /// The date and time when voting opens.
        /// </summary>
        /// <example>2026-10-15T08:00:00</example>
        public string? VotingStartAt { get; set; }

        /// <summary>
        /// The date and time when voting closes.
        /// </summary>
        /// <example>2026-10-15T18:00:00</example>
        public string? VotingEndAt { get; set; }
    }

    /// <summary>
    /// Represents the information required to update an election scope.
    /// </summary>
    public class UpdateElectionScopeRequest
    {
        /// <summary>
        /// The identifier of the election scope.
        /// </summary>
        /// <example>2</example>
        public int Id { get; set; }

        /// <summary>
        /// The display name of the election scope.
        /// </summary>
        /// <example>Faculty Level</example>
        public required string Name { get; set; }

        /// <summary>
        /// A short description of the election scope.
        /// </summary>
        /// <example>Applies to elections conducted within a faculty.</example>
        public string? Description { get; set; }
    }

    /// <summary>
    /// Represents filtering and pagination options for election positions.
    /// </summary>
    public class ElectionPositionRequest : RequestParameters
    {
        /// <summary>
        /// Filters election positions by election identifier.
        /// </summary>
        /// <example>8aeead52-a962-452b-bb14-79ce1dc01fe2</example>
        public string? ElectionId { get; set; }

        /// <summary>
        /// Filters election positions by position identifier.
        /// </summary>
        /// <example>4eeead52-a962-452b-bb14-79ce1dc01fe2</example>
        public string? PositionId { get; set; }

        /// <summary>
        /// Filters election positions by their activation status.
        /// </summary>
        /// <example>true</example>
        public bool? ElectionPositionActive { get; set; }

        /// <summary>
        /// Filters applications by application status.
        /// </summary>
        /// <example>3</example>
        public int? PositionApplicationStatusId { get; set; }

        /// <summary>
        /// Filters applications by department.
        /// </summary>
        /// <example>4</example>
        public long? DepartmentId { get; set; }

        /// <summary>
        /// Filters applications by faculty.
        /// </summary>
        /// <example>1</example>
        public long? FacultyId { get; set; }

        /// <summary>
        /// Filters applications by their activation status.
        /// </summary>
        /// <example>true</example>
        public bool? ApplicationActive { get; set; }
    }

    /// <summary>
    /// Represents the information required to add multiple positions to an election.
    /// </summary>
    public class CreateElectionPositionsRequest
    {
        /// <summary>
        /// The identifier of the election.
        /// </summary>
        /// <example>4eeead52-a962-452b-bb14-79ce1dc01fe2</example>
        public required string ElectionId { get; set; }

        /// <summary>
        /// The positions to add to the election.
        /// </summary>
        public required List<CreateElectionPositionItemRequest> ElectionPositions { get; set; }
    }

    /// <summary>
    /// Represents a position to add as part of a bulk election position request.
    /// </summary>
    public class CreateElectionPositionItemRequest
    {
        /// <summary>
        /// The identifier of the position.
        /// </summary>
        /// <example>4eeead52-a962-452b-bb14-79ce1dc01fe2</example>
        public required string PositionId { get; set; }

        /// <summary>
        /// The application fee for the position.
        /// </summary>
        /// <example>1000</example>
        public decimal ApplicationFee { get; set; }

        /// <summary>
        /// The currency used for the application fee.
        /// </summary>
        /// <example>NGN</example>
        public required string Currency { get; set; }
    }

    /// <summary>
    /// Represents the information required to create or update an election position.
    /// </summary>
    public class CreateElectionPositionRequest
    {
        /// <summary>
        /// The identifier of the election position.
        /// </summary>
        /// <example>4eeead52-a962-452b-bb14-79ce1dc01fe2</example>
        public string? Id { get; set; }

        /// <summary>
        /// The identifier of the election.
        /// </summary>
        /// <example>4eeead52-a962-452b-bb14-79ce1dc01fe2</example>
        public required string ElectionId { get; set; }

        /// <summary>
        /// The identifier of the position.
        /// </summary>
        /// <example>4eeead52-a962-452b-bb14-79ce1dc01fe2</example>
        public required string PositionId { get; set; }

        /// <summary>
        /// The application fee for the position.
        /// </summary>
        /// <example>1000</example>
        public decimal ApplicationFee { get; set; }

        /// <summary>
        /// The currency used for the application fee.
        /// </summary>
        /// <example>NGN</example>
        public required string Currency { get; set; }
    }

    public class ElectionScopeRequest : RequestParameters
    {
    }
}