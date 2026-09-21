using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;

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
                    Name = "Pending Payment",
                    Description = "The application is waiting for payment."
                },
                new PositionApplicationStatus
                {
                    Name = "Pending Review",
                    Description = "The application is waiting to be reviewed."
                },
                new PositionApplicationStatus
                {
                    Name = "Approved",
                    Description = "The application has been approved."
                },
                new PositionApplicationStatus
                {
                    Name = "Rejected",
                    Description = "The application has been rejected."
                },
                new PositionApplicationStatus
                {
                    Name = "Withdrawn",
                    Description = "The application has been withdrawn."
                }
            };

            await context.Set<PositionApplicationStatus>().AddRangeAsync(positionApplicationStatuses);
            await context.SaveChangesAsync();
        }
    }
}