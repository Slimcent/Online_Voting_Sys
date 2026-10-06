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
            },

            [VoterDocumentationKeys.CastVote] = new ApiOperationDocumentation
            {
                Summary = "Casts a vote.",
                Description = "Casts a vote for a contestant in an election position using the registered voter credentials of the authenticated user.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["201"] = new ApiResponseDocumentation
                    {
                        Description = "The vote was cast successfully.",
                        ResponseType = typeof(string)
                    },
                    ["400"] = CommonApiResponses.BadRequest("The voting information is invalid."),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden("The voter credentials are invalid or do not belong to the authenticated user."),
                    ["404"] = CommonApiResponses.NotFound("The election position or contestant could not be found."),
                    ["409"] = CommonApiResponses.Conflict("Voting is unavailable, the voting period is invalid, or a vote has already been cast for the specified election position.")
                }
            },

            [VoterDocumentationKeys.GetMyVotes] = new ApiOperationDocumentation
            {
                Summary = "Gets the authenticated user's voting history.",
                Description = "Returns a paginated and searchable list of votes cast by the authenticated user, with optional election and election-position filters.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The voting history was retrieved successfully.",
                        ResponseType = typeof(PagedResponse<VoteHistoryResponse>)
                    },
                    ["401"] = CommonApiResponses.Unauthorized()
                }
            },

            [VoterDocumentationKeys.GetElectionResults] = new ApiOperationDocumentation
            {
                Summary = "Gets election results.",
                Description = "Returns paginated election results for completed elections, including each contestant's vote count, total votes for the election position and vote percentage.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The election results were retrieved successfully.",
                        ResponseType = typeof(PagedResponse<ElectionResultResponse>)
                    },
                    ["401"] = CommonApiResponses.Unauthorized()
                }
            },
        };
    }
}