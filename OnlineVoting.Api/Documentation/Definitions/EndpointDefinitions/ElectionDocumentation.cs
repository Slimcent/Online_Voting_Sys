using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Documentation.Models;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Api.Documentation.Definitions.EndpointDefinitions
{
    public static class ElectionDocumentation
    {
        public static readonly IReadOnlyDictionary<string, ApiOperationDocumentation> Operations = new Dictionary<string, ApiOperationDocumentation>
        {
            [ElectionDocumentationKeys.CreateElection] = new ApiOperationDocumentation
            {
                Summary = "Creates an election.",
                Description = "Creates an election for the selected academic year, election type, initial status and election scope. " +
                    "A new election can start in Draft or Registration Open status. Registration Open requires an application period.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["201"] = new ApiResponseDocumentation
                    {
                        Description = "The election was created successfully.",
                        ResponseType = typeof(string)
                    },
                    ["400"] = CommonApiResponses.BadRequest("The election information is invalid."),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound("One of the referenced election resources could not be found."),
                    ["409"] = CommonApiResponses.Conflict("An election already exists for the selected election type, year and scope.")
                }
            },

            [ElectionDocumentationKeys.GetElections] = new ApiOperationDocumentation
            {
                Summary = "Gets elections.",
                Description = "Returns elections matching the supplied search and filter criteria.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The elections were retrieved successfully.",
                        ResponseType = typeof(IEnumerable<ElectionResponse>)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden()
                }
            },

            [ElectionDocumentationKeys.GetPagedElections] = new ApiOperationDocumentation
            {
                Summary = "Gets paged elections.",
                Description = "Returns a paginated list of elections matching the supplied search and filter criteria.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The paginated elections were retrieved successfully.",
                        ResponseType = typeof(PagedResponse<ElectionResponse>)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden()
                }
            },

            [ElectionDocumentationKeys.GetElection] = new ApiOperationDocumentation
            {
                Summary = "Gets an election.",
                Description = "Returns the election with the specified identifier.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election was retrieved successfully.",
                        ResponseType = typeof(ElectionResponse)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound("The election could not be found.")
                }
            },

            [ElectionDocumentationKeys.UpdateElection] = new ApiOperationDocumentation
            {
                Summary = "Updates an election.",
                Description = "Updates an existing election, including its configuration, scope, schedule and status.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election was updated successfully.",
                        ResponseType = typeof(string)
                    },
                    ["400"] = CommonApiResponses.BadRequest("The election update request is invalid."),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound("The election or one of its referenced records could not be found."),
                    ["409"] = CommonApiResponses.Conflict("Another election already exists for the selected election type, year and scope.")
                }
            },

            [ElectionDocumentationKeys.ToggleElectionActivation] = new ApiOperationDocumentation
            {
                Summary = "Toggles election activation.",
                Description = "Activates an inactive election or deactivates an active election.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election activation status was updated successfully.",
                        ResponseType = typeof(string)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound("The election could not be found.")
                }
            },

            [ElectionDocumentationKeys.GetElectionStatuses] = new ApiOperationDocumentation
            {
                Summary = "Gets election statuses.",
                Description = "Returns all election statuses including the number of elections associated with each status.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election statuses were retrieved successfully.",
                        ResponseType = typeof(IEnumerable<ElectionStatusResponse>)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden()
                }
            },

            [ElectionDocumentationKeys.GetPagedElectionStatuses] = new ApiOperationDocumentation
            {
                Summary = "Gets paged list of election statuses.",
                Description = "Returns a paginated list and searchable list of election statuses including the number of elections associated with each status.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election statuses were retrieved successfully.",
                        ResponseType = typeof(PagedResponse<ElectionStatusResponse>)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden()
                }
            },

            [ElectionDocumentationKeys.GetElectionStatus] = new ApiOperationDocumentation
            {
                Summary = "Gets an election status.",
                Description = "Returns the election status with the specified id including the number of elections associated with the status.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election status was retrieved successfully.",
                        ResponseType = typeof(ElectionStatusResponse)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound("The specified election status could not be found.")
                }
            },

            [ElectionDocumentationKeys.UpdateElectionStatus] = new ApiOperationDocumentation
            {
                Summary = "Updates an election status.",
                Description = "Updates the election status with the specified id using the supplied name and description.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election status was updated successfully.",
                        ResponseType = typeof(string)
                    },
                    ["400"] = CommonApiResponses.BadRequest("The election status information is invalid."),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound("The specified election status could not be found."),
                    ["409"] = CommonApiResponses.Conflict("An election status with the supplied name already exists.")
                }
            },

            [ElectionDocumentationKeys.GetElectionScopes] = new ApiOperationDocumentation
            {
                Summary = "Gets election scopes.",
                Description = "Returns all election scopes.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election scopes were retrieved successfully.",
                        ResponseType = typeof(IEnumerable<ElectionScopeResponse>)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden()
                }
            },

            [ElectionDocumentationKeys.GetPagedElectionScopes] = new ApiOperationDocumentation
            {
                Summary = "Gets paged list of election scopes.",
                Description = "Returns a paginated and searchable list of election scopes.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election scopes were retrieved successfully.",
                        ResponseType = typeof(PagedResponse<ElectionScopeResponse>)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden()
                }
            },

            [ElectionDocumentationKeys.GetElectionScope] = new ApiOperationDocumentation
            {
                Summary = "Gets an election scope.",
                Description = "Returns the election scope with the specified id.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election scope was retrieved successfully.",
                        ResponseType = typeof(ElectionScopeResponse)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound("The specified election scope could not be found.")
                }
            },

            [ElectionDocumentationKeys.UpdateElectionScope] = new ApiOperationDocumentation
            {
                Summary = "Updates an election scope.",
                Description = "Updates the display name and description of the election scope with the specified id. The structural scope code cannot be changed.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election scope was updated successfully.",
                        ResponseType = typeof(string)
                    },
                    ["400"] = CommonApiResponses.BadRequest("The election scope information is invalid."),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound("The specified election scope could not be found."),
                    ["409"] = CommonApiResponses.Conflict("An election scope with the supplied name already exists.")
                }
            },

            [ElectionDocumentationKeys.ToggleElectionScopeActivation] = new ApiOperationDocumentation
            {
                Summary = "Toggles election scope activation.",
                Description = "Toggles the active status of the election scope with the specified id. The active status controls its visibility and availability for use when configuring election types.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election scope activation status was updated successfully.",
                        ResponseType = typeof(string)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound("The specified election scope could not be found.")
                }
            }
        };
    }
}
