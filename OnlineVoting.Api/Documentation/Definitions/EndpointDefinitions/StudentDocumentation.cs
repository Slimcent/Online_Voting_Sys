using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Documentation.Models;

namespace OnlineVoting.Api.Documentation.Definitions.EndpointDefinitions
{
    public static class StudentDocumentation
    {
        public static readonly IReadOnlyDictionary<string, ApiOperationDocumentation> Operations = new Dictionary<string, ApiOperationDocumentation>
        {
            [StudentDocumentationKeys.CreateStudent] = new ApiOperationDocumentation
            {
                Summary = "Creates a student.",
                Description = "Creates a new student account using the supplied personal and department information.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["201"] = new ApiResponseDocumentation
                    {
                        Description = "The student was created successfully."
                    },

                    ["400"] = CommonApiResponses.BadRequest(),

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden(),

                    ["404"] = CommonApiResponses.NotFound("A required related resource, such as the department or role, could not be found.")
                }
            },

            [StudentDocumentationKeys.DownloadStudentsExcelTemplate] = new ApiOperationDocumentation
            {
                Summary = "Downloads the students Excel template.",
                Description = "Downloads the Excel template used when uploading multiple student records.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation

                    {
                        Description = "The Excel template was generated successfully."
                    },

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden()
                }
            },

            [StudentDocumentationKeys.UploadStudents] = new ApiOperationDocumentation
            {
                Summary = "Uploads students from an Excel file.",
                Description = "Reads student records from the supplied Excel file and creates the corresponding student accounts.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The student file was processed successfully.",
                        ResponseType = typeof(string)
                    },

                    ["400"] = CommonApiResponses.BadRequest("The uploaded file is missing, invalid, or contains invalid student records."),

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden()
                }
            },

            [StudentDocumentationKeys.CreateContestant] = new ApiOperationDocumentation
            {
                Summary = "Creates a contestant.",
                Description = "Registers an existing student as a contestant for the specified position.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["201"] = new ApiResponseDocumentation
                    {
                        Description = "The contestant was created successfully."
                    },

                    ["400"] = CommonApiResponses.BadRequest("The registration number or position is invalid."),

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden(),

                    ["404"] = CommonApiResponses.NotFound("The student or position could not be found.")
                }
            }
        };
    }
}