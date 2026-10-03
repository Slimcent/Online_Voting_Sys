using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;

namespace VotingSystem.Data.SeedData
{
    public static class SeedPaymentStatusData
    {
        public static async Task SeedPaymentStatuses(VotingDbContext context)
        {
            bool paymentStatusesExist = await context.Set<PaymentStatus>().AnyAsync();

            if (paymentStatusesExist)
                return;

            List<PaymentStatus> paymentStatuses = new List<PaymentStatus>
            {
                new PaymentStatus
                {
                    Code = ApplicationConstants.PaymentStatuses.Pending,
                    Name = "Pending",
                    Description = "The payment is waiting to be completed or confirmed."
                },
                new PaymentStatus
                {
                    Code = ApplicationConstants.PaymentStatuses.Succeeded,
                    Name = "Succeeded",
                    Description = "The payment was completed successfully."
                },
                new PaymentStatus
                {
                    Code = ApplicationConstants.PaymentStatuses.Failed,
                    Name = "Failed",
                    Description = "The payment attempt failed."
                },
                new PaymentStatus
                {
                    Code = ApplicationConstants.PaymentStatuses.Cancelled,
                    Name = "Cancelled",
                    Description = "The payment attempt was cancelled."
                }
            };

            await context.Set<PaymentStatus>().AddRangeAsync(paymentStatuses);
            await context.SaveChangesAsync();
        }
    }
}