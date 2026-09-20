namespace OnlineVoting.Api.Configurations
{
    public sealed class SecurityHeadersOptions
    {
        public const string SectionName = "SecurityHeaders";

        public bool HttpsRedirectionEnabled { get; set; }

        public HstsSecurityOptions Hsts { get; set; } = new();
    }

    public sealed class HstsSecurityOptions
    {
        public bool Enabled { get; set; }

        public int MaxAgeDays { get; set; } = 30;

        public bool IncludeSubDomains { get; set; }

        public bool Preload { get; set; }
    }
}