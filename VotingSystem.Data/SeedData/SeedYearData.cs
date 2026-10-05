using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;

namespace VotingSystem.Data.SeedData
{
    public static class SeedYearData
    {
        public static async Task SeedYears(VotingDbContext context)
        {
            bool yearsExist = await context.Set<Year>().AnyAsync();

            if (yearsExist)
                return;

            List<Year> years = new List<Year>
            {
                new Year
                {
                    Name = "2025/2026"
                },
                new Year
                {
                    Name = "2026/2027"
                },
                new Year
                {
                    Name = "2027/2028"
                }
            };

            await context.Set<Year>().AddRangeAsync(years);
            await context.SaveChangesAsync();
        }
    }
}