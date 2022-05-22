using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace OnlineVoting.Models.Dtos.Request
{
    public class UploadStudentRequestDto
    {
        public UploadStudentRequestDto()
        {
            RequiredHeaders = new[] { "RegNo", "LastName", "FirstName", "Sex", "PhoneNumber", "Email" };
            NullableFields = new[] { "Sex", "PhoneNumber" };
        }

        [JsonIgnore]
        public string[] RequiredHeaders { get; set; }

        [JsonIgnore]
        public string[] NullableFields { get; set; }

        public IFormFile File { get; set; }
    }
}
