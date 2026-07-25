namespace OnlineVoting.Models.Dtos.Request
{
    /// <summary>
    /// Represents the information required to create one or more departments.
    /// </summary>
    public class CreateDepartmentRequest
    {
        /// <summary>
        /// The name of a single department to create.
        /// </summary>
        /// <example>Computer Engineering</example>
        public string? Name { get; set; }

        /// <summary>
        /// The names of multiple departments to create.
        /// </summary>
        /// <example>["Computer Engineering", "Electrical Engineering"]</example>
        public List<string>? Names { get; set; }

        /// <summary>
        /// The identifier of the faculty to which the department belongs.
        /// </summary>
        /// <example>1</example>
        public int FacultyId { get; set; }
    }
}