using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OnlineVoting.Api.Controllers;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Infrastructures;
using OnlineVoting.Services.Interfaces;
using System.Text;

namespace OnlineVoting.Tests.UnitTests.Controllers
{
    public class PaymentControllerTests
    {
        [Fact]
        public async Task InitiatePayment_WithCreatedResult_ShouldReturnCreated()
        {
            Mock<IPaymentService> paymentService = new Mock<IPaymentService>();

            PaymentController controller = new PaymentController(paymentService.Object);

            InitiatePaymentRequest request = new InitiatePaymentRequest
            {
                InvoiceId = Guid.NewGuid().ToString(),
                PaymentGatewayId = 1,
                IdempotencyKey = "payment-key-001"
            };

            InitiatePaymentResponse response = new InitiatePaymentResponse
            {
                PaymentTransactionId = Guid.NewGuid().ToString(),
                PaymentReference = "PAY-123",
                ProviderReference = "PAYSTACK-123",
                CheckoutUrl = "https://checkout.paystack.com/test",
                PaymentGatewayCode = ApplicationConstants.PaymentGateways.Paystack,
                PaymentStatus = "Pending",
                Amount = 5000m,
                Currency = "NGN"
            };

            paymentService.Setup(x => x.InitiatePayment(request))
                .ReturnsAsync(Result<InitiatePaymentResponse>.Created(response));

            IActionResult result = await controller.InitiatePayment(request);

            ObjectResult createdResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
            Assert.Equal(response, createdResult.Value);

            paymentService.Verify(x => x.InitiatePayment(request), Times.Once);
        }

        [Fact]
        public async Task VerifyPayment_WithSuccessfulResult_ShouldReturnOk()
        {
            Mock<IPaymentService> paymentService = new Mock<IPaymentService>();

            PaymentController controller = new PaymentController(paymentService.Object);

            string paymentReference = "PAY-123";

            PaymentVerificationResponse response = new PaymentVerificationResponse
            {
                PaymentTransactionId = Guid.NewGuid().ToString(),
                PaymentReference = paymentReference,
                ProviderReference = "PAYSTACK-123",
                PaymentStatus = ApplicationConstants.GatewayPaymentStatuses.Succeeded,
                InvoiceStatus = ApplicationConstants.InvoiceStatuses.Paid,
                Amount = 5000m,
                Currency = "NGN",
                PaidAt = DateTime.UtcNow
            };

            paymentService.Setup(x => x.VerifyPayment(paymentReference))
                .ReturnsAsync(Result<PaymentVerificationResponse>.Success(response));

            IActionResult result = await controller.VerifyPayment(paymentReference);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Equal(response, successResponse.Data);

            paymentService.Verify(x => x.VerifyPayment(paymentReference), Times.Once);
        }

        [Fact]
        public async Task ProcessPaystackWebhook_WithSuccessfulResult_ShouldReturnOk()
        {
            Mock<IPaymentService> paymentService = new Mock<IPaymentService>();

            PaymentController controller = new PaymentController(paymentService.Object);

            string payload = """
                {
                    "event": "charge.success",
                    "data": {
                        "reference": "PAY-123"
                    }
                }
                """;

            string signature = "test-signature";

            DefaultHttpContext httpContext = new DefaultHttpContext();

            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(payload));
            httpContext.Request.Headers["x-paystack-signature"] = signature;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            paymentService.Setup(x => x.ProcessPaymentWebhook(ApplicationConstants.PaymentGateways.Paystack, payload, signature))
                .ReturnsAsync(Result<string>.Success("Webhook received."));

            IActionResult result = await controller.ProcessPaystackWebhook();

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Equal("Webhook received.", successResponse.Data);

            paymentService.Verify(x => x.ProcessPaymentWebhook(ApplicationConstants.PaymentGateways.Paystack, payload, signature), Times.Once);
        }

        [Fact]
        public async Task GetInvoice_ReturnsOk_WhenInvoiceIsRetrievedSuccessfully()
        {
            Mock<IPaymentService> paymentService = new Mock<IPaymentService>();
            PaymentController controller = new PaymentController(paymentService.Object);

            string invoiceId = Guid.NewGuid().ToString();

            InvoiceResponse response = new()
            {
                Id = invoiceId,
                InvoiceNumber = "INV-001",
                Amount = 5000m,
                Currency = "NGN",
                Status = "Unpaid"
            };

            paymentService.Setup(x => x.GetInvoice(invoiceId))
                .ReturnsAsync(Result<InvoiceResponse>.Success(response));

            IActionResult actionResult = await controller.GetInvoice(invoiceId);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);
            InvoiceResponse result = Assert.IsType<InvoiceResponse>(successResponse.Data);

