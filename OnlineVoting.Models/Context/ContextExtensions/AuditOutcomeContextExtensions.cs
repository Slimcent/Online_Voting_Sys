using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Models.Context.ContextExtensions
{
    public static class AuditOutcomeContextExtensions
    {
        public static int GetSuccessAuditOutcomeId(this DbSet<AuditOutcome> auditOutcomes)
        {
            return auditOutcomes.AsNoTracking()
                .Where(auditOutcome => auditOutcome.Name == "Success")
                .Select(auditOutcome => auditOutcome.Id)
                .Single();
        }

        public static async Task<int> GetSuccessAuditOutcomeId(this DbSet<AuditOutcome> auditOutcomes, CancellationToken cancellationToken)
        {
            return await auditOutcomes.AsNoTracking()
                .Where(auditOutcome => auditOutcome.Name == "Success")
                .Select(auditOutcome => auditOutcome.Id)
                .SingleAsync(cancellationToken);
        }
    }
}