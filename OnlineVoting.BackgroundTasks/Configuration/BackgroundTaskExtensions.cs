using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineVoting.BackgroundTasks.Implementation;
using OnlineVoting.BackgroundTasks.Interfaces;

namespace OnlineVoting.BackgroundTasks.Configuration
{
    public static class BackgroundTaskExtensions
    {
        public static IServiceCollection AddBackgroundTasks(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection section = configuration.GetSection(BackgroundTaskOptions.SectionName);

            services.AddOptions<BackgroundTaskOptions>()
                .Bind(section)
                .Validate(options => !string.IsNullOrWhiteSpace(options.SchemaName),
                    "BackgroundTasks:SchemaName is required.")
                .Validate(options => options.WorkerCount > 0,
                    "BackgroundTasks:WorkerCount must be greater than zero.")
                .Validate(options => options.QueuePollIntervalSeconds > 0,
                    "BackgroundTasks:QueuePollIntervalSeconds must be greater than zero.")
                .Validate(options => options.RetryAttempts >= 0,
                    "BackgroundTasks:RetryAttempts cannot be negative.")
                .Validate(options => options.RetryDelaysSeconds.Length >= options.RetryAttempts,
                    "BackgroundTasks:RetryDelaysSeconds must contain at least one delay for each retry attempt.")
                .ValidateOnStart();

            BackgroundTaskOptions backgroundTaskOptions = section.Get<BackgroundTaskOptions>() ?? new BackgroundTaskOptions();

            string? connectionString = configuration.GetConnectionString(BackgroundTaskOptions.ConnectionStringName);

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException($"The connection string '{BackgroundTaskOptions.ConnectionStringName}' was not found.");

            services.AddHangfire(hangfireConfiguration =>
            {
                hangfireConfiguration
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                    {
                        SchemaName = backgroundTaskOptions.SchemaName,
                        PrepareSchemaIfNecessary = true,
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.FromSeconds(backgroundTaskOptions.QueuePollIntervalSeconds),
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true
                    })
                    .UseFilter(new AutomaticRetryAttribute
                    {
                        Attempts = backgroundTaskOptions.RetryAttempts,
                        DelaysInSeconds = backgroundTaskOptions.RetryDelaysSeconds,
                        OnAttemptsExceeded = AttemptsExceededAction.Fail
                    });
            });

            services.AddHangfireServer(options =>
            {
                options.WorkerCount = backgroundTaskOptions.WorkerCount;
            });

            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

            return services;
        }
    }
}