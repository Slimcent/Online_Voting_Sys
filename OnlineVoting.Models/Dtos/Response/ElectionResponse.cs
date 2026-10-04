

using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Models.Dtos.Response
{
    /// <summary>
    /// Represents an election.
    /// </summary>
    public class ElectionResponse
    {
        /// <summary>
        /// The identifier of the election.
        /// </summary>
        /// <example>4eeead52-a962-452b-bb14-79ce1dc01fe2</example>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The name of the election.
        /// </summary>
        /// <example>Computer Engineering Department Election</example>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The identifier of the academic year.
        /// </summary>
        /// <example>2</example>
        public long YearId { get; set; }

        /// <summary>
        /// The academic year.
        /// </summary>
        /// <example>2026/2027</example>
        public string Year { get; set; } = string.Empty;

        /// <summary>
        /// The identifier of the election type.
        /// </summary>
        /// <example>1</example>
        public int ElectionTypeId { get; set; }

        /// <summary>
        /// The election type.
        /// </summary>
        /// <example>Department Election</example>
        public string ElectionType { get; set; } = string.Empty;

        /// <summary>
        /// The identifier of the election status.
        /// </summary>
        /// <example>1</example>
        public int ElectionStatusId { get; set; }

        /// <summary>
        /// The election status.
        /// </summary>
        /// <example>Draft</example>
        public string ElectionStatus { get; set; } = string.Empty;

        /// <summary>
        /// The identifier of the faculty associated with the election.
        /// </summary>
        /// <example>1</example>
        public long? FacultyId { get; set; }

        /// <summary>
        /// The faculty associated with the election.
        /// </summary>
        /// <example>Faculty of Engineering</example>
        public string? Faculty { get; set; }

        /// <summary>
        /// The identifier of the department associated with the election.
        /// </summary>
        /// <example>4</example>
        public long? DepartmentId { get; set; }

        /// <summary>
        /// The department associated with the election.
        /// </summary>
        /// <example>Computer Engineering</example>
        public string? Department { get; set; }

        /// <summary>
        /// The date and time when applications open.
        /// </summary>
        public string? ApplicationStartAt { get; set; }

        /// <summary>
        /// The date and time when applications close.
        /// </summary>
        public string? ApplicationEndAt { get; set; }

        /// <summary>
        /// The date and time when voter registration opens.
        /// </summary>
        public string? VoterRegistrationStartAt { get; set; }

        /// <summary>
        /// The date and time when voter registration closes.
        /// </summary>
        public string? VoterRegistrationEndAt { get; set; }

        /// <summary>
        /// The date and time when voting opens.
        /// </summary>
        public string? VotingStartAt { get; set; }

        /// <summary>
        /// The date and time when voting closes.
        /// </summary>
        public string? VotingEndAt { get; set; }

        /// <summary>
        /// Indicates whether the election is active.
        /// </summary>
        /// <example>true</example>
        public bool Active { get; set; }

        /// <summary>
        /// The number of positions configured for the election.
        /// </summary>
        /// <example>6</example>
        public int NumberOfElectionPositions { get; set; }
    }

    /// <summary>
    /// Represents an election status.
    /// </summary>
    public class ElectionStatusResponse
    {
        /// <summary>
        /// The identifier of the election status.
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>
        /// The name of the election status.
        /// </summary>
        /// <example>Draft</example>
        public string Name { get; set; }

        /// <summary>
        /// The stable code of the election status.
        /// </summary>
        /// <example>DRAFT</example>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// A short description of the election status.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Indicates whether the election status is active.
        /// </summary>
        /// <example>true</example>
        public bool Active { get; set; }

        /// <summary>
        /// The number of elections currently associated with the status.
        /// </summary>
        /// <example>4</example>
        public int NumberOfElections { get; set; }
    }

    /// <summary>
    /// Represents an election scope.
    /// </summary>
    public class ElectionScopeResponse
    {
        /// <summary>
        /// The identifier of the election scope.
        /// </summary>
        /// <example>2</example>
        public int Id { get; set; }

        /// <summary>
        /// The stable code of the election scope.
        /// </summary>
        /// <example>FACULTY</example>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// The name of the election scope.
        /// </summary>
        /// <example>Faculty</example>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// A short description of the election scope.
        /// </summary>
        /// <example>Applies within a faculty.</example>
        public string? Description { get; set; }

        /// <summary>
        /// Indicates whether the election scope is active.
        /// </summary>
        /// <example>true</example>
        public bool Active { get; set; }
    }

    /// <summary>
    /// Represents a position configured for an election.
    /// </summary>
    public class ElectionPositionResponse
    {
        /// <summary>
        /// The identifier of the election position.
        /// </summary>
        /// <example>4eeead52-a962-452b-bb14-79ce1dc01fe2</example>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The identifier of the election.
        /// </summary>
        /// <example>2feead52-a962-452b-bb14-79ce1dc01fe2</example>
        public string ElectionId { get; set; } = string.Empty;

        /// <summary>
        /// The name of the election.
        /// </summary>
        /// <example>2026 Department Election</example>
        public string Election { get; set; } = string.Empty;

        /// <summary>
        /// The identifier of the position.
        /// </summary>
        /// <example>6beead52-a962-452b-bb14-79ce1dc01fe2</example>
        public string PositionId { get; set; } = string.Empty;

        /// <summary>
        /// The name of the position.
        /// </summary>
        /// <example>President</example>
        public string Position { get; set; } = string.Empty;

        /// <summary>
        /// The application fee for the position.
        /// </summary>
        /// <example>1000</example>
        public decimal ApplicationFee { get; set; }

        /// <summary>
        /// The currency used for the application fee.
        /// </summary>
        /// <example>NGN</example>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the election position is active.
        /// </summary>
        /// <example>true</example>
        public bool Active { get; set; }

        /// <summary>
        /// The number of applications submitted for the election position.
        /// </summary>
        /// <example>12</example>
        public int NumberOfApplications { get; set; }
    }

    /// <summary>
    /// Represents an application submitted for an election position.
    /// </summary>
    public class ElectionPositionApplicationResponse
    {
        /// <summary>
        /// The identifier of the application.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The identifier of the student who submitted the application.
        /// </summary>
        public Guid StudentId { get; set; }

        /// <summary>
        /// The student's first name.
        /// </summary>
        /// <example>John</example>
        public string? FirstName { get; set; }

        /// <summary>
        /// The student's last name.
        /// </summary>
        /// <example>Doe</example>
        public string? LastName { get; set; }

        /// <summary>
        /// The student's registration number.
        /// </summary>
        /// <example>CE/2024/001</example>
        public string? RegNumber { get; set; }

        /// <summary>
        /// The student's department identifier.
        /// </summary>
        public long DepartmentId { get; set; }

        /// <summary>
        /// The student's department.
        /// </summary>
        /// <example>Computer Engineering</example>
        public string Department { get; set; } = string.Empty;

        /// <summary>
        /// The student's faculty identifier.
        /// </summary>
        public long FacultyId { get; set; }

        /// <summary>
        /// The student's faculty.
        /// </summary>
        /// <example>Engineering</example>
        public string Faculty { get; set; } = string.Empty;

        /// <summary>
        /// The identifier of the application status.
        /// </summary>
        public int PositionApplicationStatusId { get; set; }

        /// <summary>
        /// The application status.
        /// </summary>
        /// <example>Approved</example>
        public string PositionApplicationStatus { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the application is active.
        /// </summary>
        /// <example>true</example>
        public bool Active { get; set; }
    }

    /// <summary>
    /// Represents an election position together with its applications.
    /// </summary>
    public class ElectionPositionWithApplicationsResponse
    {
        public string Id { get; set; } = string.Empty;

        public string ElectionId { get; set; } = string.Empty;

        public string Election { get; set; } = string.Empty;

        public string PositionId { get; set; } = string.Empty;

        public string Position { get; set; } = string.Empty;

        public decimal ApplicationFee { get; set; }

        public string Currency { get; set; } = string.Empty;

        public bool Active { get; set; }

        public int NumberOfApplications { get; set; }

        public IEnumerable<ElectionPositionApplicationResponse> Applications { get; set; } = [];
    }
}
