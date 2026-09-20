using OnlineVoting.Models.Enums;

namespace OnlineVoting.Models.Configurations
{
    public static class RateLimitPolicyNames
    {
        public const string Authentication = nameof(RateLimitPolicy.Authentication);

        public const string Voting = nameof(RateLimitPolicy.Voting);

        public const string AdministrativeWrite = nameof(RateLimitPolicy.AdministrativeWrite);
    }
}