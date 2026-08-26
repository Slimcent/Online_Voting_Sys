using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Api.Extensions;
using OnlineVoting.Api.Middlewares;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Tests.UnitTests.Api.Extensions
{
    public class ResultActionResultExtensionsTests
    {
        private static ControllerBase CreateController()
        {
            DefaultHttpContext httpContext = new();
            httpContext.Request.Path = "/api/v1/test";
            httpContext.TraceIdentifier = "trace-123";
            httpContext.Items[CorrelationIdMiddleware.CorrelationIdItemName] = "correlation-123";

            ControllerContext controllerContext = new()
            {
                HttpContext = httpContext
            };

            TestController controller = new()
            {
                ControllerContext = controllerContext
            };

            return controller;
        }

        [Fact]
        public void ToActionResult_WithSuccess_ShouldReturnOk()
        {
            ControllerBase controller = CreateController();
            Result<string> result = Result<string>.Success("Success");

            IActionResult actionResult = result.ToActionResult(controller);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult);

            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public void ToActionResult_WithCreated_ShouldReturnCreated()
        {
            ControllerBase controller = CreateController();
            Result<string> result = Result<string>.Created("Created");

            IActionResult actionResult = result.ToActionResult(controller);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(actionResult);

            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
            Assert.Equal("Created", objectResult.Value);
        }

        [Fact]
        public void ToActionResult_WithNoContent_ShouldReturnNoContent()
        {
            ControllerBase controller = CreateController();
            Result<string> result = Result<string>.NoContent();

            IActionResult actionResult = result.ToActionResult(controller);

            NoContentResult noContentResult = Assert.IsType<NoContentResult>(actionResult);

            Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
        }

        [Fact]
        public void ToActionResult_WithValidationError_ShouldReturnBadRequestProblemDetails()
        {
            ControllerBase controller = CreateController();
            Result<string> result = Result<string>.ValidationError("Invalid data");

            IActionResult actionResult = result.ToActionResult(controller);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(actionResult);
            ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
            Assert.Equal("Validation error", problemDetails.Title);
            Assert.Equal("Invalid data", problemDetails.Detail);
            Assert.Equal("/api/v1/test", problemDetails.Instance);
            Assert.Equal("trace-123", problemDetails.Extensions["traceId"]);
            Assert.Equal("correlation-123", problemDetails.Extensions["correlationId"]);
        }

        [Fact]
        public void ToActionResult_WithNotFound_ShouldReturnNotFoundProblemDetails()
        {
            ControllerBase controller = CreateController();
            Result<string> result = Result<string>.NotFound("Resource missing");

            IActionResult actionResult = result.ToActionResult(controller);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(actionResult);
            ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
            Assert.Equal("Resource not found", problemDetails.Title);
            Assert.Equal("Resource missing", problemDetails.Detail);
        }

        [Fact]
        public void ToActionResult_WithConflict_ShouldReturnConflictProblemDetails()
        {
            ControllerBase controller = CreateController();
            Result<string> result = Result<string>.Conflict("Resource already exists");

            IActionResult actionResult = result.ToActionResult(controller);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(actionResult);
            ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

            Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);
            Assert.Equal("Conflict", problemDetails.Title);
            Assert.Equal("Resource already exists", problemDetails.Detail);
        }

        [Fact]
        public void ToActionResult_WithUnauthorized_ShouldReturnUnauthorizedProblemDetails()
        {
            ControllerBase controller = CreateController();
            Result<string> result = Result<string>.Unauthorized("Authentication required");

            IActionResult actionResult = result.ToActionResult(controller);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(actionResult);
            ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

            Assert.Equal(StatusCodes.Status401Unauthorized, objectResult.StatusCode);
            Assert.Equal("Unauthorized", problemDetails.Title);
            Assert.Equal("Authentication required", problemDetails.Detail);
        }

        [Fact]
        public void ToActionResult_WithForbidden_ShouldReturnForbiddenProblemDetails()
        {
            ControllerBase controller = CreateController();
            Result<string> result = Result<string>.Forbidden("Access denied");

            IActionResult actionResult = result.ToActionResult(controller);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(actionResult);
            ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

            Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
            Assert.Equal("Forbidden", problemDetails.Title);
            Assert.Equal("Access denied", problemDetails.Detail);
        }

        private class TestController : ControllerBase
        {
        }
    }
}