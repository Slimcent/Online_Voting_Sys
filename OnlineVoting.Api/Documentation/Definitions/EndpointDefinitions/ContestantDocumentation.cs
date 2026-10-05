using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Documentation.Models;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Api.Documentation
{
    public static class ContestantDocumentation
    {
        public static readonly Dictionary<string, ApiOperationDocumentation> Documentation = new()
        {
            [ContestantDocumentationKeys.GetContestants] = new ApiOperationDocumentation
            {
                Summary = "Gets contestants.",
                Description = "Returns a paginated and searchable list of contestants with their election and position details.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The contestants were retrieved successfully.",
                        ResponseType = typeof(PagedResponse<ContestantResponse>)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden()
                }
            },

            [ContestantDocumentationKeys.GetContestant] = new ApiOperationDocumentation
            {
                Summary = "Gets a contestant.",
                Description = "Returns a contestant with the related election, position and student details.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The contestant was retrieved successfully.",
                        ResponseType = typeof(ContestantResponse)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound()
                }
            },

            [ContestantDocumentationKeys.ToggleContestantActivation] = new ApiOperationDocumentation
            {
                Summary = "Toggles contestant activation.",
                Description = "Activates an inactive contestant or deactivates an active contestant.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The contestant activation status was updated successfully.",
                        ResponseType = typeof(string)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound()
                }
            },
        };
    }
}