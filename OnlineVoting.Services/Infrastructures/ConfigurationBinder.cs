using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineVoting.Services.Extension;
using OnlineVoting.Services.Infrastructures.Authorization.Jwt;
using VotingSystem.Data.SeedData;

namespace OnlineVoting.Services.Infrastructures
{
    public static class ConfigurationBinder
    {
        public static IServiceCollection BindConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            JwtSettings jwt = new();
            Seed seed = new();

            configuration.GetSection("JwtSettings").Bind(jwt);
            services.AddSingleton(jwt.Validate());

            configuration.GetSection("Seed").Bind(seed);
            services.AddSingleton(seed.Validate());

            return services;
        }
    }
}