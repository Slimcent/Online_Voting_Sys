using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Services.Caching.Keys
{
    public static class PaymentTransactionCacheKeys
    {
        private const string Prefix = "onlinevoting:v1:payment-transaction";

        public static string GetPaymentTransaction(string paymentReference)
        {
            return $"{Prefix}:reference:{paymentReference}";
        }

        public static string GetPaymentTransactions(PaymentTransactionRequest request)
        {
            return $"{Prefix}:page:{request.PageNumber}:size:{request.PageSize}:gateway:{request.PaymentGatewayId}:status:{request.PaymentStatusId}:invoice:{request.InvoiceId}:student:{request.StudentId}:user:{request.UserId}:search:{request.SearchTerm}";
        }
    }
}
