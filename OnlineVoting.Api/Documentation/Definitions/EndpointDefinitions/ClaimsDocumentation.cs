using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Documentation.Models;

namespace OnlineVoting.Api.Documentation.Definitions.EndpointDefinitions
{
    public static class ClaimsDocumentation
    {
        public static readonly IReadOnlyDictionary<string, ApiOperationDocumentation> Operations = new Dictionary<string, ApiOperationDocumentation>
        {
            [ClaimsDocumentationKeys.AddUserToClaims] = new ApiOperationDocumentation
            {
                Summary = "Adds a claim to a user.",
                Description = "Adds the supplied claim type and value to the user identified by the email address.",

                Responses = CreateClaimOperationResponses("The claim was added successfully.", includeConflict: true)
            },

            [ClaimsDocumentationKeys.DeleteClaim] = new ApiOperationDocumentation
            {
                Summary = "Deletes a user claim.",
                Description = "Deletes the specified claim from the user identified by the email address.",

                Responses = CreateClaimOperationResponses("The claim was deleted successfully.", includeNotFound: true)
            },

            [ClaimsDocumentationKeys.EditClaim] = new ApiOperationDocumentation
            {
                Summary = "Updates a user claim.",
                Description = "Replaces an existing user claim value with the supplied new value.",

                Responses = CreateClaimOperationResponses("The claim was updated successfully.", includeNotFound: true)
            },

            [ClaimsDocumentationKeys.GetUserClaims] = new ApiOperationDocumentation
            {
                Summary = "Gets a user's claims.",
                Description = "Returns all claims belonging to the user identified by the supplied email address.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The user's claims were returned successfully."
                    },

                    ["400"] = CommonApiResponses.BadRequest("No claims were found for the user."),

                    ["401"] = CommonApiResponses.Unauthorized(),

                    ["403"] = CommonApiResponses.Forbidden(),

                    ["404"] = CommonApiResponses.NotFound("The user could not be found.")
                }
            }
        };

        private static IReadOnlyDictionary<string, ApiResponseDocumentation> CreateClaimOperationResponses(string successDescription,
            bool includeNotFound = false, bool includeConflict = false)
        {
            Dictionary<string, ApiResponseDocumentation> responses = new()
            {
                ["201"] = new ApiResponseDocumentation
                {
                    Description = successDescription
                },

                ["400"] = CommonApiResponses.BadRequest("The supplied claim information is invalid."),

                ["401"] = CommonApiResponses.Unauthorized(),

                ["403"] = CommonApiResponses.Forbidden()
            };

            if (includeNotFound)
            {
                responses["404"] = CommonApiResponses.NotFound("The requested user or claim could not be found.");
            }

            if (includeConflict)
            {
                responses["409"] = CommonApiResponses.Conflict("The user already has the specified claim.");
            }

            return responses;
        }
    }
}