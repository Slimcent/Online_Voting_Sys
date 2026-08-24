using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Interfaces;

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

            VotingDbContext context = new(options, new TestCurrentUserContext());

            await context.Database.EnsureCreatedAsync();

            return (connection, context);
        }

        public static VotingDbContext Create(SqliteConnection connection)
        {
            DbContextOptions<VotingDbContext> options = new DbContextOptionsBuilder<VotingDbContext>()
                .UseSqlite(connection)
                .Options;

            return new VotingDbContext(options, new TestCurrentUserContext());
        }

        private sealed class TestCurrentUserContext : ICurrentUserContext
        {
            public string? Username => "testuser";
        }
    }
}