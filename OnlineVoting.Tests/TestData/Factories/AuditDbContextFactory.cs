using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;
using OnlineVoting.Tests.TestData.Contexts;

namespace OnlineVoting.Tests.TestData.Factories
{
    public sealed class AuditDbContextFactory : IDisposable
    {
        private readonly SqliteConnection _connection;

        public VotingDbContext Context { get; }

        public TestCurrentUserContext CurrentUserContext { get; }

        public TestAuditMetadataProvider AuditMetadataProvider { get; }

        public AuditDbContextFactory(string? username = "super.admin", string? userId = "user-id")
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            DbContextOptions<VotingDbContext> options = new DbContextOptionsBuilder<VotingDbContext>()
                .UseSqlite(_connection)
                .Options;

            CurrentUserContext = new TestCurrentUserContext(username, userId);

            AuditMetadataProvider = new TestAuditMetadataProvider
            {
                ActorUserId = userId,
                ActorUsername = username
            };

            Context = new VotingDbContext(options, CurrentUserContext, AuditMetadataProvider);

            Context.Database.EnsureCreated();

            Context.AuditOutcomes.AddRange(
                new AuditOutcome
                {
                    Name = ApplicationConstants.Audit.Outcomes.Success,
                    Description = "The operation completed successfully."
                },
                new AuditOutcome
                {
                    Name = ApplicationConstants.Audit.Outcomes.Failure,
                    Description = "The operation failed."
                },
                new AuditOutcome
                {
                    Name = ApplicationConstants.Audit.Outcomes.Denied,
                    Description = "The operation was denied."
                });

            Context.SaveChanges();
        }

        public void SetRequestMetadata(string endpointName, string httpMethod)
        {
            AuditMetadataProvider.EndpointName = endpointName;
            AuditMetadataProvider.HttpMethod = httpMethod;
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
        }
    }
}