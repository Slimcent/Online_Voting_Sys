using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Tests.UnitTests.Models.Context
{
    public class AuditArchitectureTests
    {
        [Fact]
        public void Vote_ShouldNotBeAuditable()
        {
            bool isAuditable = typeof(IAuditable).IsAssignableFrom(typeof(Vote));

            Assert.False(isAuditable);
        }

        [Fact]
        public void RefreshToken_ShouldNotBeAuditable()
        {
            bool isAuditable = typeof(IAuditable).IsAssignableFrom(typeof(RefreshToken));

            Assert.False(isAuditable);
        }
    }
}
