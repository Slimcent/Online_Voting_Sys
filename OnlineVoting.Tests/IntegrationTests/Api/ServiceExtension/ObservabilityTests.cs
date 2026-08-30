using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OnlineVoting.Api.Extensions;

namespace OnlineVoting.Tests.IntegrationTests.Api.ServiceExtension
{
    public class ObservabilityTests
    {
        [Fact]
        public async Task Configuration_WhenObservabilityIsDisabled_ShouldStartSuccessfully()
        {
            using IHost host = await CreateHost(enabled: false);

            Assert.NotNull(host);
        }

        [Fact]
        public async Task Configuration_WhenEnabledWithValidSettings_ShouldStartSuccessfully()
        {
            using IHost host = await CreateHost();

            Assert.NotNull(host);
        }

        [Fact]
        public async Task Configuration_WhenServiceNameIsMissing_ShouldFailAtStartup()
        {
            await Assert.ThrowsAsync<OptionsValidationException>(() =>
                CreateHost(serviceName: ""));
        }

        [Fact]
        public async Task Configuration_WhenServiceNamespaceIsMissing_ShouldFailAtStartup()
        {
            await Assert.ThrowsAsync<OptionsValidationException>(() =>
                CreateHost(serviceNamespace: ""));
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(1.1)]
        public async Task Configuration_WhenTraceSamplingRatioIsInvalid_ShouldFailAtStartup(double traceSamplingRatio)
        {
            await Assert.ThrowsAsync<OptionsValidationException>(() =>
                CreateHost(traceSamplingRatio: traceSamplingRatio));
        }

        [Theory]
        [InlineData("invalid-endpoint")]
        [InlineData("ftp://collector.example.com")]
        public async Task Configuration_WhenOtlpEndpointIsInvalid_ShouldFailAtStartup(string otlpEndpoint)
        {
            await Assert.ThrowsAsync<OptionsValidationException>(() =>
                CreateHost(otlpEndpoint: otlpEndpoint));
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(0.5)]
        [InlineData(1.0)]
        public async Task Configuration_WhenTraceSamplingRatioIsValid_ShouldStartSuccessfully(double traceSamplingRatio)
        {
            using IHost host = await CreateHost(traceSamplingRatio: traceSamplingRatio);

            Assert.NotNull(host);
        }

        private static async Task<IHost> CreateHost(
            bool enabled = true,
            string serviceName = "OnlineVoting.Api",
            string serviceNamespace = "OnlineVoting",
            double traceSamplingRatio = 1.0,
            string otlpEndpoint = "http://localhost:4318/")
        {
            Dictionary<string, string?> configurationValues = new()
            {
                ["Observability:Enabled"] = enabled.ToString(),
                ["Observability:ServiceName"] = serviceName,
                ["Observability:ServiceNamespace"] = serviceNamespace,
                ["Observability:TraceSamplingRatio"] = traceSamplingRatio.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["Observability:OtlpEndpoint"] = otlpEndpoint,
                ["Observability:ExcludedTracingPaths:0"] = "/health",
                ["Observability:ExcludedTracingPaths:1"] = "/swagger"
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationValues)
                .Build();

            IHost host = await new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.ConfigureObservability(
                        configuration,
                        new TestHostEnvironment());
                })
                .StartAsync();

            return host;
        }

        private sealed class TestHostEnvironment : IHostEnvironment
        {
            public string EnvironmentName { get; set; } = Environments.Development;

            public string ApplicationName { get; set; } = "OnlineVoting.Api";

            public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();

            public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
                new Microsoft.Extensions.FileProviders.NullFileProvider();
        }
    }
}
