using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Services.Exceptions;
using VotingSystem.Logger;

namespace OnlineVoting.Api.Middlewares
{
    public static class ExceptionMiddleware
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    //context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    IExceptionHandlerFeature? contextFeature =  context.Features.Get<IExceptionHandlerFeature>();

                    if (contextFeature != null)
                    {
                        ILoggerMessage logger = context.RequestServices.GetRequiredService<ILoggerMessage>();

                        int statusCode;
                        string title;
                        string detail;
                        string type;

                        switch (contextFeature.Error)
                        {
                            case InvalidDataException:
                            case ArgumentException:
                                statusCode = StatusCodes.Status400BadRequest;

                                title = "Bad request";
                                detail = contextFeature.Error.Message;
                                type = "https://tools.ietf.org/html/" + "rfc9110#section-15.5.1";

                                logger.LogWarn($"Bad request on {context.Request.Method} {context.Request.Path}:" 
                                    + $"{contextFeature.Error.Message}");

                                break;

                            case InvalidCredentialsException:
                                statusCode = StatusCodes.Status401Unauthorized;

                                title = "Authentication failed";
                                detail = contextFeature.Error.Message;
                                type = "https://tools.ietf.org/html/" + "rfc9110#section-15.5.2";

                                logger.LogWarn($"Invalid login attempt on {context.Request.Method} {context.Request.Path}:" 
                                    + $"{contextFeature.Error.Message}");

                                break;

                            case NotFoundException:
                                statusCode = StatusCodes.Status404NotFound;

                                title = "Resource not found";
                                detail = contextFeature.Error.Message;
                                type = "https://tools.ietf.org/html/" + "rfc9110#section-15.5.5";

                                logger.LogWarn($"Resource not found on {context.Request.Method} {context.Request.Path}:" 
                                    + $"{contextFeature.Error.Message}");

                                break;

                            case ConflictException:
                                statusCode = StatusCodes.Status409Conflict;

                                title = "Conflict";
                                detail = contextFeature.Error.Message;
                                type = "https://tools.ietf.org/html/" + "rfc9110#section-15.5.10";

                                logger.LogWarn($"Conflict while processing {context.Request.Method} {context.Request.Path}:" 
                                    + $"{contextFeature.Error.Message}");

                                break;

                            default:
                                statusCode = StatusCodes.Status500InternalServerError;

                                title = "Internal server error";
                                detail = "An unexpected error occurred.";
                                type = "https://tools.ietf.org/html/" + "rfc9110#section-15.6.1";

                                logger.LogError(contextFeature.Error, $"An unexpected error occurred while " 
                                    + $"processing {context.Request.Method} {context.Request.Path}.");

                                break;
                        }

                        context.Response.StatusCode = statusCode;
                        context.Response.ContentType = "application/problem+json";

                        ProblemDetails problemDetails = new()
                        {
                            Status = statusCode,
                            Title = title,
                            Detail = detail,
                            Type = type,
                            Instance = context.Request.Path
                        };

                        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

                        //logger.LogError($"Something went wrong: {contextFeature.Error}");

                        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken: context.RequestAborted);
                    }
                });
            });
        }

        public static void ConfigureStatusCodePages(this IApplicationBuilder app)
        {
            app.UseStatusCodePages(async statusCodeContext =>
            {
                HttpContext context = statusCodeContext.HttpContext;

                int statusCode = context.Response.StatusCode;

                string title;
                string detail;
                string type;

                switch (statusCode)
                {
                    case StatusCodes.Status401Unauthorized:
                        title = "Unauthorized";
                        detail = "Authentication is required to access this resource.";
                        type = "https://tools.ietf.org/html/" + "rfc9110#section-15.5.2";

                        break;

                    case StatusCodes.Status403Forbidden:
                        title = "Forbidden";
                        detail = "You do not have permission to access this resource.";
                        type = "https://tools.ietf.org/html/" + "rfc9110#section-15.5.4";

                        break;

                    case StatusCodes.Status404NotFound:
                        title = "Resource not found";
                        detail = "The requested endpoint was not found.";
                        type = "https://tools.ietf.org/html/" + "rfc9110#section-15.5.5";

                        break;

                    default:
                        return;
                }

                context.Response.ContentType = "application/problem+json";

                ProblemDetails problemDetails = new()
                {
                    Status = statusCode,
                    Title = title,
                    Detail = detail,
                    Type = type,
                    Instance = context.Request.Path
                };

                problemDetails.Extensions["traceId"] = context.TraceIdentifier;

                await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken: context.RequestAborted);
            });
        }
    }
}