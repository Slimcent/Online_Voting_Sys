using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Tests.TestData.Contexts
{
    public sealed class TestCurrentUserContext : ICurrentUserContext
    {
        public TestCurrentUserContext(string? username = "super.admin", string? userId = "user-id")
        {
            Username = username;
            UserId = userId;
        }

        public string? Username { get; set; }

        public string? UserId { get; set; }
    }
}
