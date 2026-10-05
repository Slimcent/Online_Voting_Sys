using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Documentation.Models;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Api.Documentation.Definitions.EndpointDefinitions
{
    public static class PositionApplicationDocumentation
    {
        public static readonly IReadOnlyDictionary<string, ApiOperationDocumentation> Operations = new Dictionary<string, ApiOperationDocumentation>
        {
            [PositionApplicationDocumentationKeys.CreatePositionApplication] = new ApiOperationDocumentation
            {
                Summary = "Creates a position application.",
                Description = "Creates a position application for the authenticated student and generates the corresponding invoice.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "An existing position application and invoice were retrieved successfully.",
                        ResponseType = typeof(CreatePositionApplicationResponse)
                    },
                    ["201"] = new ApiResponseDocumentation
                    {
                        Description = "The position application and invoice were created successfully.",
                        ResponseType = typeof(CreatePositionApplicationResponse)
                    },
                    ["400"] = CommonApiResponses.BadRequest(),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound(),
                    ["409"] = CommonApiResponses.Conflict()
                }
            },

            [PositionApplicationDocumentationKeys.CancelPositionApplication] = new ApiOperationDocumentation
            {
                Summary = "Cancels a position application.",
                Description = "Cancels the specified position application belonging to the current student when the application is awaiting payment and its invoice is unpaid.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The position application was cancelled successfully.",
                        ResponseType = typeof(string)
                    },
                    ["400"] = CommonApiResponses.BadRequest("The current user could not be identified."),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound("The specified position application or its required cancellation data could not be found."),
                    ["409"] = CommonApiResponses.Conflict("The position application or invoice is not in a state that allows cancellation.")
                }
            },

            [PositionApplicationDocumentationKeys.GetMyPositionApplications] = new ApiOperationDocumentation
            {
                Summary = "Gets the current student's position applications.",
                Description = "Returns a paginated and searchable list of position applications belonging to the current student.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The position applications were retrieved successfully.",
                        ResponseType = typeof(PagedResponse<PositionApplicationResponse>)
                    },
                    ["400"] = CommonApiResponses.BadRequest("The current user could not be identified."),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden()
                }
            },

            [PositionApplicationDocumentationKeys.GetMyPositionApplication] = new ApiOperationDocumentation
            {
                Summary = "Gets one position application for the current student.",
                Description = "Returns the specified position application when it belongs to the current student.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The position application was retrieved successfully.",
                        ResponseType = typeof(PositionApplicationResponse)
                    },
                    ["400"] = CommonApiResponses.BadRequest("The current user could not be identified."),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound("The specified position application could not be found.")
                }
            },

            [PositionApplicationDocumentationKeys.GetPositionApplication] = new ApiOperationDocumentation
            {
                Summary = "Gets a position application.",
                Description = "Returns the specified position application. Students can only retrieve their own applications, while administrators and super administrators can retrieve any position application.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The position application was retrieved successfully.",
                        ResponseType = typeof(PositionApplicationResponse)
                    },
                    ["400"] = CommonApiResponses.BadRequest("The current user could not be identified."),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound("The specified position application could not be found.")
                }
            },

            [PositionApplicationDocumentationKeys.GetPositionApplications] = new ApiOperationDocumentation
            {
                Summary = "Gets position applications.",
                Description = "Returns a paginated and searchable list of position applications.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The position applications were retrieved successfully.",
                        ResponseType = typeof(PagedResponse<PositionApplicationResponse>)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden()
                }
            },

            [PositionApplicationDocumentationKeys.ApproveOrRejectPositionApplication] = new ApiOperationDocumentation
            {
                Summary = "Approves or rejects a position application.",
                Description = "Approves or rejects a position application that is pending review and has a paid invoice. Approving the application also creates a contestant.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The position application was approved or rejected successfully.",
                        ResponseType = typeof(string)
                    },
                    ["400"] = CommonApiResponses.BadRequest(),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound(),
                    ["409"] = CommonApiResponses.Conflict()
                }
            },

            [PositionApplicationDocumentationKeys.GetPositionApplicationsWithContestants] = new ApiOperationDocumentation
            {
                Summary = "Gets position applications with contestants.",
                Description = "Returns a paginated and searchable list of position applications that have contestants, including contestant details.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The position applications with contestants were retrieved successfully.",
                        ResponseType = typeof(PagedResponse<PositionApplicationResponse>)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden()
                }
            },
        };
    }
}