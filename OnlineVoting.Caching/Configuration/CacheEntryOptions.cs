namespace OnlineVoting.Caching.Configuration
{
    public sealed class CacheEntryOptions
    {
        public TimeSpan? Expiration { get; init; }

        public TimeSpan? LocalCacheExpiration { get; init; }

        public CacheMode Mode { get; init; } = CacheMode.Hybrid;

        public IReadOnlyCollection<string>? Tags { get; init; }
    }
}