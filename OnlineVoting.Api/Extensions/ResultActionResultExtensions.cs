using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Api.Middlewares;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Api.Extensions
{
    public static class ResultActionResultExtensions
    {
        public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
        {
            return result.Status switch
            {
                ResultStatus.Success => controller.Ok(result.Value),

                ResultStatus.Created => controller.StatusCode(StatusCodes.Status201Created, result.Value),

                ResultStatus.NoContent => controller.NoContent(),

                ResultStatus.ValidationError => CreateProblemDetails(controller,
                    StatusCodes.Status400BadRequest, "Validation error", result.Error),

                ResultStatus.NotFound => CreateProblemDetails(controller,
                    StatusCodes.Status404NotFound, "Resource not found", result.Error),

                ResultStatus.Conflict => CreateProblemDetails(controller,
                    StatusCodes.Status409Conflict, "Conflict", result.Error),

                ResultStatus.Unauthorized => CreateProblemDetails(controller,
                    StatusCodes.Status401Unauthorized, "Unauthorized", result.Error),

                ResultStatus.Forbidden => CreateProblemDetails(controller,
                    StatusCodes.Status403Forbidden, "Forbidden", result.Error),

                _ => throw new InvalidOperationException($"Unsupported result status: {result.Status}")
            };
        }

        private static IActionResult CreateProblemDetails(ControllerBase controller, int statusCode, string title, string? detail)
        {
            HttpContext context = controller.HttpContext;

            string correlationId = context.Items[CorrelationIdMiddleware.CorrelationIdItemName]?.ToString() ?? "Unavailable";

            ProblemDetails problemDetails = new()
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            problemDetails.Extensions["traceId"] = context.TraceIdentifier;
            problemDetails.Extensions["correlationId"] = correlationId;

            return controller.StatusCode(statusCode, problemDetails);
        }
    }
}