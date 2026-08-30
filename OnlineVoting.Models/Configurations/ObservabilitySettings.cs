namespace OnlineVoting.Models.Configurations
{
    public class ObservabilitySettings
    {
        public const string SectionName = "Observability";

        public bool Enabled { get; set; }

        public string ServiceName { get; set; } = "OnlineVoting.Api";

        public string ServiceNamespace { get; set; } = "OnlineVoting";

        public double TraceSamplingRatio { get; set; } = 1.0;

        public string OtlpEndpoint { get; set; } = "http://localhost:4318/";

        public string[] ExcludedTracingPaths { get; set; } =
        [
            "/health",
            "/swagger"
        ];
    }
}
