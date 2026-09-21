using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;

namespace VotingSystem.Data.SeedData
{
    public static class SeedElectionTypeData
    {
        public static async Task SeedElectionTypes(VotingDbContext context)
        {
            bool electionTypesExist = await context.Set<ElectionType>().AnyAsync();

            if (electionTypesExist)
                return;

            List<ElectionType> electionTypes = new List<ElectionType>
            {
                new ElectionType
                {
                    Name = "Department Election",
                    Description = "An election conducted within a department."
                },
                new ElectionType
                {
                    Name = "Faculty Election",
                    Description = "An election conducted within a faculty."
                },
                new ElectionType
                {
                    Name = "University Election",
                    Description = "An election conducted across the university."
                }
            };

            await context.Set<ElectionType>().AddRangeAsync(electionTypes);
            await context.SaveChangesAsync();
        }
    }
}