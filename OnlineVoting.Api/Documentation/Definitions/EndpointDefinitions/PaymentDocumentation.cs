using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Documentation.Models;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Api.Documentation.Definitions.EndpointDefinitions
{
    public static class PaymentDocumentation
    {
        public static readonly IReadOnlyDictionary<string, ApiOperationDocumentation> Operations = new Dictionary<string, ApiOperationDocumentation>
        {
            [PaymentDocumentationKeys.InitiatePayment] = new ApiOperationDocumentation
            {
                Summary = "Initiates a payment.",
                Description = "Initiates payment for an unpaid invoice using the selected payment gateway and returns the payment checkout information.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["201"] = new ApiResponseDocumentation
                    {
                        Description = "The payment was initialized successfully.",
                        ResponseType = typeof(InitiatePaymentResponse)
                    },
                    ["400"] = CommonApiResponses.BadRequest("The payment request is invalid."),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound("The invoice, payment gateway, or required payment status could not be found."),
                    ["409"] = CommonApiResponses.Conflict("The payment cannot be initiated because the invoice or another payment attempt is already being processed.")
                }
            },

            [PaymentDocumentationKeys.VerifyPayment] = new ApiOperationDocumentation
            {
                Summary = "Verifies a payment.",
                Description = "Verifies the payment with the configured payment gateway and reconciles the local payment, invoice, and position application states.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The payment was verified successfully.",
                        ResponseType = typeof(PaymentVerificationResponse)
                    },
                    ["400"] = CommonApiResponses.BadRequest("The payment reference or payment verification information is invalid."),
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["403"] = CommonApiResponses.Forbidden(),
                    ["404"] = CommonApiResponses.NotFound("The specified payment transaction or required payment state could not be found."),
                    ["409"] = CommonApiResponses.Conflict("The verified payment details do not match the stored payment information.")
                }
            },

            [PaymentDocumentationKeys.ProcessPaystackWebhook] = new ApiOperationDocumentation
            {
                Summary = "Processes a Paystack payment webhook.",
                Description = "Receives a Paystack payment event, validates its signature, and reconciles successful payments with the local payment state.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The webhook was received successfully.",
                        ResponseType = typeof(string)
                    },
                    ["400"] = CommonApiResponses.BadRequest("The webhook payload or payment gateway configuration is invalid."),
                    ["401"] = CommonApiResponses.Unauthorized()
                }
            },

            [PaymentDocumentationKeys.GetInvoice] = new ApiOperationDocumentation
            {
                Summary = "Gets an invoice.",
                Description = "Returns an invoice by its identifier.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The invoice was retrieved successfully.",
                        ResponseType = typeof(InvoiceResponse)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["404"] = CommonApiResponses.NotFound()
                }
            },

            [PaymentDocumentationKeys.GetInvoices] = new ApiOperationDocumentation
            {
                Summary = "Gets invoices.",
                Description = "Returns a paginated list of invoices with optional filtering.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The invoices were retrieved successfully.",
                        ResponseType = typeof(PagedResponse<InvoiceResponse>)
                    },
                    ["401"] = CommonApiResponses.Unauthorized()
                }
            },

            [PaymentDocumentationKeys.GetPaymentTransactions] = new ApiOperationDocumentation
            {
                Summary = "Gets payment transactions.",
                Description = "Returns a paginated list of payment transactions with optional filtering.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The payment transactions were retrieved successfully.",
                        ResponseType = typeof(PagedResponse<PaymentTransactionResponse>)
                    },
                    ["401"] = CommonApiResponses.Unauthorized()
                }
            },

            [PaymentDocumentationKeys.GetPaymentTransaction] = new ApiOperationDocumentation
            {
                Summary = "Gets a payment transaction.",
                Description = "Returns a payment transaction by its payment reference.",
                Responses = new Dictionary<string, ApiResponseDocumentation>
                {
                    ["200"] = new ApiResponseDocumentation
                    {
                        Description = "The payment transaction was retrieved successfully.",
                        ResponseType = typeof(PaymentTransactionResponse)
                    },
                    ["401"] = CommonApiResponses.Unauthorized(),
                    ["404"] = CommonApiResponses.NotFound()
                }
            },
        };
    }
}