using Microsoft.Extensions.Options;
using OnlineVoting.Api.Configurations;
using OnlineVoting.Models.Configurations;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

namespace OnlineVoting.Api.Extensions
{
    public static class ObservabilityExtensions
    {
        public static IServiceCollection ConfigureObservability(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            IConfigurationSection section = configuration.GetSection(ObservabilitySettings.SectionName);

            ObservabilitySettingsValidator validator = new();

            services.AddSingleton<IValidateOptions<ObservabilitySettings>>(validator);

            services.AddOptions<ObservabilitySettings>().Bind(section).ValidateOnStart();

            ObservabilitySettings observabilitySettings = section.Get<ObservabilitySettings>() ?? new ObservabilitySettings();

            ValidateOptionsResult validationResult = validator.Validate(Options.DefaultName, observabilitySettings);

            if (validationResult.Failed)
            {
                throw new OptionsValidationException(Options.DefaultName, typeof(ObservabilitySettings), validationResult.Failures);
            }

            if (!observabilitySettings.Enabled)
                return services;

            string serviceVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown";

            Uri otlpEndpoint = new(observabilitySettings.OtlpEndpoint, UriKind.Absolute);
            Uri traceEndpoint = new(otlpEndpoint, "v1/traces");
            Uri metricsEndpoint = new(otlpEndpoint, "v1/metrics");

            OpenTelemetryBuilder openTelemetryBuilder = services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService(serviceName: observabilitySettings.ServiceName,
                        serviceNamespace: observabilitySettings.ServiceNamespace,
                        serviceVersion: serviceVersion)
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["deployment.environment.name"] = environment.EnvironmentName
                    }));

            openTelemetryBuilder.WithTracing(tracing => tracing
                .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(observabilitySettings.TraceSamplingRatio)))
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = context => !observabilitySettings.ExcludedTracingPaths.Any(path =>
                            context.Request.Path.StartsWithSegments(path));
                })
                .AddOtlpExporter(options =>
                {
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                    options.Endpoint = traceEndpoint;
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                    options.Endpoint = metricsEndpoint;
                }));

            return services;
        }
    }
}