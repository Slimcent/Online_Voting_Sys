namespace OnlineVoting.Caching.Configuration
{
    public sealed class CacheOptions
    {
        public const string SectionName = "Caching";

        public bool Enabled { get; set; } = true;

        public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(10);

        public TimeSpan DefaultLocalCacheExpiration { get; set; } = TimeSpan.FromMinutes(2);

        public bool DistributedEnabled { get; set; }

        public string RedisConnectionStringName { get; set; } = "Redis";
    }
}