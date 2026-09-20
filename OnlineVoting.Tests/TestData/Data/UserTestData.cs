using OnlineVoting.Models.Entities;

namespace OnlineVoting.Tests.TestData.Data
{
    public static class UserTestData
    {
        public static User CreateUser(string id, bool emailConfirmed, DateTime createdAt, bool active = true)
        {
            return new User
            {
                Id = id,
                EmailConfirmed = emailConfirmed,
                Active = active,
                UserTypeId = 1,
                CreatedAt = createdAt
            };
        }

        public static User CreateExpiredUnconfirmedUser(string id, int daysOld = 6, bool active = true)
        {
            return CreateUser(id, false, DateTime.UtcNow.AddDays(-daysOld), active);
        }

        public static User CreateRecentUnconfirmedUser(string id, int daysOld = 4)
        {
            return CreateUser(id, false, DateTime.UtcNow.AddDays(-daysOld));
        }

        public static User CreateConfirmedUser(string id, int daysOld = 10, bool active = true)
        {
            return CreateUser(id, true, DateTime.UtcNow.AddDays(-daysOld), active);
        }

        public static User CreateUserAtRetentionBoundary(string id, int retentionDays = 5)
        {
            return CreateUser(id, false, DateTime.UtcNow.AddDays(-retentionDays));
        }
    }
}