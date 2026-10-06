using OnlineVoting.Caching.Configuration;
using OnlineVoting.Services.Caching.Tags;


namespace OnlineVoting.Services.Caching.Policies
{
    public static class CachePolicies
    {
        public static readonly CacheEntryOptions Faculty = new()
        {
            Tags = [CacheTags.Faculty]
        };

        public static readonly CacheEntryOptions Department = new()
        {
            Tags = [CacheTags.Department]
        };

        public static readonly CacheEntryOptions PositionApplication = new()
        {
            Tags = [CacheTags.PositionApplication]
        };

        public static readonly CacheEntryOptions Contestant = new()
        {
            Tags = [CacheTags.Contestant]
        };

        public static CacheEntryOptions Invoice => new()
        {
            Tags = [CacheTags.Invoice]
        };

        public static CacheEntryOptions PaymentTransaction => new()
        {
            Tags = [CacheTags.PaymentTransaction]
        };

        public static readonly CacheEntryOptions RegisteredVoter = new()
        {
            Tags = [CacheTags.RegisteredVoter]
        };

        public static readonly CacheEntryOptions VoteHistory = new()
        {
            Tags = [CacheTags.VoteHistory]
        };

        public static readonly CacheEntryOptions ElectionResult = new()
        {
            Tags = [CacheTags.ElectionResult]
        };
    }
}
