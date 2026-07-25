using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Documentation.Models;

namespace OnlineVoting.Api.Documentation.Definitions.EndpointDefinitions
{
    public static class DepartmentDocumentation
    {
        public static readonly IReadOnlyDictionary<string, ApiOperationDocumentation> Operations = new Dictionary<string, ApiOperationDocumentation>
        {
            [DepartmentDocumentationKeys.CreateDepartment] = new ApiOperationDocumentation
            {
                Summary = "Creates a department.",

                Description = "Creates one or more departments under the specified faculty.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The department was created successfully.",

                        ResponseType = typeof(string)
                    },

                    ["400"] = CommonApiResponses.BadRequest("The department information is invalid."),

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden(),

                    ["404"] = CommonApiResponses.NotFound("The specified faculty could not be found."),

                    ["409"] = CommonApiResponses.Conflict("A department with the supplied name already exists.")
                }
            }
        };
    }
}