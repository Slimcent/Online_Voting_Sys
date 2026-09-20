namespace OnlineVoting.Api.Configurations
{
    public sealed class CorsSettings
    {
        public const string SectionName = "Cors";

        public bool Enabled { get; set; }

        public string[] AllowedOrigins { get; set; } = [];

        public string[] AllowedMethods { get; set; } =
        [
            "GET",
            "POST",
            "PUT",
            "PATCH",
            "DELETE"
        ];

        public string[] AllowedHeaders { get; set; } =
        [
            "Accept",
            "Authorization",
            "Content-Type",
            "X-Correlation-ID",
            "X-Device-Latitude",
            "X-Device-Longitude",
            "X-Device-Accuracy",
            "X-Device-Location-Captured-At"
        ];

        public int PreflightMaxAgeMinutes { get; set; } = 10;
    }
}