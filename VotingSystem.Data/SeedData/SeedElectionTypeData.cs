using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Entities.OnlineVoting.Models.Entities;

namespace VotingSystem.Data.SeedData
{
    public static class SeedElectionTypeData
    {
        public static async Task SeedElectionTypes(VotingDbContext context)
        {
            bool recordsExist = await context.Set<ElectionType>().AnyAsync();

            if (recordsExist)
                return;

            Dictionary<string, int> electionScopeIds = await context.Set<ElectionScope>()
                .ToDictionaryAsync(electionScope => electionScope.Code, electionScope => electionScope.Id);

            List<ElectionType> electionTypes = new List<ElectionType>
            {
                new ElectionType
                {
                    Name = "Department Election",
                    Description = "An election conducted within a department.",
                    ElectionScopeId = electionScopeIds["DEPARTMENT"]
                },
                new ElectionType
                {
                    Name = "Faculty Election",
                    Description = "An election conducted within a faculty.",
                    ElectionScopeId = electionScopeIds["FACULTY"]
                },
                new ElectionType
                {
                    Name = "University Election",
                    Description = "An election conducted across the university.",
                    ElectionScopeId = electionScopeIds["UNIVERSITY"]
                }
            };

            await context.Set<ElectionType>().AddRangeAsync(electionTypes);
        }
    }
}