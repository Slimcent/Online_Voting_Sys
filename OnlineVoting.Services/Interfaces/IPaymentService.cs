using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<Result<InvoiceResponse>> CreateInvoice(CreateInvoiceRequest request);
        Task<Result<InitiatePaymentResponse>> InitiatePayment(InitiatePaymentRequest request);
        Task<Result<PaymentVerificationResponse>> VerifyPayment(string paymentReference);
        Task<Result<string>> ProcessPaymentWebhook(string gatewayCode, string payload, string signature);
        Task<Result<InvoiceResponse>> GetInvoice(string invoiceId);
        Task<Result<PagedResponse<InvoiceResponse>>> GetInvoices(InvoiceRequest request);
        Task<Result<PagedResponse<PaymentTransactionResponse>>> GetPaymentTransactions(PaymentTransactionRequest request);
        Task<Result<PaymentTransactionResponse>> GetPaymentTransaction(string paymentReference);
    }
}
