using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;
using OnlineVoting.Tests.TestData.Contexts;

namespace OnlineVoting.Tests.TestData.Data
{
    public static class SqliteTestDbContextFactory
    {
        public static async Task<(SqliteConnection Connection, VotingDbContext Context)> Create()
        {
            SqliteConnection connection = new("Data Source=:memory:");

            await connection.OpenAsync();

            DbContextOptions<VotingDbContext> options = new DbContextOptionsBuilder<VotingDbContext>()
                .UseSqlite(connection)
                .Options;

            TestCurrentUserContext currentUserContext = new();
            TestAuditMetadataProvider auditMetadataProvider = new();

            VotingDbContext context = new(options, currentUserContext, auditMetadataProvider);

            await context.Database.EnsureCreatedAsync();

            await SeedAuditOutcomes(context);

            return (connection, context);
        }

        public static VotingDbContext Create(SqliteConnection connection)
        {
            DbContextOptions<VotingDbContext> options = new DbContextOptionsBuilder<VotingDbContext>()
                .UseSqlite(connection)
                .Options;

            TestCurrentUserContext currentUserContext = new();
            TestAuditMetadataProvider auditMetadataProvider = new();

            return new VotingDbContext(options, currentUserContext, auditMetadataProvider);
        }

        private static async Task SeedAuditOutcomes(VotingDbContext context)
        {
            bool successOutcomeExists = await context.AuditOutcomes.AnyAsync(auditOutcome => auditOutcome.Name == "Success");

            if (successOutcomeExists)
                return;

            await context.AuditOutcomes.AddAsync(new AuditOutcome
            {
                Name = "Success",
                Description = "The operation completed successfully."
            });

            await context.SaveChangesAsync(true);
        }
    }
}