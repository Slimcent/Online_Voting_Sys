using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Documentation.Models;

namespace OnlineVoting.Api.Documentation.Definitions.EndpointDefinitions
{
    public static class FacultyDocumentation
    {
        public static readonly IReadOnlyDictionary<string, ApiOperationDocumentation> Operations = new Dictionary<string, ApiOperationDocumentation>
        {
            [FacultyDocumentationKeys.CreateFaculty] = new ApiOperationDocumentation
            {
                Summary = "Creates a faculty.",
                Description = "Creates a new faculty using the supplied name.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The faculty was created successfully.",

                        ResponseType = typeof(string)
                    },

                    ["400"] = CommonApiResponses.BadRequest(),

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden(),

                    ["409"] = CommonApiResponses.Conflict("A faculty with the supplied name already exists.")
                }
            }
        };
    }
}