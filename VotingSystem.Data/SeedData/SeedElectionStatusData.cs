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
                    Code = "DRAFT",
                    Name = "Draft",
                    Description = "The election has been created but is not yet operational."
                },
                new ElectionStatus
                {
                    Code = "REGISTRATION_OPEN",
                    Name = "Registration Open",
                    Description = "Candidate applications and/or voter registration are currently open according to the configured election periods."
                },
                new ElectionStatus
                {
                    Code = "REGISTRATION_CLOSED",
                    Name = "Registration Closed",
                    Description = "Candidate applications and voter registration have closed."
                },
                new ElectionStatus
                {
                    Code = "VOTING_OPEN",
                    Name = "Voting Open",
                    Description = "Voting is currently open."
                },
                new ElectionStatus
                {
                    Code = "COMPLETED",
                    Name = "Completed",
                    Description = "The election has been completed."
                },
                new ElectionStatus
                {
                    Code = "CANCELLED",
                    Name = "Cancelled",
                    Description = "The election has been cancelled."
                }
            };

            await context.Set<ElectionStatus>().AddRangeAsync(electionStatuses);
            await context.SaveChangesAsync();
        }
    }
}