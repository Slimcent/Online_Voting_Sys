using Microsoft.AspNetCore.Mvc;

namespace OnlineVoting.Api.Documentation.Models
{
    public static class CommonApiResponses
    {
        public static ApiResponseDocumentation BadRequest(string description = "The request contains validation errors.")
        {
            return new ApiResponseDocumentation
            {
                Description = description,
                ResponseType = typeof(ValidationProblemDetails)
            };
        }

        public static ApiResponseDocumentation Unauthorized(string description = "Authentication is required or the supplied credentials are invalid.")
        {
            return new ApiResponseDocumentation
            {
                Description = description,
                ResponseType = typeof(ProblemDetails)
            };
        }

        public static ApiResponseDocumentation Forbidden(string description = "The authenticated user does not have permission to perform this operation.")
        {
            return new ApiResponseDocumentation
            {
                Description = description,
                ResponseType = typeof(ProblemDetails)
            };
        }

        public static ApiResponseDocumentation NotFound(string description = "The requested resource could not be found.")
        {
            return new ApiResponseDocumentation
            {
                Description = description,
                ResponseType = typeof(ProblemDetails)
            };
        }

        public static ApiResponseDocumentation Conflict(string description = "The request conflicts with the current state of the resource.")
        {
            return new ApiResponseDocumentation
            {
                Description = description,
                ResponseType = typeof(ProblemDetails)
            };
        }
    }
}