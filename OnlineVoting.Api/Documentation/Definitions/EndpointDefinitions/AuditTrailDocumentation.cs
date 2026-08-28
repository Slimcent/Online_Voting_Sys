using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Documentation.Models;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Api.Documentation.Definitions.EndpointDefinitions
{
    public static class AuditTrailDocumentation
    {
        public static readonly IReadOnlyDictionary<string, ApiOperationDocumentation> Operations = new Dictionary<string, ApiOperationDocumentation>
        {
            [AuditTrailDocumentationKeys.GetAuditTrails] = new ApiOperationDocumentation
            {
                Summary = "Gets audit trail records.",

                Description = "Returns a paginated list of audit trail records with optional filtering by actor, endpoint, event, entity, outcome, correlation id, IP address, and date range.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The audit trail records were retrieved successfully.",

                        ResponseType = typeof(PagedResponse<AuditTrailResponse>)
                    },

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden()
                }
            }
        };
    }
}