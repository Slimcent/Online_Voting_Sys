using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Tests.TestData.Data
{
    public static class TestDbContextFactory
    {
        public static VotingDbContext Create(string databaseName)
        {
            DbContextOptions<VotingDbContext> options = new DbContextOptionsBuilder<VotingDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            return new VotingDbContext(options, new TestCurrentUserContext());
        }

        private sealed class TestCurrentUserContext : ICurrentUserContext
        {
            public string? Username => "testuser";
        }
    }
}