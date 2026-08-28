using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;

namespace VotingSystem.Data.SeedData
{
    public static class SeedAuditData
    {
        public static async Task SeedAuditOutcomes(VotingDbContext context)
        {
            bool auditOutcomesExist = await context.Set<AuditOutcome>().AnyAsync();

            if (auditOutcomesExist)
                return;

            List<AuditOutcome> auditOutcomes = new List<AuditOutcome>
            {
                new AuditOutcome
                {
                    Name = "Success",
                    Description = "The operation completed successfully."
                },
                new AuditOutcome
                {
                    Name = "Failure",
                    Description = "The operation failed."
                },
                new AuditOutcome
                {
                    Name = "Denied",
                    Description = "The operation was rejected or not permitted."
                }
            };

            await context.Set<AuditOutcome>().AddRangeAsync(auditOutcomes);
            await context.SaveChangesAsync();
        }
    }
}