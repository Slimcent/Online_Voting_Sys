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
    }
}