            Assert.Equal(response.Id, result.Id);
            Assert.Equal(response.InvoiceNumber, result.InvoiceNumber);
            Assert.Equal(response.Amount, result.Amount);
            Assert.Equal(response.Currency, result.Currency);
            Assert.Equal(response.Status, result.Status);

            paymentService.Verify(x => x.GetInvoice(invoiceId), Times.Once);
        }

        [Fact]
        public async Task GetInvoices_ReturnsOk_WhenInvoicesAreRetrievedSuccessfully()
        {
            Mock<IPaymentService> paymentService = new Mock<IPaymentService>();
            PaymentController controller = new PaymentController(paymentService.Object);

            InvoiceRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<InvoiceResponse> response = new()
            {
                Items =
                [
                            new InvoiceResponse
                    {
                        Id = Guid.NewGuid().ToString(),
                        InvoiceNumber = "INV-001",
                        Amount = 5000m,
                        Currency = "NGN",
                        Status = "Unpaid"
                    }
                ]
            };

            paymentService.Setup(x => x.GetInvoices(request))
                .ReturnsAsync(Result<PagedResponse<InvoiceResponse>>.Success(response));

            IActionResult actionResult = await controller.GetInvoices(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);
            PagedResponse<InvoiceResponse> result = Assert.IsType<PagedResponse<InvoiceResponse>>(successResponse.Data);

            Assert.NotNull(result.Items);
            Assert.Single(result.Items);
            Assert.Equal("INV-001", result.Items.Single().InvoiceNumber);

            paymentService.Verify(x => x.GetInvoices(request), Times.Once);
        }

        [Fact]
        public async Task GetPaymentTransactions_ReturnsOk_WhenPaymentTransactionsAreRetrievedSuccessfully()
        {
            Mock<IPaymentService> paymentService = new Mock<IPaymentService>();
            PaymentController controller = new PaymentController(paymentService.Object);

            PaymentTransactionRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<PaymentTransactionResponse> response = new()
            {
                Items =
                [
                    new PaymentTransactionResponse
            {
                PaymentTransactionId = Guid.NewGuid().ToString(),
                InvoiceId = Guid.NewGuid().ToString(),
                PaymentReference = "PAY-001",
                ProviderReference = "PROVIDER-PAY-001",
                PaymentGateway = "Paystack",
                PaymentStatus = "Pending",
                StudentId = Guid.NewGuid(),
                UserId = "user-123",
                PayerFirstName = "Test",
                PayerLastName = "Student",
                Amount = 5000m,
                Currency = "NGN",
                CreatedAt = DateTime.UtcNow
            }
                ]
            };

            paymentService.Setup(x => x.GetPaymentTransactions(request))
                .ReturnsAsync(Result<PagedResponse<PaymentTransactionResponse>>.Success(response));

            IActionResult actionResult = await controller.GetPaymentTransactions(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);
            PagedResponse<PaymentTransactionResponse> result = Assert.IsType<PagedResponse<PaymentTransactionResponse>>(successResponse.Data);

            Assert.NotNull(result.Items);
            Assert.Single(result.Items);

            PaymentTransactionResponse paymentTransaction = result.Items.Single();

            Assert.Equal("PAY-001", paymentTransaction.PaymentReference);
            Assert.Equal("Paystack", paymentTransaction.PaymentGateway);

            paymentService.Verify(x => x.GetPaymentTransactions(request), Times.Once);
        }

        [Fact]
        public async Task GetPaymentTransaction_ReturnsOk_WhenPaymentTransactionIsRetrievedSuccessfully()
        {
            Mock<IPaymentService> paymentService = new Mock<IPaymentService>();
            PaymentController controller = new PaymentController(paymentService.Object);

            string paymentReference = "PAY-001";

            PaymentTransactionResponse response = new()
            {
                PaymentTransactionId = Guid.NewGuid().ToString(),
                InvoiceId = Guid.NewGuid().ToString(),
                PaymentReference = paymentReference,
                ProviderReference = "PROVIDER-PAY-001",
                PaymentGateway = "Paystack",
                PaymentStatus = "Succeeded",
                StudentId = Guid.NewGuid(),
                UserId = "user-123",
                PayerFirstName = "Test",
                PayerLastName = "Student",
                Amount = 5000m,
                Currency = "NGN",
                PaidAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            paymentService.Setup(x => x.GetPaymentTransaction(paymentReference))
                .ReturnsAsync(Result<PaymentTransactionResponse>.Success(response));

            IActionResult actionResult = await controller.GetPaymentTransaction(paymentReference);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);
            PaymentTransactionResponse result = Assert.IsType<PaymentTransactionResponse>(successResponse.Data);

            Assert.Equal(paymentReference, result.PaymentReference);
            Assert.Equal("Succeeded", result.PaymentStatus);
            Assert.Equal("Paystack", result.PaymentGateway);

            paymentService.Verify(x => x.GetPaymentTransaction(paymentReference), Times.Once);
        }
    }
}