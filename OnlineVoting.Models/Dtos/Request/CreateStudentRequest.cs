namespace OnlineVoting.Models.Dtos.Request
{
    /// <summary>
    /// Represents the information required to create a student.
    /// </summary>
    public class CreateStudentRequest : CreateUserRequest
    {
        /// <summary>
        /// The identifier of the department to which the student belongs.
        /// </summary>
        /// <example>1</example>
        public required int DepartmentId { get; set; }
    }
}