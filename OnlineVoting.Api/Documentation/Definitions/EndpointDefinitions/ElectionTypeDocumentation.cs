using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Documentation.Models;
using OnlineVoting.Models.Dtos.Response;

namespace OnlineVoting.Api.Documentation.Definitions.EndpointDefinitions
{
    public static class ElectionTypeDocumentation
    {
        public static readonly IReadOnlyDictionary<string, ApiOperationDocumentation> Operations = new Dictionary<string, ApiOperationDocumentation>
        {
            [ElectionTypeDocumentationKeys.CreateElectionType] = new ApiOperationDocumentation
            {
                Summary = "Creates an election type.",
                Description = "Creates a new election type using the supplied information.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["201"] = new ApiResponseDocumentation
                    {
                        Description = "The election type was created successfully.",
                        ResponseType = typeof(string)
                    },
                    ["400"] = CommonApiResponses.BadRequest("The election type information is invalid."),
                    ["400"] = CommonApiResponses.BadRequest("The election type information is invalid."),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["409"] = CommonApiResponses.Conflict("An election type with the supplied name already exists.")
                }
            },

            [ElectionTypeDocumentationKeys.GetElectionTypes] = new ApiOperationDocumentation
            {
                Summary = "Gets election types.",
                Description = "Returns all election types.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election types were retrieved successfully.",
                        ResponseType = typeof(IEnumerable<ElectionTypeResponse>)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden()
                }
            },

            [ElectionTypeDocumentationKeys.GetPagedElectionTypes] = new ApiOperationDocumentation
            {
                Summary = "Gets paged list of election types.",
                Description = "Returns a paged list of election types.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election types were retrieved successfully.",
                        ResponseType = typeof(IEnumerable<ElectionTypeResponse>)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden()
                }
            },

            [ElectionTypeDocumentationKeys.GetElectionType] = new ApiOperationDocumentation
            {
                Summary = "Gets an election type.",
                Description = "Returns the election type with the specified id.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election type was retrieved successfully.",
                        ResponseType = typeof(ElectionTypeResponse)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound("The specified election type could not be found.")
                }
            },

            [ElectionTypeDocumentationKeys.UpdateElectionType] = new ApiOperationDocumentation
            {
                Summary = "Updates an election type.",
                Description = "Updates the election type with the specified id using the supplied information.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election type was updated successfully.",
                        ResponseType = typeof(string)
                    },
                    ["400"] = CommonApiResponses.BadRequest("The ID is invalid."),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound("The specified election type could not be found."),
                    ["409"] = CommonApiResponses.Conflict("An election type with the supplied name already exists.")
                }
            },

            [ElectionTypeDocumentationKeys.ToggleElectionTypeActivation] = new ApiOperationDocumentation
            {
                Summary = "Toggles election type activation.",
                Description = "Toggles the active status of the election type with the specified id.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election type activation status was updated successfully.",
                        ResponseType = typeof(string)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound("The specified election type could not be found.")
                }
            },

            [ElectionTypeDocumentationKeys.DeleteElectionType] = new ApiOperationDocumentation
            {
                Summary = "Deletes an election type.",
                Description = "Deletes the election type with the specified id.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election type was deleted successfully.",
                        ResponseType = typeof(string)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound("The specified election type could not be found."),
                    ["409"] = CommonApiResponses.Conflict("The election type cannot be deleted because it is already used by an election.")
                }
            }
        };
    }
}
