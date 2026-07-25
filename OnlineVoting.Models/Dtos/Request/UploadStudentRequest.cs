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
            RequiredFields = new[]
            {
                "RegNumber",
                "FirstName",
                "LastName",
                "Email"
            };

            IgnoreFields = new[]
            {
                "SN"
            };
        }

        [JsonIgnore]
        public string[] RequiredFields { get; set; }

        [JsonIgnore]
        public string[] IgnoreFields { get; set; }

        /// <summary>
        /// The Excel file containing the student records.
        /// </summary>
        public required IFormFile File { get; set; }
    }
}