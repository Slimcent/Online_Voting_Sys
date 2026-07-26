using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Documentation.Models;
using OnlineVoting.Models.Dtos.Response;

namespace OnlineVoting.Api.Documentation.Definitions.EndpointDefinitions
{
    public static class AuthDocumentation
    {
        public static readonly IReadOnlyDictionary<string, ApiOperationDocumentation> Operations = new Dictionary<string, ApiOperationDocumentation>
        {
            [AuthDocumentationKeys.Auth.Login] = new ApiOperationDocumentation
            {
                Summary = "Authenticates a user.",
                Description = "Validates the supplied credentials and returns the authenticated user together with an access token.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The user was authenticated successfully.",
                        ResponseType = typeof(LoggedInUserResponse)
                    },

                    ["400"] = CommonApiResponses.BadRequest(),

                    ["401"] = new ApiResponseDocumentation
                    {
                        Description = "The supplied credentials are invalid.",
                        ResponseType = typeof(ProblemDetails)
                    },

                    ["404"] = new ApiResponseDocumentation
                    {
                        Description = "The requested user could not be found.",
                        ResponseType = typeof(ProblemDetails)
                    },
                    ["429"] = new ApiResponseDocumentation
                    {
                        Description = "Too many login attempts. Try again later.",
                        ResponseType = typeof(ProblemDetails)
                    }
                }
            },

            [AuthDocumentationKeys.Auth.VerifyUser] = new ApiOperationDocumentation
            {
                Summary = "Verifies a user account.",
                Description = "Confirms the user's email address and sets a new password using the supplied verification and password-reset tokens.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The user account was verified successfully.",
                        ResponseType = typeof(string)
                    },

                    ["400"] = new ApiResponseDocumentation
                    {
                        Description = "The request contains validation errors or invalid verification information.",
                        ResponseType = typeof(ValidationProblemDetails)
                    },

                    ["404"] = new ApiResponseDocumentation
                    {
                        Description = "The requested user account could not be found.",
                        ResponseType = typeof(ProblemDetails)
                    }
                }
            },

            [AuthDocumentationKeys.Auth.SendResetPasswordMail] = new ApiOperationDocumentation
            {
                Summary = "Sends a password reset email.",
                Description = "Sends password reset instructions to the supplied email address.",

                Responses = CreateEmailOperationResponses("The password reset email was sent successfully.")
            },

            [AuthDocumentationKeys.Auth.ResetPassword] = new ApiOperationDocumentation
            {
                Summary = "Resets a user's password.",
                Description = "Resets the password using the supplied email address, reset token and new password.",

                Responses = CreateAuthenticationOperationResponses("The password was reset successfully.", includeNotFound: true)
            },

            [AuthDocumentationKeys.Auth.ChangePassword] = new ApiOperationDocumentation
            {
                Summary = "Changes a user's password.",
                Description = "Changes the password of the authenticated user after validating the current password.",

                Responses = CreateAuthenticationOperationResponses("The password was changed successfully.", includeNotFound: true)
            },

            [AuthDocumentationKeys.Auth.UpdateRecoveryEmail] = new ApiOperationDocumentation
            {
                Summary = "Updates a user's recovery email.",
                Description = "Updates the recovery email address associated with the specified user account.",

                Responses = new Dictionary<string, ApiResponseDocumentation>
                    {
                        ["200"] = new ApiResponseDocumentation
                            {
                                Description = "The recovery email was updated successfully.",
                                ResponseType = typeof(string)
                            },

                        ["400"] = CommonApiResponses.BadRequest("The supplied user or email information is invalid."),

                        ["401"] = CommonApiResponses.Unauthorized(),

                        ["403"] = CommonApiResponses.Forbidden(),

                        ["404"] = CommonApiResponses.NotFound("The user could not be found."),

                        ["409"] = CommonApiResponses.Conflict("The supplied email address is already in use.")
                    }
            },

            [AuthDocumentationKeys.Auth.SendChangeEmailMail] = new ApiOperationDocumentation
            {
                Summary = "Sends an email-change confirmation email.",
                Description = "Sends the confirmation information required to change a user's email address.",

                Responses = CreateEmailOperationResponses("The email-change confirmation message was sent successfully.")
            },

            [AuthDocumentationKeys.Auth.ChangeEmail] = new ApiOperationDocumentation
            {
                Summary = "Changes a user's email address.",

                Description =
                    "Changes the user's email address using the supplied " +
                    "new email address and confirmation token.",

                Responses = CreateAuthenticationOperationResponses("The email address was changed successfully.", includeNotFound: true,
                    includeConflict: true)
            }
        };


        private static IReadOnlyDictionary<string, ApiResponseDocumentation> CreateEmailOperationResponses(
        string successDescription)
        {
            return new Dictionary<string, ApiResponseDocumentation>
            {
                ["200"] = new ApiResponseDocumentation
                {
                    Description = successDescription,
                    ResponseType = typeof(string)
                },

                ["400"] = CommonApiResponses.BadRequest("The supplied email information is invalid."),

                ["401"] = CommonApiResponses.Unauthorized(),

                ["403"] = CommonApiResponses.Forbidden(),

                ["404"] = CommonApiResponses.NotFound("The user associated with the email address could not be found.")
            };
        }

        private static IReadOnlyDictionary<string, ApiResponseDocumentation> CreateAuthenticationOperationResponses(
            string successDescription, bool includeNotFound = false, bool includeConflict = false)
        {
            Dictionary<string, ApiResponseDocumentation> responses = new()
            {
                ["200"] = new ApiResponseDocumentation
                {
                    Description = successDescription,
                    ResponseType = typeof(string)
                },

                ["400"] = CommonApiResponses.BadRequest(),

                ["401"] = CommonApiResponses.Unauthorized(),

                ["403"] = CommonApiResponses.Forbidden()
            };

            if (includeNotFound)
            {
                responses["404"] = CommonApiResponses.NotFound("The user could not be found.");
            }

            if (includeConflict)
            {
                responses["409"] = CommonApiResponses.Conflict("The supplied email address is already in use.");
            }

            return responses;
        }
    }
}