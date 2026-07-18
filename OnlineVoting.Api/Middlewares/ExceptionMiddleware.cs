using Microsoft.AspNetCore.Diagnostics;
using OnlineVoting.Models.Enums;
using OnlineVoting.Models.GlobalMessage;
using OnlineVoting.Services.Exceptions;
using System.Security.Authentication;
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

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();

                    if (contextFeature != null)
                    {
                        context.Response.ContentType = "application/json";

                        var logger = context.RequestServices.GetRequiredService<ILoggerMessage>();

                        var status = ResponseStatus.FATAL_ERROR;
                        String message = "An unexpected error occurred.";

                        switch (contextFeature.Error)
                        {
                            case InvalidDataException:
                            case ArgumentException:
                                context.Response.StatusCode = StatusCodes.Status400BadRequest;

                                status = ResponseStatus.APP_ERROR;
                                message = contextFeature.Error.Message;

                                logger.LogWarn($"Bad request on {context.Request.Method} " +
                                    $"{context.Request.Path}: " +
                                    $"{contextFeature.Error.Message}");

                                break;

                            case InvalidCredentialsException:
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                                status = ResponseStatus.APP_ERROR;
                                message = contextFeature.Error.Message;

                                logger.LogWarn(
                                    $"Invalid login attempt on {context.Request.Method} " +
                                    $"{context.Request.Path}: " +
                                    $"{contextFeature.Error.Message}");

                                break;

                            case NotFoundException:
                                context.Response.StatusCode = StatusCodes.Status404NotFound;

                                status = ResponseStatus.NOT_FOUND;
                                message = contextFeature.Error.Message;

                                logger.LogWarn($"Resource not found on {context.Request.Method} " +
                                    $"{context.Request.Path}: " +
                                    $"{contextFeature.Error.Message}");

                                break;

                            default:
                                context.Response.StatusCode =  StatusCodes.Status500InternalServerError;

                                logger.LogError($"Unhandled exception on {context.Request.Method} " +
                                    $"{context.Request.Path}: " +
                                    $"{contextFeature.Error}");

                                break;
                        }

                        //logger.LogError($"Something went wrong: {contextFeature.Error}");

                        await context.Response.WriteAsync(
                            new ResponseError
                            {
                                Status = status,
                                Message = message
                                //StatusCode = context.Response.StatusCode,
                                //Message = contextFeature.Error.InnerException.Message
                            }.ToString());
                    }
                });
            });
        }
    }
}