using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Context;

namespace VotingSystem.Data.SeedData
{
    public static class SeedPositionApplicationStatusData
    {
        public static async Task SeedPositionApplicationStatuses(VotingDbContext context)
        {
            bool positionApplicationStatusesExist = await context.Set<PositionApplicationStatus>().AnyAsync();

            if (positionApplicationStatusesExist)
                return;

            List<PositionApplicationStatus> positionApplicationStatuses = new List<PositionApplicationStatus>
            {
                new PositionApplicationStatus
                {
                    Code = ApplicationConstants.PositionApplicationStatuses.PendingPayment,
                    Name = "Pending Payment",
                    Description = "The position application is waiting for payment."
                },
                new PositionApplicationStatus
                {
                    Code = ApplicationConstants.PositionApplicationStatuses.PendingReview,
                    Name = "Pending Review",
                    Description = "The position application is waiting for review."
                },
                new PositionApplicationStatus
                {
                    Code = ApplicationConstants.PositionApplicationStatuses.Approved,
                    Name = "Approved",
                    Description = "The position application has been approved."
                },
                new PositionApplicationStatus
                {
                    Code = ApplicationConstants.PositionApplicationStatuses.Rejected,
                    Name = "Rejected",
                    Description = "The position application has been rejected."
                },
                new PositionApplicationStatus
                {
                    Code = ApplicationConstants.PositionApplicationStatuses.Withdrawn,
                    Name = "Withdrawn",
                    Description = "The position application has been withdrawn."
                }
            };

            await context.Set<PositionApplicationStatus>().AddRangeAsync(positionApplicationStatuses);
            await context.SaveChangesAsync();
        }
    }
}