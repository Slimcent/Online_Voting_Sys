using Microsoft.Extensions.Options;
using OnlineVoting.Models.Configurations;

namespace OnlineVoting.Api.Configurations
{
    public sealed class ObservabilitySettingsValidator : IValidateOptions<ObservabilitySettings>
    {
        public ValidateOptionsResult Validate(string? name, ObservabilitySettings options)
        {
            List<string> failures = [];

            if (options.Enabled && string.IsNullOrWhiteSpace(options.ServiceName))
            {
                failures.Add("Observability:ServiceName is required when observability is enabled.");
            }

            if (options.Enabled && string.IsNullOrWhiteSpace(options.ServiceNamespace))
            {
                failures.Add("Observability:ServiceNamespace is required when observability is enabled.");
            }

            if (options.TraceSamplingRatio < 0 || options.TraceSamplingRatio > 1)
            {
                failures.Add("Observability:TraceSamplingRatio must be between 0 and 1.");
            }

            if (options.Enabled && (!Uri.TryCreate(options.OtlpEndpoint, UriKind.Absolute, out Uri? endpoint)
                || (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps)))
            {
                failures.Add("Observability:OtlpEndpoint must be a valid HTTP or HTTPS URL when observability is enabled.");
            }

            return failures.Count == 0
                ? ValidateOptionsResult.Success
                : ValidateOptionsResult.Fail(failures);
        }
    }
}