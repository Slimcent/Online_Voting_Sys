using NLog;
using OnlineVoting.Models.Configurations;
using System.Diagnostics;
using System.Security.Claims;
using VotingSystem.Logger;

namespace OnlineVoting.Api.Middlewares
{
    public class CorrelationIdMiddleware
    {
        public const string CorrelationIdHeaderName = "X-Correlation-ID";
        public const string CorrelationIdActivityTagName = "app.correlation_id";
        public const string CorrelationIdItemName = RequestContextKeys.CorrelationId;

        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILoggerMessage logger)
        {
            string? correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(correlationId)
                || correlationId.Length > 64
                || correlationId.Any(character => !(char.IsLetterOrDigit(character) || character == '-' || character == '_' || character == '.')))
            {
                correlationId = Guid.NewGuid().ToString("N");
            }

            string requestId = context.TraceIdentifier;
            string traceId = Activity.Current?.TraceId.ToString() ?? string.Empty;
            string spanId = Activity.Current?.SpanId.ToString() ?? string.Empty;

            context.Items[CorrelationIdItemName] = correlationId;
            Activity.Current?.SetTag(CorrelationIdActivityTagName, correlationId);

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeaderName] = correlationId;

                return Task.CompletedTask;
            });

            using IDisposable correlationIdScope = ScopeContext.PushProperty("CorrelationId", correlationId);
            using IDisposable requestIdScope = ScopeContext.PushProperty("RequestId", requestId);
            using IDisposable traceIdScope = ScopeContext.PushProperty("TraceId", traceId);
            using IDisposable spanIdScope = ScopeContext.PushProperty("SpanId", spanId);

            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                string user = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";

                logger.LogInfo($"HTTP request completed. Method: {context.Request.Method}, Path: {context.Request.Path}, "
                    + $"StatusCode: {context.Response.StatusCode}, ElapsedMilliseconds: {stopwatch.ElapsedMilliseconds}, "
                    + $"User: {user}");
            }
        }
    }
}