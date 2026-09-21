using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;

namespace VotingSystem.Data.SeedData
{
    public static class SeedElectionStatusData
    {
        public static async Task SeedElectionStatuses(VotingDbContext context)
        {
            bool electionStatusesExist = await context.Set<ElectionStatus>().AnyAsync();

            if (electionStatusesExist)
                return;

            List<ElectionStatus> electionStatuses = new List<ElectionStatus>
            {
                new ElectionStatus
                {
                    Name = "Draft",
                    Description = "The election has been created but is not yet open."
                },
                new ElectionStatus
                {
                    Name = "Application Open",
                    Description = "Applications for election positions are open."
                },
                new ElectionStatus
                {
                    Name = "Application Closed",
                    Description = "Applications for election positions are closed."
                },
                new ElectionStatus
                {
                    Name = "Voter Registration Open",
                    Description = "Voter registration is open."
                },
                new ElectionStatus
                {
                    Name = "Voter Registration Closed",
                    Description = "Voter registration is closed."
                },
                new ElectionStatus
                {
                    Name = "Voting Open",
                    Description = "Voting is currently open."
                },
                new ElectionStatus
                {
                    Name = "Voting Closed",
                    Description = "Voting has ended."
                },
                new ElectionStatus
                {
                    Name = "Completed",
                    Description = "The election has been completed."
                },
                new ElectionStatus
                {
                    Name = "Cancelled",
                    Description = "The election has been cancelled."
                }
            };

            await context.Set<ElectionStatus>().AddRangeAsync(electionStatuses);
            await context.SaveChangesAsync();
        }
    }
}