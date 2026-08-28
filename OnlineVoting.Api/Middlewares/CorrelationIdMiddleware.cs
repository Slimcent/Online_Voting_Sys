using OnlineVoting.Models.Configurations;
using System.Diagnostics;
using System.Security.Claims;
using VotingSystem.Logger;

namespace OnlineVoting.Api.Middlewares
{
    public class CorrelationIdMiddleware
    {
        public const string CorrelationIdHeaderName = "X-Correlation-ID";
        public const string CorrelationIdItemName = RequestContextKeys.CorrelationId;

        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILoggerMessage logger)
        {
            string? correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(correlationId))
                correlationId = Guid.NewGuid().ToString();

            context.Items[CorrelationIdItemName] = correlationId;

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeaderName] = correlationId;

                return Task.CompletedTask;
            });

            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                string user = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";

                logger.LogInfo($"HTTP request completed. CorrelationId: {correlationId}, TraceId: {context.TraceIdentifier}, "
                    + $"Method: {context.Request.Method}, Path: {context.Request.Path}, "
                    + $"StatusCode: {context.Response.StatusCode}, ElapsedMilliseconds: {stopwatch.ElapsedMilliseconds}, "
                    + $"User: {user}");
            }
        }
    }
}