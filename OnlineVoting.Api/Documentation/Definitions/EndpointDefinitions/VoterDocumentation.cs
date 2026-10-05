using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Documentation.Models;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Api.Documentation.Definitions.EndpointDefinitions
{
    public static class VoterDocumentation
    {
        public static readonly IReadOnlyDictionary<string, ApiOperationDocumentation> Operations = new Dictionary<string, ApiOperationDocumentation>
        {
            [VoterDocumentationKeys.RegisterVoter] = new ApiOperationDocumentation
            {
                Summary = "Registers a voter.",
                Description = "Registers an eligible student to vote in the specified election and generates a voting code.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["201"] = new ApiResponseDocumentation
                    {
                        Description = "The voter was registered successfully.",
                        ResponseType = typeof(RegisteredVoterResponse)
                    },
                    ["400"] = CommonApiResponses.BadRequest("The voter registration information is invalid."),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden("The student is not eligible to register for the specified election."),
                    ["404"] = CommonApiResponses.NotFound("The student or election could not be found."),
                    ["409"] = CommonApiResponses.Conflict("Voter registration is unavailable or the student has already registered for the specified election.")
                }
            },

            [VoterDocumentationKeys.GetRegisteredVoter] = new ApiOperationDocumentation
            {
                Summary = "Gets a registered voter.",
                Description = "Returns the registered voter with the specified identifier.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The registered voter was retrieved successfully.",
                        ResponseType = typeof(RegisteredVoterResponse)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound("The specified registered voter could not be found.")
                }
            },

            [VoterDocumentationKeys.GetRegisteredVoters] = new ApiOperationDocumentation
            {
                Summary = "Gets registered voters.",
                Description = "Returns a paginated list of registered voters and supports filtering by election, student and active status.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The registered voters were retrieved successfully.",
                        ResponseType = typeof(PagedResponse<RegisteredVoterResponse>)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden()
                }
            }
        };
    }
}