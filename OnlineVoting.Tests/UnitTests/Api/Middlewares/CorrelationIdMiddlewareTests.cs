using Microsoft.AspNetCore.Http;
using NLog;
using OnlineVoting.Api.Middlewares;
using System.Diagnostics;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.UnitTests.Api.Middlewares
{
    public class CorrelationIdMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_WhenCorrelationIdIsMissing_ShouldGenerateCorrelationId()
        {
            DefaultHttpContext context = new();
            TestLoggerMessage logger = new();

            CorrelationIdMiddleware middleware = new(async httpContext =>
            {
                await Task.CompletedTask;
            });

            await middleware.InvokeAsync(context, logger);

            string correlationId = Assert.IsType<string>(
                context.Items[CorrelationIdMiddleware.CorrelationIdItemName]);

            Assert.False(string.IsNullOrWhiteSpace(correlationId));
            Assert.Equal(32, correlationId.Length);
        }

        [Fact]
        public async Task InvokeAsync_WhenCorrelationIdIsValid_ShouldPreserveCorrelationId()
        {
            DefaultHttpContext context = new();
            TestLoggerMessage logger = new();

            const string correlationId = "client-request-123";

            context.Request.Headers[CorrelationIdMiddleware.CorrelationIdHeaderName] = correlationId;

            CorrelationIdMiddleware middleware = new(async httpContext =>
            {
                await Task.CompletedTask;
            });

            await middleware.InvokeAsync(context, logger);

            Assert.Equal(correlationId, context.Items[CorrelationIdMiddleware.CorrelationIdItemName]);
        }

        [Fact]
        public async Task InvokeAsync_WhenCorrelationIdContainsInvalidCharacters_ShouldReplaceCorrelationId()
        {
            DefaultHttpContext context = new();
            TestLoggerMessage logger = new();

            const string invalidCorrelationId = "request\r\ninvalid";

            context.Request.Headers[CorrelationIdMiddleware.CorrelationIdHeaderName] = invalidCorrelationId;

            CorrelationIdMiddleware middleware = new(async httpContext =>
            {
                await httpContext.Response.StartAsync();
            });

            await middleware.InvokeAsync(context, logger);

            string correlationId = Assert.IsType<string>(context.Items[CorrelationIdMiddleware.CorrelationIdItemName]);

            Assert.NotEqual(invalidCorrelationId, correlationId);
            Assert.Equal(32, correlationId.Length);
        }

        [Fact]
        public async Task InvokeAsync_WhenCorrelationIdIsLongerThanMaximumLength_ShouldReplaceCorrelationId()
        {
            DefaultHttpContext context = new();
            TestLoggerMessage logger = new();

            string invalidCorrelationId = new('a', 65);

            context.Request.Headers[CorrelationIdMiddleware.CorrelationIdHeaderName] = invalidCorrelationId;

            CorrelationIdMiddleware middleware = new(async httpContext =>
            {
                await httpContext.Response.StartAsync();
            });

            await middleware.InvokeAsync(context, logger);

            string correlationId = Assert.IsType<string>(context.Items[CorrelationIdMiddleware.CorrelationIdItemName]);

            Assert.NotEqual(invalidCorrelationId, correlationId);
            Assert.Equal(32, correlationId.Length);
        }

        [Fact]
        public async Task InvokeAsync_ShouldExposeRequestIdentifiersToNLogScope()
        {
            DefaultHttpContext context = new();
            TestLoggerMessage logger = new();

            context.TraceIdentifier = "request-123";

            using Activity activity = new("test-request");
            activity.SetIdFormat(ActivityIdFormat.W3C);
            activity.Start();

            CorrelationIdMiddleware middleware = new(async httpContext =>
            {
                await Task.CompletedTask;
            });

            await middleware.InvokeAsync(context, logger);

            Assert.False(string.IsNullOrWhiteSpace(logger.CorrelationId));
            Assert.Equal("request-123", logger.RequestId);
            Assert.Equal(activity.TraceId.ToString(), logger.TraceId);
            Assert.Equal(activity.SpanId.ToString(), logger.SpanId);
            Assert.Equal(logger.CorrelationId, activity.GetTagItem(CorrelationIdMiddleware.CorrelationIdActivityTagName)?.ToString());
        }

        [Fact]
        public async Task InvokeAsync_ShouldLogCompletedRequest()
        {
            DefaultHttpContext context = new();
            TestLoggerMessage logger = new();

            context.Request.Method = HttpMethods.Get;
            context.Request.Path = "/api/v1/test";
            context.Response.StatusCode = StatusCodes.Status200OK;

            CorrelationIdMiddleware middleware = new(async httpContext =>
            {
                await Task.CompletedTask;
            });

            await middleware.InvokeAsync(context, logger);

            Assert.Contains("HTTP request completed.", logger.LastInfoMessage);
            Assert.Contains("Method: GET", logger.LastInfoMessage);
            Assert.Contains("Path: /api/v1/test", logger.LastInfoMessage);
            Assert.Contains("StatusCode: 200", logger.LastInfoMessage);
            Assert.DoesNotContain("TraceId:", logger.LastInfoMessage);
        }

        private sealed class TestLoggerMessage : ILoggerMessage
        {
            public string LastInfoMessage { get; private set; } = string.Empty;
            public string CorrelationId { get; private set; } = string.Empty;
            public string RequestId { get; private set; } = string.Empty;
            public string TraceId { get; private set; } = string.Empty;
            public string SpanId { get; private set; } = string.Empty;

            public void LogDebug(string message)
            {
            }

            public void LogError(string message)
            {
            }

            public void LogInfo(string message)
            {
                LastInfoMessage = message;

                CorrelationId = GetScopeValue("CorrelationId");
                RequestId = GetScopeValue("RequestId");
                TraceId = GetScopeValue("TraceId");
                SpanId = GetScopeValue("SpanId");
            }

            public void LogWarn(string message)
            {
            }

            public void LogError(Exception exception, string message)
            {
            }

            private static string GetScopeValue(string propertyName)
            {
                return ScopeContext.TryGetProperty(propertyName, out object? value)
                    ? value?.ToString() ?? string.Empty
                    : string.Empty;
            }
        }
    }
}