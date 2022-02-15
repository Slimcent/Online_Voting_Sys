using Microsoft.AspNetCore.Diagnostics;
using OnlineVoting.Models.Enums;
using OnlineVoting.Models.GlobalMessage;
using OnlineVoting.Services.Exceptions;
using System.Net;
using VotingSystem.Logger;

namespace OnlineVoting.Api.Middlewares
{
    public static class GlobalExceptionMiddleware
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILoggerMessage logger)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        var status = ResponseStatus.FATAL_ERROR;

                        switch (contextFeature.Error)
                        {
                            case InvalidDataException:
                            case InvalidOperationException:
                            case ArgumentException:
                                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                                status = ResponseStatus.APP_ERROR;
                                break;
                            case NotFoundException:
                                context.Response.StatusCode = StatusCodes.Status404NotFound;
                                status = ResponseStatus.NOT_FOUND;
                                break;
                            default:
                                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                                break;
                        }

                        logger.LogError($"Something went wrong: {contextFeature.Error.Message}");
                        await context.Response.WriteAsync(new ErrorResponse()
                        {
                            Status = status,
                            Message = contextFeature.Error.Message
                        }.ToString());

                        await context.Response.CompleteAsync();
                    }
                });
            });
        }
    }
}
