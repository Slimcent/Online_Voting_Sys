using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Documentation.Models;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Api.Documentation.Definitions.EndpointDefinitions
{
    public static class ElectionPositionDocumentation
    {
        public static readonly IReadOnlyDictionary<string, ApiOperationDocumentation> Operations = new Dictionary<string, ApiOperationDocumentation>
        {
            [ElectionPositionDocumentationKeys.GetElectionPositions] = new ApiOperationDocumentation
            {
                Summary = "Gets paged list of election positions.",
                Description = "Returns a paginated and searchable list of election positions including the number of applications for each position.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election positions were retrieved successfully.",
                        ResponseType = typeof(PagedResponse<ElectionPositionResponse>)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden()
                }
            },

            [ElectionPositionDocumentationKeys.GetElectionPositionsWithApplications] = new ApiOperationDocumentation
            {
                Summary = "Gets paged list of election positions with applications.",
                Description = "Returns a paginated and searchable list of election positions including their matching applications and application details.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election positions and applications were retrieved successfully.",
                        ResponseType = typeof(PagedResponse<ElectionPositionWithApplicationsResponse>)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden()
                }
            },

            [ElectionPositionDocumentationKeys.CreateElectionPosition] = new ApiOperationDocumentation
            {
                Summary = "Creates an election position.",
                Description = "Creates a new election position for the specified election.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["201"] = new ApiResponseDocumentation
                    {
                        Description = "The election position was created successfully.",
                        ResponseType = typeof(string)
                    },
                    ["400"] = CommonApiResponses.BadRequest(),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound(),
                    ["409"] = CommonApiResponses.Conflict()
                }
            },

            [ElectionPositionDocumentationKeys.CreateElectionPositions] = new ApiOperationDocumentation
            {
                Summary = "Creates multiple election positions.",
                Description = "Creates multiple election positions for the specified election.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["201"] = new ApiResponseDocumentation
                    {
                        Description = "The election positions were created successfully.",
                        ResponseType = typeof(string)
                    },
                    ["400"] = CommonApiResponses.BadRequest(),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound(),
                    ["409"] = CommonApiResponses.Conflict()
                }
            },

            [ElectionPositionDocumentationKeys.UpdateElectionPosition] = new ApiOperationDocumentation
            {
                Summary = "Updates an election position.",
                Description = "Updates an existing election position.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election position was updated successfully.",
                        ResponseType = typeof(string)
                    },
                    ["400"] = CommonApiResponses.BadRequest(),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound(),
                    ["409"] = CommonApiResponses.Conflict()
                }
            },

            [ElectionPositionDocumentationKeys.DeleteElectionPosition] = new ApiOperationDocumentation
            {
                Summary = "Deletes an election position.",
                Description = "Deletes an election position when it has no dependent applications.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election position was deleted successfully.",
                        ResponseType = typeof(string)
                    },
                    ["400"] = CommonApiResponses.BadRequest(),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound(),
                    ["409"] = CommonApiResponses.Conflict()
                }
            },

            [ElectionPositionDocumentationKeys.ToggleElectionPositionActivation] = new ApiOperationDocumentation
            {
                Summary = "Toggles election position activation.",
                Description = "Activates or deactivates an election position.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election position activation status was updated successfully.",
                        ResponseType = typeof(string)
                    },
                    ["400"] = CommonApiResponses.BadRequest(),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound()
                }
            }
        };
    }
}