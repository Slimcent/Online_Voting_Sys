using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace OnlineVoting.Models.Dtos.Request
{
    /// <summary>
    /// Represents the Excel file used to upload student records.
    /// </summary>
    public class UploadStudentRequest
    {
        public UploadStudentRequest()
        {
            RequiredColumns = new[]
            {
                "RegNumber",
                "FirstName",
                "LastName",
                "Email",
                "PhoneNumber",
                "Gender",
                "Department"
            };
        }

        [JsonIgnore]
        public string[] RequiredColumns { get; set; }
        public string[] UniqueColumns { get; set; } = { "RegNumber" };

        /// <summary>
        /// The Excel file containing the student records.
        /// </summary>
        public required IFormFile File { get; set; }
    }
}