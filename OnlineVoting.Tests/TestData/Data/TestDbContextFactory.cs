using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;
using OnlineVoting.Tests.TestData.Contexts;
using System.Collections.Concurrent;

namespace OnlineVoting.Tests.TestData.Data
{
    public static class TestDbContextFactory
    {
        private static readonly ConcurrentDictionary<string, SqliteConnection> Connections = new();

        public static VotingDbContext Create(string databaseName)
        {
            string connectionString = $"Data Source={databaseName};Mode=Memory;Cache=Shared";

            Connections.GetOrAdd(databaseName, _ =>
            {
                SqliteConnection connection = new(connectionString);

                connection.Open();

                return connection;
            });

            DbContextOptions<VotingDbContext> options = new DbContextOptionsBuilder<VotingDbContext>()
                .UseSqlite(connectionString)
                .Options;

            TestCurrentUserContext currentUserContext = new();
            TestAuditMetadataProvider auditMetadataProvider = new();

            VotingDbContext context = new(options, currentUserContext, auditMetadataProvider);

            context.Database.EnsureCreated();

            SeedAuditOutcomes(context);

            return context;
        }

        private static void SeedAuditOutcomes(VotingDbContext context)
        {
            bool successOutcomeExists = context.AuditOutcomes
                .Any(auditOutcome => auditOutcome.Name == "Success");

            if (successOutcomeExists)
                return;

            context.AuditOutcomes.Add(new AuditOutcome
            {
                Name = "Success",
                Description = "The operation completed successfully."
            });

            context.SaveChanges(true);

            context.ChangeTracker.Clear();
        }
    }
}