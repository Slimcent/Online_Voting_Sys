using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities.OnlineVoting.Models.Entities;
using Microsoft.EntityFrameworkCore;


namespace VotingSystem.Data.SeedData
{
    public static class SeedElectionScopeData
    {
        public static async Task SeedElectionScopes(VotingDbContext context)
        {
            List<ElectionScope> electionScopes = new List<ElectionScope>
            {
                new ElectionScope
                {
                    Code = "UNIVERSITY",
                    Name = "University",
                    Description = "Applies across the university."
                },
                new ElectionScope
                {
                    Code = "FACULTY",
                    Name = "Faculty",
                    Description = "Applies within a faculty."
                },
                new ElectionScope
                {
                    Code = "DEPARTMENT",
                    Name = "Department",
                    Description = "Applies within a department."
                }
            };

            List<string> existingCodes = await context.Set<ElectionScope>()
                .Select(electionScope => electionScope.Code)
                .ToListAsync();

            List<ElectionScope> missingElectionScopes = electionScopes
                .Where(electionScope => !existingCodes.Contains(electionScope.Code, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (missingElectionScopes.Count == 0)
                return;

            await context.Set<ElectionScope>().AddRangeAsync(missingElectionScopes);
        }
    }
}
