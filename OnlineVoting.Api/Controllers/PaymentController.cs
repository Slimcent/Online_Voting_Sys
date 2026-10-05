using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Api.Documentation.Attributes;
using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Extensions;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Policy = "Authorization")]
    public class PaymentController : BaseController
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("initiate-payment", Name = "Initiate-Payment")]
        [ApiDocumentation(PaymentDocumentationKeys.InitiatePayment)]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentRequest request)
        {
            Result<InitiatePaymentResponse> result = await _paymentService.InitiatePayment(request);
            return result.ToActionResult(this);
        }

        [HttpGet("verify-payment/{paymentReference}", Name = "Verify-Payment")]
        [ApiDocumentation(PaymentDocumentationKeys.VerifyPayment)]
        public async Task<IActionResult> VerifyPayment(string paymentReference)
        {
            Result<PaymentVerificationResponse> result = await _paymentService.VerifyPayment(paymentReference);
            return result.ToActionResult(this);
        }

        [AllowAnonymous]
        [HttpPost("webhook/paystack", Name = "Paystack-Payment-Webhook")]
        [ApiDocumentation(PaymentDocumentationKeys.ProcessPaystackWebhook)]
        public async Task<IActionResult> ProcessPaystackWebhook()
        {
            using StreamReader reader = new StreamReader(Request.Body);

            string payload = await reader.ReadToEndAsync();
            string signature = Request.Headers["x-paystack-signature"].ToString();

            Result<string> result = await _paymentService.ProcessPaymentWebhook(ApplicationConstants.PaymentGateways.Paystack, payload, signature);

            return result.ToActionResult(this);
        }

        [HttpGet("invoice/{invoiceId}", Name = "Get-Invoice")]
        [ApiDocumentation(PaymentDocumentationKeys.GetInvoice)]
        public async Task<IActionResult> GetInvoice(string invoiceId)
        {
            Result<InvoiceResponse> result = await _paymentService.GetInvoice(invoiceId);

            return result.ToActionResult(this);
        }

        [HttpGet("invoices", Name = "Get-Invoices")]
        [ApiDocumentation(PaymentDocumentationKeys.GetInvoices)]
        public async Task<IActionResult> GetInvoices([FromQuery] InvoiceRequest request)
        {
            Result<PagedResponse<InvoiceResponse>> result = await _paymentService.GetInvoices(request);

            return result.ToActionResult(this);
        }

        [HttpGet("payment-transactions", Name = "Get-Payment-Transactions")]
        [ApiDocumentation(PaymentDocumentationKeys.GetPaymentTransactions)]
        public async Task<IActionResult> GetPaymentTransactions([FromQuery] PaymentTransactionRequest request)
        {
            Result<PagedResponse<PaymentTransactionResponse>> result = await _paymentService.GetPaymentTransactions(request);

            return result.ToActionResult(this);
        }

        [HttpGet("payment-transaction/{paymentReference}", Name = "Get-Payment-Transaction")]
        [ApiDocumentation(PaymentDocumentationKeys.GetPaymentTransaction)]
        public async Task<IActionResult> GetPaymentTransaction(string paymentReference)
        {
            Result<PaymentTransactionResponse> result = await _paymentService.GetPaymentTransaction(paymentReference);

            return result.ToActionResult(this);
        }
    }
}