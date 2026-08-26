using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Models.Dtos.Request
{
    /// <summary>
    /// Represents the information required to create one or more faculties.
    /// </summary>
    public class CreateFacultyRequest
    {
        /// <summary>
        /// The name of a single faculty to create.
        /// </summary>
        /// <example>Engineering</example>
        public string? Name { get; set; }

        /// <summary>
        /// The names of multiple faculties to create.
        /// </summary>
        /// <example>["Engineering", "Science"]</example>
        public List<string>? Names { get; set; }
    }


    /// <summary>
    /// Provides pagination, sorting and search parameters for faculty requests.
    /// </summary>
    public class FacultyRequestParameters : RequestParameters
    {
    }
}