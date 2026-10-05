using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Context;
using OnlineVoting.Models.Entities;

namespace VotingSystem.Data.SeedData
{
    public static class SeedPaymentGatewayData
    {
        public static async Task SeedPaymentGateways(VotingDbContext context)
        {
            bool paymentGatewaysExist = await context.Set<PaymentGateway>().AnyAsync();

            if (paymentGatewaysExist)
                return;

            List<PaymentGateway> paymentGateways = new List<PaymentGateway>
            {
                new PaymentGateway
                {
                    Code = ApplicationConstants.PaymentGateways.Paystack,
                    Name = "Paystack",
                    Description = "Paystack payment gateway."
                },
                new PaymentGateway
                {
                    Code = ApplicationConstants.PaymentGateways.Flutterwave,
                    Name = "Flutterwave",
                    Description = "Flutterwave payment gateway."
                }
            };

            await context.Set<PaymentGateway>().AddRangeAsync(paymentGateways);
            await context.SaveChangesAsync();
        }
    }
}