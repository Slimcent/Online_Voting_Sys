using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OnlineVoting.Api.HealthChecks
{
    public sealed class RedisHealthCheck : IHealthCheck
    {
        private const string HealthCheckKey = "online-voting-health-check";

        private readonly IDistributedCache distributedCache;

        public RedisHealthCheck(IDistributedCache distributedCache)
        {
            this.distributedCache = distributedCache;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            await distributedCache.GetAsync(HealthCheckKey, cancellationToken);

            return HealthCheckResult.Healthy();
        }
    }
}