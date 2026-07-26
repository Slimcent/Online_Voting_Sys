using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

                ResultStatus.ValidationError => controller.Problem(statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation error", detail: result.Error),

                ResultStatus.NotFound => controller.Problem(statusCode: StatusCodes.Status404NotFound,
                    title: "Resource not found", detail: result.Error),

                ResultStatus.Conflict => controller.Problem(statusCode: StatusCodes.Status409Conflict,
                    title: "Conflict", detail: result.Error),

                ResultStatus.Unauthorized => controller.Problem(statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized", detail: result.Error),

                ResultStatus.Forbidden => controller.Problem(statusCode: StatusCodes.Status403Forbidden,
                    title: "Forbidden", detail: result.Error),

                _ =>

                    throw new InvalidOperationException($"Unsupported result status: {result.Status}")
            };
        }
    }
}