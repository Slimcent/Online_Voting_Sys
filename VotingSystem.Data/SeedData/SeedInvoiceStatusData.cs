using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;

namespace VotingSystem.Data.SeedData
{
    public static class SeedInvoiceStatusData
    {
        public static async Task SeedInvoiceStatuses(VotingDbContext context)
        {
            bool invoiceStatusesExist = await context.Set<InvoiceStatus>().AnyAsync();

            if (invoiceStatusesExist)
                return;

            List<InvoiceStatus> invoiceStatuses = new List<InvoiceStatus>
            {
                new InvoiceStatus
                {
                    Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                    Name = "Unpaid",
                    Description = "The invoice has not been paid."
                },
                new InvoiceStatus
                {
                    Code = ApplicationConstants.InvoiceStatuses.Paid,
                    Name = "Paid",
                    Description = "The invoice has been paid."
                },
                new InvoiceStatus
                {
                    Code = ApplicationConstants.InvoiceStatuses.Cancelled,
                    Name = "Cancelled",
                    Description = "The invoice has been cancelled."
                }
            };

            await context.Set<InvoiceStatus>().AddRangeAsync(invoiceStatuses);
            await context.SaveChangesAsync();
        }
    }
}