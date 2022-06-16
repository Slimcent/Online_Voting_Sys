using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace OnlineVoting.Models.Dtos.Request
{
    public class UploadStudentRequestDto
    {
        public UploadStudentRequestDto()
        {
            RequiredFields = new[] { "RegNo", "FirstName", "LastName", "Email" };
            IgnoreFields = new[] { "SN" };
        }

        [JsonIgnore]
        public string[] RequiredFields { get; set; }

        [JsonIgnore]
        public string[] IgnoreFields { get; set; }
        public IFormFile File { get; set; }
    }
}
