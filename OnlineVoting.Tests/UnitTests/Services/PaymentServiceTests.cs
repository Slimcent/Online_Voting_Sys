using Microsoft.AspNetCore.Http;
using Moq;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Request.Payments;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Dtos.Response.Payments;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces.Payments;
using OnlineVoting.Tests.TestData.Factories;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OnlineVoting.Tests.UnitTests.Services
{
    public class PaymentServiceTests
    {
        [Fact]
        public async Task InitiatePayment_WithValidRequest_ShouldCreatePendingTransactionAndReturnCheckoutInformation()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            string userId = "user-123";
            Guid studentId = Guid.NewGuid();
            string invoiceId = Guid.NewGuid().ToString();

            InvoiceStatus unpaidInvoiceStatus = new InvoiceStatus
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid"
            };

            PaymentStatus pendingPaymentStatus = new PaymentStatus
            {
                Id = 1,
                Code = ApplicationConstants.PaymentStatuses.Pending,
                Name = "Pending"
            };

            PaymentStatus failedPaymentStatus = new PaymentStatus
            {
                Id = 2,
                Code = ApplicationConstants.PaymentStatuses.Failed,
                Name = "Failed"
            };

            PaymentGateway paymentGateway = new PaymentGateway
            {
                Id = 1,
                Code = ApplicationConstants.PaymentGateways.Paystack,
                Name = "Paystack",
                Active = true
            };

            Invoice invoice = new Invoice
            {
                Id = invoiceId,
                PositionApplicationId = Guid.NewGuid().ToString(),
                InvoiceNumber = $"INV-{Guid.NewGuid():N}",
                StudentId = studentId,
                UserId = userId,
                PayerFirstName = "Test",
                PayerLastName = "Student",
                PayerEmail = "student@example.com",
                RegistrationNumber = "REG001",
                Amount = 5000m,
                Currency = "NGN",
                InvoiceStatusId = unpaidInvoiceStatus.Id,
                InvoiceStatus = unpaidInvoiceStatus
            };

            factory.DbContextFactory.Context.InvoiceStatuses.Add(unpaidInvoiceStatus);
            factory.DbContextFactory.Context.PaymentStatuses.AddRange(pendingPaymentStatus, failedPaymentStatus);
            factory.DbContextFactory.Context.PaymentGateways.Add(paymentGateway);
            factory.DbContextFactory.Context.Invoices.Add(invoice);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            Mock<IPaymentGateway> gateway = new Mock<IPaymentGateway>();

            gateway.SetupGet(x => x.Code)
                .Returns(ApplicationConstants.PaymentGateways.Paystack);

            factory.PaymentGatewayResolver.Setup(x => x.Resolve(ApplicationConstants.PaymentGateways.Paystack))
                .Returns(gateway.Object);

            factory.Mapper.Setup(x => x.Map<PaymentTransaction>(It.IsAny<Invoice>()))
                .Returns((Invoice source) => new PaymentTransaction
                {
                    InvoiceId = source.Id,
                    StudentId = source.StudentId,
                    UserId = source.UserId,
                    PayerFirstName = source.PayerFirstName,
                    PayerLastName = source.PayerLastName,
                    PayerEmail = source.PayerEmail,
                    RegistrationNumber = source.RegistrationNumber,
                    Amount = source.Amount,
                    Currency = source.Currency
                });

            factory.Mapper.Setup(x => x.Map<GatewayPaymentInitializationRequest>(It.IsAny<PaymentTransaction>()))
                .Returns((PaymentTransaction source) => new GatewayPaymentInitializationRequest
                {
                    PaymentReference = source.PaymentReference,
                    Amount = source.Amount,
                    Currency = source.Currency,
                    Email = source.PayerEmail,
                    PayerFirstName = source.PayerFirstName,
                    PayerLastName = source.PayerLastName
                });

            GatewayPaymentInitializationResponse gatewayResponse = new GatewayPaymentInitializationResponse
            {
                ProviderReference = "PAYSTACK-123",
                CheckoutUrl = "https://checkout.paystack.com/test"
            };

            gateway.Setup(x => x.InitiatePayment(It.IsAny<GatewayPaymentInitializationRequest>()))
                .ReturnsAsync(Result<GatewayPaymentInitializationResponse>.Success(gatewayResponse));

            factory.Mapper.Setup(x => x.Map<InitiatePaymentResponse>(It.IsAny<PaymentTransaction>()))
                .Returns((PaymentTransaction source) => new InitiatePaymentResponse
                {
                    PaymentTransactionId = source.Id,
                    PaymentReference = source.PaymentReference,
                    ProviderReference = source.ProviderReference,
                    CheckoutUrl = source.CheckoutUrl,
                    Amount = source.Amount,
                    Currency = source.Currency
                });

            InitiatePaymentRequest request = new InitiatePaymentRequest
            {
                InvoiceId = invoiceId,
                PaymentGatewayId = paymentGateway.Id,
                IdempotencyKey = "payment-key-001"
            };

            Result<InitiatePaymentResponse> result = await factory.Service.InitiatePayment(request);

            Assert.True(result.IsSuccess, $"Status: {result.Status}, Error: {result.Error}");
            Assert.NotNull(result.Value);
            Assert.Equal("PAYSTACK-123", result.Value.ProviderReference);
            Assert.Equal("https://checkout.paystack.com/test", result.Value.CheckoutUrl);
            Assert.Equal(ApplicationConstants.PaymentGateways.Paystack, result.Value.PaymentGatewayCode);
            Assert.Equal(pendingPaymentStatus.Name, result.Value.PaymentStatus);
            Assert.Equal(5000m, result.Value.Amount);
            Assert.Equal("NGN", result.Value.Currency);

            PaymentTransaction? paymentTransaction = factory.DbContextFactory.Context.PaymentTransactions
                .SingleOrDefault(x => x.InvoiceId == invoiceId);

            Assert.NotNull(paymentTransaction);
            Assert.Equal(pendingPaymentStatus.Id, paymentTransaction.PaymentStatusId);
            Assert.Equal(paymentGateway.Id, paymentTransaction.PaymentGatewayId);
            Assert.Equal("PAYSTACK-123", paymentTransaction.ProviderReference);
            Assert.Equal("https://checkout.paystack.com/test", paymentTransaction.CheckoutUrl);

            IdempotencyRecord? idempotencyRecord = factory.DbContextFactory.Context.IdempotencyRecords
                .SingleOrDefault(x => x.Key == request.IdempotencyKey);

            Assert.NotNull(idempotencyRecord);
            Assert.Equal(ApplicationConstants.IdempotencyStatuses.Completed, idempotencyRecord.Status);
            Assert.Equal(paymentTransaction.Id, idempotencyRecord.ResourceId);

            gateway.Verify(x => x.InitiatePayment(It.Is<GatewayPaymentInitializationRequest>(gatewayRequest =>
                gatewayRequest.PaymentReference == paymentTransaction.PaymentReference
                && gatewayRequest.Amount == invoice.Amount
                && gatewayRequest.Currency == invoice.Currency
                && gatewayRequest.Email == invoice.PayerEmail)), Times.Once);
        }

        [Fact]
        public async Task InitiatePayment_WithCompletedIdempotencyRequest_ShouldReturnStoredResponse()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            string userId = "user-123";
            string invoiceId = Guid.NewGuid().ToString();
            string idempotencyKey = "payment-key-001";

            InvoiceStatus unpaidInvoiceStatus = new InvoiceStatus
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid"
            };

            PaymentGateway paymentGateway = new PaymentGateway
            {
                Id = 1,
                Code = ApplicationConstants.PaymentGateways.Paystack,
                Name = "Paystack",
                Active = true
            };

            Invoice invoice = new Invoice
            {
                Id = invoiceId,
                PositionApplicationId = Guid.NewGuid().ToString(),
                InvoiceNumber = $"INV-{Guid.NewGuid():N}",
                StudentId = Guid.NewGuid(),
                UserId = userId,
                PayerFirstName = "Test",
                PayerLastName = "Student",
                PayerEmail = "student@example.com",
                RegistrationNumber = "REG001",
                Amount = 5000m,
                Currency = "NGN",
                InvoiceStatusId = unpaidInvoiceStatus.Id,
                InvoiceStatus = unpaidInvoiceStatus
            };

            InitiatePaymentResponse storedResponse = new InitiatePaymentResponse
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

            string requestHash = CreatePaymentRequestHash(invoiceId, paymentGateway.Id);

            IdempotencyRecord idempotencyRecord = new IdempotencyRecord
            {
                Key = idempotencyKey,
                UserId = userId,
                Operation = ApplicationConstants.IdempotencyOperations.InitiatePayment,
                RequestHash = requestHash,
                Status = ApplicationConstants.IdempotencyStatuses.Completed,
                ResourceId = storedResponse.PaymentTransactionId,
                StatusCode = StatusCodes.Status201Created,
                Response = JsonSerializer.Serialize(storedResponse)
            };

            factory.DbContextFactory.Context.InvoiceStatuses.Add(unpaidInvoiceStatus);
            factory.DbContextFactory.Context.PaymentGateways.Add(paymentGateway);
            factory.DbContextFactory.Context.Invoices.Add(invoice);
            factory.DbContextFactory.Context.IdempotencyRecords.Add(idempotencyRecord);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            InitiatePaymentRequest request = new InitiatePaymentRequest
            {
                InvoiceId = invoiceId,
                PaymentGatewayId = paymentGateway.Id,
                IdempotencyKey = idempotencyKey
            };

            Result<InitiatePaymentResponse> result = await factory.Service.InitiatePayment(request);

            Assert.True(result.IsSuccess, $"Status: {result.Status}, Error: {result.Error}");
            Assert.NotNull(result.Value);
            Assert.Equal(storedResponse.PaymentTransactionId, result.Value.PaymentTransactionId);
            Assert.Equal(storedResponse.PaymentReference, result.Value.PaymentReference);
            Assert.Equal(storedResponse.ProviderReference, result.Value.ProviderReference);
            Assert.Equal(storedResponse.CheckoutUrl, result.Value.CheckoutUrl);
            Assert.Equal(storedResponse.PaymentGatewayCode, result.Value.PaymentGatewayCode);
            Assert.Equal(storedResponse.PaymentStatus, result.Value.PaymentStatus);
            Assert.Equal(storedResponse.Amount, result.Value.Amount);
            Assert.Equal(storedResponse.Currency, result.Value.Currency);

            Assert.Empty(factory.DbContextFactory.Context.PaymentTransactions);

            factory.PaymentGatewayResolver.Verify(x => x.Resolve(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task InitiatePayment_WithProcessingIdempotencyRequest_ShouldReturnConflict()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            string userId = "user-123";
            string invoiceId = Guid.NewGuid().ToString();
            string idempotencyKey = "payment-key-001";

            InvoiceStatus unpaidInvoiceStatus = new InvoiceStatus
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid"
            };

            PaymentGateway paymentGateway = new PaymentGateway
            {
                Id = 1,
                Code = ApplicationConstants.PaymentGateways.Paystack,
                Name = "Paystack",
                Active = true
            };

            Invoice invoice = new Invoice
            {
                Id = invoiceId,
                PositionApplicationId = Guid.NewGuid().ToString(),
                InvoiceNumber = $"INV-{Guid.NewGuid():N}",
                StudentId = Guid.NewGuid(),
                UserId = userId,
                PayerFirstName = "Test",
                PayerLastName = "Student",
                PayerEmail = "student@example.com",
                RegistrationNumber = "REG001",
                Amount = 5000m,
                Currency = "NGN",
                InvoiceStatusId = unpaidInvoiceStatus.Id,
                InvoiceStatus = unpaidInvoiceStatus
            };

            string requestHash = CreatePaymentRequestHash(invoiceId, paymentGateway.Id);

            IdempotencyRecord idempotencyRecord = new IdempotencyRecord
            {
                Key = idempotencyKey,
                UserId = userId,
                Operation = ApplicationConstants.IdempotencyOperations.InitiatePayment,
                RequestHash = requestHash,
                Status = ApplicationConstants.IdempotencyStatuses.Processing
            };

            factory.DbContextFactory.Context.InvoiceStatuses.Add(unpaidInvoiceStatus);
            factory.DbContextFactory.Context.PaymentGateways.Add(paymentGateway);
            factory.DbContextFactory.Context.Invoices.Add(invoice);
            factory.DbContextFactory.Context.IdempotencyRecords.Add(idempotencyRecord);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            InitiatePaymentRequest request = new InitiatePaymentRequest
            {
                InvoiceId = invoiceId,
                PaymentGatewayId = paymentGateway.Id,
                IdempotencyKey = idempotencyKey
            };

            Result<InitiatePaymentResponse> result = await factory.Service.InitiatePayment(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("The payment initiation request is already being processed.", result.Error);
            Assert.Null(result.Value);

            Assert.Empty(factory.DbContextFactory.Context.PaymentTransactions);

            factory.PaymentGatewayResolver.Verify(x => x.Resolve(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task InitiatePayment_WithIdempotencyKeyUsedForDifferentRequest_ShouldReturnConflict()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            string userId = "user-123";
            string invoiceId = Guid.NewGuid().ToString();
            string idempotencyKey = "payment-key-001";

            InvoiceStatus unpaidInvoiceStatus = new InvoiceStatus
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid"
            };

            PaymentStatus pendingPaymentStatus = new PaymentStatus
            {
                Id = 1,
                Code = ApplicationConstants.PaymentStatuses.Pending,
                Name = "Pending"
            };

            PaymentStatus failedPaymentStatus = new PaymentStatus
            {
                Id = 2,
                Code = ApplicationConstants.PaymentStatuses.Failed,
                Name = "Failed"
            };

            PaymentGateway paymentGateway = new PaymentGateway
            {
                Id = 1,
                Code = ApplicationConstants.PaymentGateways.Paystack,
                Name = "Paystack",
                Active = true
            };

            Invoice invoice = CreateInvoice(invoiceId, userId, unpaidInvoiceStatus);

            IdempotencyRecord idempotencyRecord = new IdempotencyRecord
            {
                Key = idempotencyKey,
                UserId = userId,
                Operation = ApplicationConstants.IdempotencyOperations.InitiatePayment,
                RequestHash = CreatePaymentRequestHash(Guid.NewGuid().ToString(), paymentGateway.Id),
                Status = ApplicationConstants.IdempotencyStatuses.Processing
            };

            factory.DbContextFactory.Context.InvoiceStatuses.Add(unpaidInvoiceStatus);
            factory.DbContextFactory.Context.PaymentStatuses.AddRange(pendingPaymentStatus, failedPaymentStatus);
            factory.DbContextFactory.Context.PaymentGateways.Add(paymentGateway);
            factory.DbContextFactory.Context.Invoices.Add(invoice);
            factory.DbContextFactory.Context.IdempotencyRecords.Add(idempotencyRecord);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            Mock<IPaymentGateway> gateway = CreateGateway();

            factory.PaymentGatewayResolver.Setup(x => x.Resolve(ApplicationConstants.PaymentGateways.Paystack))
                .Returns(gateway.Object);

            InitiatePaymentRequest request = new InitiatePaymentRequest
            {
                InvoiceId = invoiceId,
                PaymentGatewayId = paymentGateway.Id,
                IdempotencyKey = idempotencyKey
            };

            Result<InitiatePaymentResponse> result = await factory.Service.InitiatePayment(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("The idempotency key has already been used for a different request.", result.Error);
            Assert.Null(result.Value);

            gateway.Verify(x => x.InitiatePayment(It.IsAny<GatewayPaymentInitializationRequest>()), Times.Never);
        }

        [Fact]
        public async Task InitiatePayment_WhenGatewayInitializationFails_ShouldMarkTransactionAndIdempotencyAsFailed()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            string userId = "user-123";
            string invoiceId = Guid.NewGuid().ToString();

            InvoiceStatus unpaidInvoiceStatus = new InvoiceStatus
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid"
            };

            PaymentStatus pendingPaymentStatus = new PaymentStatus
            {
                Id = 1,
                Code = ApplicationConstants.PaymentStatuses.Pending,
                Name = "Pending"
            };

            PaymentStatus failedPaymentStatus = new PaymentStatus
            {
                Id = 2,
                Code = ApplicationConstants.PaymentStatuses.Failed,
                Name = "Failed"
            };

            PaymentGateway paymentGateway = new PaymentGateway
            {
                Id = 1,
                Code = ApplicationConstants.PaymentGateways.Paystack,
                Name = "Paystack",
                Active = true
            };

            Invoice invoice = CreateInvoice(invoiceId, userId, unpaidInvoiceStatus);

            factory.DbContextFactory.Context.InvoiceStatuses.Add(unpaidInvoiceStatus);
            factory.DbContextFactory.Context.PaymentStatuses.AddRange(pendingPaymentStatus, failedPaymentStatus);
            factory.DbContextFactory.Context.PaymentGateways.Add(paymentGateway);
            factory.DbContextFactory.Context.Invoices.Add(invoice);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            Mock<IPaymentGateway> gateway = CreateGateway();

            factory.PaymentGatewayResolver.Setup(x => x.Resolve(ApplicationConstants.PaymentGateways.Paystack))
                .Returns(gateway.Object);

            SetupInitiatePaymentMappings(factory);

            gateway.Setup(x => x.InitiatePayment(It.IsAny<GatewayPaymentInitializationRequest>()))
                .ReturnsAsync(Result<GatewayPaymentInitializationResponse>.ValidationError("Gateway initialization failed."));

            InitiatePaymentRequest request = new InitiatePaymentRequest
            {
                InvoiceId = invoiceId,
                PaymentGatewayId = paymentGateway.Id,
                IdempotencyKey = "payment-key-001"
            };

            Result<InitiatePaymentResponse> result = await factory.Service.InitiatePayment(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Gateway initialization failed.", result.Error);
            Assert.Null(result.Value);

            PaymentTransaction paymentTransaction = factory.DbContextFactory.Context.PaymentTransactions.Single(x => x.InvoiceId == invoiceId);

            Assert.Equal(failedPaymentStatus.Id, paymentTransaction.PaymentStatusId);
            Assert.Equal("Gateway initialization failed.", paymentTransaction.FailureReason);

            IdempotencyRecord idempotencyRecord = factory.DbContextFactory.Context.IdempotencyRecords
                .Single(x => x.Key == request.IdempotencyKey);

            Assert.Equal(ApplicationConstants.IdempotencyStatuses.Failed, idempotencyRecord.Status);
            Assert.Equal(paymentTransaction.Id, idempotencyRecord.ResourceId);
        }

        [Fact]
        public async Task InitiatePayment_WithExistingPendingPaymentStillPending_ShouldReturnConflict()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            string userId = "user-123";
            string invoiceId = Guid.NewGuid().ToString();

            InvoiceStatus unpaidInvoiceStatus = new InvoiceStatus
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid"
            };

            PaymentStatus pendingPaymentStatus = new PaymentStatus
            {
                Id = 1,
                Code = ApplicationConstants.PaymentStatuses.Pending,
                Name = "Pending"
            };

            PaymentStatus failedPaymentStatus = new PaymentStatus
            {
                Id = 2,
                Code = ApplicationConstants.PaymentStatuses.Failed,
                Name = "Failed"
            };

            PaymentGateway paymentGateway = new PaymentGateway
            {
                Id = 1,
                Code = ApplicationConstants.PaymentGateways.Paystack,
                Name = "Paystack",
                Active = true
            };

            Invoice invoice = CreateInvoice(invoiceId, userId, unpaidInvoiceStatus);

            PaymentTransaction paymentTransaction = CreatePaymentTransaction(
                invoice,
                paymentGateway,
                pendingPaymentStatus,
                "PAY-123");

            factory.DbContextFactory.Context.InvoiceStatuses.Add(unpaidInvoiceStatus);
            factory.DbContextFactory.Context.PaymentStatuses.AddRange(pendingPaymentStatus, failedPaymentStatus);
            factory.DbContextFactory.Context.PaymentGateways.Add(paymentGateway);
            factory.DbContextFactory.Context.Invoices.Add(invoice);
            factory.DbContextFactory.Context.PaymentTransactions.Add(paymentTransaction);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.DbContextFactory.Context.ChangeTracker.Clear();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            Mock<IPaymentGateway> gateway = CreateGateway();

            factory.PaymentGatewayResolver.Setup(x => x.Resolve(ApplicationConstants.PaymentGateways.Paystack))
                .Returns(gateway.Object);

            SetupVerificationMappings(factory);

            gateway.Setup(x => x.VerifyPayment("PAY-123"))
                .ReturnsAsync(Result<GatewayPaymentVerificationResponse>.Success(
                    new GatewayPaymentVerificationResponse
                    {
                        PaymentReference = "PAY-123",
                        Status = ApplicationConstants.GatewayPaymentStatuses.Pending,
                        Amount = 5000m,
                        Currency = "NGN"
                    }));

            InitiatePaymentRequest request = new InitiatePaymentRequest
            {
                InvoiceId = invoiceId,
                PaymentGatewayId = paymentGateway.Id,
                IdempotencyKey = "payment-key-001"
            };

            Result<InitiatePaymentResponse> result = await factory.Service.InitiatePayment(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("A payment for this invoice is still being processed.", result.Error);
            Assert.Null(result.Value);

            gateway.Verify(x => x.VerifyPayment("PAY-123"), Times.Once);
            gateway.Verify(x => x.InitiatePayment(It.IsAny<GatewayPaymentInitializationRequest>()), Times.Never);
        }

        [Fact]
        public async Task VerifyPayment_WithSuccessfulPayment_ShouldReconcilePaymentInvoiceAndPositionApplication()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            string userId = "user-123";
            string invoiceId = Guid.NewGuid().ToString();
            string positionApplicationId = Guid.NewGuid().ToString();
            string paymentReference = "PAY-123";
            DateTime paidAt = new DateTime(2026, 10, 4, 12, 30, 0, DateTimeKind.Utc);

            InvoiceStatus unpaidInvoiceStatus = new InvoiceStatus
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid"
            };

            InvoiceStatus paidInvoiceStatus = new InvoiceStatus
            {
                Id = 2,
                Code = ApplicationConstants.InvoiceStatuses.Paid,
                Name = "Paid"
            };

            PaymentStatus pendingPaymentStatus = new PaymentStatus
            {
                Id = 1,
                Code = ApplicationConstants.PaymentStatuses.Pending,
                Name = "Pending"
            };

            PaymentStatus succeededPaymentStatus = new PaymentStatus
            {
                Id = 2,
                Code = ApplicationConstants.PaymentStatuses.Succeeded,
                Name = "Succeeded"
            };

            PositionApplicationStatus pendingPaymentApplicationStatus = new PositionApplicationStatus
            {
                Id = 1,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingPayment,
                Name = "Pending Payment"
            };

            PositionApplicationStatus pendingReviewStatus = new PositionApplicationStatus
            {
                Id = 2,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingReview,
                Name = "Pending Review"
            };

            PaymentGateway paymentGateway = new PaymentGateway
            {
                Id = 1,
                Code = ApplicationConstants.PaymentGateways.Paystack,
                Name = "Paystack",
                Active = true
            };

            PositionApplication positionApplication = new PositionApplication
            {
                Id = positionApplicationId,
                StudentId = Guid.NewGuid(),
                ElectionPositionId = Guid.NewGuid().ToString(),
                PositionApplicationStatusId = pendingPaymentApplicationStatus.Id,
                Active = true
            };

            Invoice invoice = CreateInvoice(invoiceId, userId, unpaidInvoiceStatus);
            invoice.PositionApplicationId = positionApplicationId;

            PaymentTransaction paymentTransaction = CreatePaymentTransaction(
                invoice,
                paymentGateway,
                pendingPaymentStatus,
                paymentReference);

            factory.DbContextFactory.Context.InvoiceStatuses.AddRange(unpaidInvoiceStatus, paidInvoiceStatus);
            factory.DbContextFactory.Context.PaymentStatuses.AddRange(pendingPaymentStatus, succeededPaymentStatus);
            factory.DbContextFactory.Context.PositionApplicationStatuses.AddRange(pendingPaymentApplicationStatus, pendingReviewStatus);
            factory.DbContextFactory.Context.PaymentGateways.Add(paymentGateway);
            factory.DbContextFactory.Context.PositionApplications.Add(positionApplication);
            factory.DbContextFactory.Context.Invoices.Add(invoice);
            factory.DbContextFactory.Context.PaymentTransactions.Add(paymentTransaction);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.DbContextFactory.Context.ChangeTracker.Clear();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            Mock<IPaymentGateway> gateway = CreateGateway();

            factory.PaymentGatewayResolver.Setup(x => x.Resolve(ApplicationConstants.PaymentGateways.Paystack))
                .Returns(gateway.Object);

            SetupVerificationMappings(factory);

            gateway.Setup(x => x.VerifyPayment(paymentReference))
                .ReturnsAsync(Result<GatewayPaymentVerificationResponse>.Success(
                    new GatewayPaymentVerificationResponse
                    {
                        PaymentReference = paymentReference,
                        ProviderReference = "PAYSTACK-123",
                        Status = ApplicationConstants.GatewayPaymentStatuses.Succeeded,
                        Amount = 5000m,
                        Currency = "NGN",
                        PaidAt = paidAt
                    }));

            Result<PaymentVerificationResponse> result = await factory.Service.VerifyPayment(paymentReference);

            Assert.True(result.IsSuccess, $"Status: {result.Status}, Error: {result.Error}");
            Assert.NotNull(result.Value);
            Assert.Equal(ApplicationConstants.GatewayPaymentStatuses.Succeeded, result.Value.PaymentStatus);
            Assert.Equal("PAYSTACK-123", result.Value.ProviderReference);
            Assert.Equal(paidAt, result.Value.PaidAt);

            PaymentTransaction savedPaymentTransaction = factory.DbContextFactory.Context.PaymentTransactions
                .Single(x => x.PaymentReference == paymentReference);

            Invoice savedInvoice = factory.DbContextFactory.Context.Invoices
                .Single(x => x.Id == invoiceId);

            PositionApplication savedPositionApplication = factory.DbContextFactory.Context.PositionApplications
                .Single(x => x.Id == positionApplicationId);

            Assert.Equal(succeededPaymentStatus.Id, savedPaymentTransaction.PaymentStatusId);
            Assert.Equal("PAYSTACK-123", savedPaymentTransaction.ProviderReference);
            Assert.Equal(paidAt, savedPaymentTransaction.PaidAt);

            Assert.Equal(paidInvoiceStatus.Id, savedInvoice.InvoiceStatusId);
            Assert.Equal(paidAt, savedInvoice.PaidAt);

            Assert.Equal(pendingReviewStatus.Id, savedPositionApplication.PositionApplicationStatusId);
        }

        [Fact]
        public async Task VerifyPayment_WithDifferentAmount_ShouldReturnConflictWithoutReconciliation()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            string userId = "user-123";
            string invoiceId = Guid.NewGuid().ToString();
            string paymentReference = "PAY-123";

            InvoiceStatus unpaidInvoiceStatus = new InvoiceStatus
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid"
            };

            PaymentStatus pendingPaymentStatus = new PaymentStatus
            {
                Id = 1,
                Code = ApplicationConstants.PaymentStatuses.Pending,
                Name = "Pending"
            };

            PaymentGateway paymentGateway = new PaymentGateway
            {
                Id = 1,
                Code = ApplicationConstants.PaymentGateways.Paystack,
                Name = "Paystack",
                Active = true
            };

            Invoice invoice = CreateInvoice(invoiceId, userId, unpaidInvoiceStatus);

            PaymentTransaction paymentTransaction = CreatePaymentTransaction(
                invoice,
                paymentGateway,
                pendingPaymentStatus,
                paymentReference);

            factory.DbContextFactory.Context.InvoiceStatuses.Add(unpaidInvoiceStatus);
            factory.DbContextFactory.Context.PaymentStatuses.Add(pendingPaymentStatus);
            factory.DbContextFactory.Context.PaymentGateways.Add(paymentGateway);
            factory.DbContextFactory.Context.Invoices.Add(invoice);
            factory.DbContextFactory.Context.PaymentTransactions.Add(paymentTransaction);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.DbContextFactory.Context.ChangeTracker.Clear();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            Mock<IPaymentGateway> gateway = CreateGateway();

            factory.PaymentGatewayResolver.Setup(x => x.Resolve(ApplicationConstants.PaymentGateways.Paystack))
                .Returns(gateway.Object);

            gateway.Setup(x => x.VerifyPayment(paymentReference))
                .ReturnsAsync(Result<GatewayPaymentVerificationResponse>.Success(
                    new GatewayPaymentVerificationResponse
                    {
                        PaymentReference = paymentReference,
                        Status = ApplicationConstants.GatewayPaymentStatuses.Succeeded,
                        Amount = 6000m,
                        Currency = "NGN"
                    }));

            Result<PaymentVerificationResponse> result = await factory.Service.VerifyPayment(paymentReference);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("The verified payment amount does not match the expected amount.", result.Error);
            Assert.Null(result.Value);

            PaymentTransaction savedPaymentTransaction = factory.DbContextFactory.Context.PaymentTransactions
                .Single(x => x.PaymentReference == paymentReference);

            Invoice savedInvoice = factory.DbContextFactory.Context.Invoices
                .Single(x => x.Id == invoiceId);

            Assert.Equal(pendingPaymentStatus.Id, savedPaymentTransaction.PaymentStatusId);
            Assert.Equal(unpaidInvoiceStatus.Id, savedInvoice.InvoiceStatusId);
            Assert.Null(savedPaymentTransaction.PaidAt);
            Assert.Null(savedInvoice.PaidAt);
        }

        [Fact]
        public async Task VerifyPayment_WhenPaymentBelongsToAnotherUser_ShouldReturnForbidden()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            string ownerUserId = "owner-user";
            string currentUserId = "another-user";
            string invoiceId = Guid.NewGuid().ToString();
            string paymentReference = "PAY-123";

            InvoiceStatus unpaidInvoiceStatus = new InvoiceStatus
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid"
            };

            PaymentStatus pendingPaymentStatus = new PaymentStatus
            {
                Id = 1,
                Code = ApplicationConstants.PaymentStatuses.Pending,
                Name = "Pending"
            };

            PaymentGateway paymentGateway = new PaymentGateway
            {
                Id = 1,
                Code = ApplicationConstants.PaymentGateways.Paystack,
                Name = "Paystack",
                Active = true
            };

            Invoice invoice = CreateInvoice(invoiceId, ownerUserId, unpaidInvoiceStatus);

            PaymentTransaction paymentTransaction = CreatePaymentTransaction(
                invoice,
                paymentGateway,
                pendingPaymentStatus,
                paymentReference);

            factory.DbContextFactory.Context.InvoiceStatuses.Add(unpaidInvoiceStatus);
            factory.DbContextFactory.Context.PaymentStatuses.Add(pendingPaymentStatus);
            factory.DbContextFactory.Context.PaymentGateways.Add(paymentGateway);
            factory.DbContextFactory.Context.Invoices.Add(invoice);
            factory.DbContextFactory.Context.PaymentTransactions.Add(paymentTransaction);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.DbContextFactory.Context.ChangeTracker.Clear();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(currentUserId);

            Result<PaymentVerificationResponse> result = await factory.Service.VerifyPayment(paymentReference);

            Assert.Equal(ResultStatus.Forbidden, result.Status);
            Assert.Equal("You are not allowed to verify this payment.", result.Error);
            Assert.Null(result.Value);

            factory.PaymentGatewayResolver.Verify(x => x.Resolve(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPaymentWebhook_WithNonSuccessfulChargeEvent_ShouldAcknowledgeWithoutVerification()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            Mock<IPaymentGateway> gateway = CreateGateway();

            factory.PaymentGatewayResolver.Setup(x => x.Resolve(ApplicationConstants.PaymentGateways.Paystack))
                .Returns(gateway.Object);

            gateway.Setup(x => x.ProcessWebhook("payload", "signature"))
                .Returns(Result<GatewayPaymentWebhookResponse>.Success(
                    new GatewayPaymentWebhookResponse
                    {
                        Event = "charge.failed",
                        PaymentReference = "PAY-123"
                    }));

            Result<string> result = await factory.Service.ProcessPaymentWebhook(
                ApplicationConstants.PaymentGateways.Paystack,
                "payload",
                "signature");

            Assert.True(result.IsSuccess);
            Assert.Equal("Webhook received.", result.Value);

            gateway.Verify(x => x.VerifyPayment(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPaymentWebhook_WithUnknownPaymentReference_ShouldAcknowledgeWithoutVerification()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            Mock<IPaymentGateway> gateway = CreateGateway();

            factory.PaymentGatewayResolver.Setup(x => x.Resolve(ApplicationConstants.PaymentGateways.Paystack))
                .Returns(gateway.Object);

            gateway.Setup(x => x.ProcessWebhook("payload", "signature"))
                .Returns(Result<GatewayPaymentWebhookResponse>.Success(
                    new GatewayPaymentWebhookResponse
                    {
                        Event = "charge.success",
                        PaymentReference = "UNKNOWN-PAYMENT"
                    }));

            Result<string> result = await factory.Service.ProcessPaymentWebhook(
                ApplicationConstants.PaymentGateways.Paystack,
                "payload",
                "signature");

            Assert.True(result.IsSuccess);
            Assert.Equal("Webhook received.", result.Value);

            gateway.Verify(x => x.VerifyPayment(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPaymentWebhook_WithAlreadySuccessfulPayment_ShouldAcknowledgeWithoutProviderVerification()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            string userId = "user-123";
            string invoiceId = Guid.NewGuid().ToString();
            string paymentReference = "PAY-123";

            InvoiceStatus paidInvoiceStatus = new InvoiceStatus
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Paid,
                Name = "Paid"
            };

            PaymentStatus succeededPaymentStatus = new PaymentStatus
            {
                Id = 1,
                Code = ApplicationConstants.PaymentStatuses.Succeeded,
                Name = "Succeeded"
            };

            PaymentGateway paymentGateway = new PaymentGateway
            {
                Id = 1,
                Code = ApplicationConstants.PaymentGateways.Paystack,
                Name = "Paystack",
                Active = true
            };

            Invoice invoice = CreateInvoice(invoiceId, userId, paidInvoiceStatus);
            invoice.PaidAt = DateTime.UtcNow;

            PaymentTransaction paymentTransaction = CreatePaymentTransaction(
                invoice,
                paymentGateway,
                succeededPaymentStatus,
                paymentReference);

            paymentTransaction.PaidAt = invoice.PaidAt;

            factory.DbContextFactory.Context.InvoiceStatuses.Add(paidInvoiceStatus);
            factory.DbContextFactory.Context.PaymentStatuses.Add(succeededPaymentStatus);
            factory.DbContextFactory.Context.PaymentGateways.Add(paymentGateway);
            factory.DbContextFactory.Context.Invoices.Add(invoice);
            factory.DbContextFactory.Context.PaymentTransactions.Add(paymentTransaction);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.DbContextFactory.Context.ChangeTracker.Clear();

            Mock<IPaymentGateway> gateway = CreateGateway();

            factory.PaymentGatewayResolver.Setup(x => x.Resolve(ApplicationConstants.PaymentGateways.Paystack))
                .Returns(gateway.Object);

            gateway.Setup(x => x.ProcessWebhook("payload", "signature"))
                .Returns(Result<GatewayPaymentWebhookResponse>.Success(
                    new GatewayPaymentWebhookResponse
                    {
                        Event = "charge.success",
                        PaymentReference = paymentReference
                    }));

            Result<string> result = await factory.Service.ProcessPaymentWebhook(
                ApplicationConstants.PaymentGateways.Paystack,
                "payload",
                "signature");

            Assert.True(result.IsSuccess);
            Assert.Equal("Webhook already processed.", result.Value);

            gateway.Verify(x => x.VerifyPayment(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPaymentWebhook_WithSuccessfulPayment_ShouldReconcilePayment()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            string userId = "user-123";
            string invoiceId = Guid.NewGuid().ToString();
            string positionApplicationId = Guid.NewGuid().ToString();
            string paymentReference = "PAY-123";
            DateTime paidAt = new DateTime(2026, 10, 4, 14, 0, 0, DateTimeKind.Utc);

            InvoiceStatus unpaidInvoiceStatus = new InvoiceStatus
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid"
            };

            InvoiceStatus paidInvoiceStatus = new InvoiceStatus
            {
                Id = 2,
                Code = ApplicationConstants.InvoiceStatuses.Paid,
                Name = "Paid"
            };

            PaymentStatus pendingPaymentStatus = new PaymentStatus
            {
                Id = 1,
                Code = ApplicationConstants.PaymentStatuses.Pending,
                Name = "Pending"
            };

            PaymentStatus succeededPaymentStatus = new PaymentStatus
            {
                Id = 2,
                Code = ApplicationConstants.PaymentStatuses.Succeeded,
                Name = "Succeeded"
            };

            PositionApplicationStatus pendingPaymentApplicationStatus = new PositionApplicationStatus
            {
                Id = 1,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingPayment,
                Name = "Pending Payment"
            };

            PositionApplicationStatus pendingReviewStatus = new PositionApplicationStatus
            {
                Id = 2,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingReview,
                Name = "Pending Review"
            };

            PaymentGateway paymentGateway = new PaymentGateway
            {
                Id = 1,
                Code = ApplicationConstants.PaymentGateways.Paystack,
                Name = "Paystack",
                Active = true
            };

            PositionApplication positionApplication = new PositionApplication
            {
                Id = positionApplicationId,
                StudentId = Guid.NewGuid(),
                ElectionPositionId = Guid.NewGuid().ToString(),
                PositionApplicationStatusId = pendingPaymentApplicationStatus.Id,
                Active = true
            };

            Invoice invoice = CreateInvoice(invoiceId, userId, unpaidInvoiceStatus);
            invoice.PositionApplicationId = positionApplicationId;

            PaymentTransaction paymentTransaction = CreatePaymentTransaction(
                invoice,
                paymentGateway,
                pendingPaymentStatus,
                paymentReference);

            factory.DbContextFactory.Context.InvoiceStatuses.AddRange(unpaidInvoiceStatus, paidInvoiceStatus);
            factory.DbContextFactory.Context.PaymentStatuses.AddRange(pendingPaymentStatus, succeededPaymentStatus);
            factory.DbContextFactory.Context.PositionApplicationStatuses.AddRange(pendingPaymentApplicationStatus, pendingReviewStatus);
            factory.DbContextFactory.Context.PaymentGateways.Add(paymentGateway);
            factory.DbContextFactory.Context.PositionApplications.Add(positionApplication);
            factory.DbContextFactory.Context.Invoices.Add(invoice);
            factory.DbContextFactory.Context.PaymentTransactions.Add(paymentTransaction);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.DbContextFactory.Context.ChangeTracker.Clear();

            Mock<IPaymentGateway> gateway = CreateGateway();

            factory.PaymentGatewayResolver.Setup(x => x.Resolve(ApplicationConstants.PaymentGateways.Paystack))
                .Returns(gateway.Object);

            SetupVerificationMappings(factory);

            gateway.Setup(x => x.ProcessWebhook("payload", "signature"))
                .Returns(Result<GatewayPaymentWebhookResponse>.Success(
                    new GatewayPaymentWebhookResponse
                    {
                        Event = "charge.success",
                        PaymentReference = paymentReference
                    }));

            gateway.Setup(x => x.VerifyPayment(paymentReference))
                .ReturnsAsync(Result<GatewayPaymentVerificationResponse>.Success(
                    new GatewayPaymentVerificationResponse
                    {
                        PaymentReference = paymentReference,
                        ProviderReference = "PAYSTACK-123",
                        Status = ApplicationConstants.GatewayPaymentStatuses.Succeeded,
                        Amount = 5000m,
                        Currency = "NGN",
                        PaidAt = paidAt
                    }));

            Result<string> result = await factory.Service.ProcessPaymentWebhook(ApplicationConstants.PaymentGateways.Paystack, "payload", "signature");

            Assert.True(result.IsSuccess);

            PaymentTransaction savedPaymentTransaction = factory.DbContextFactory.Context.PaymentTransactions
                .Single(x => x.PaymentReference == paymentReference);

            Invoice savedInvoice = factory.DbContextFactory.Context.Invoices
                .Single(x => x.Id == invoiceId);

            PositionApplication savedPositionApplication = factory.DbContextFactory.Context.PositionApplications
                .Single(x => x.Id == positionApplicationId);

            Assert.Equal(succeededPaymentStatus.Id, savedPaymentTransaction.PaymentStatusId);
            Assert.Equal(paidInvoiceStatus.Id, savedInvoice.InvoiceStatusId);
            Assert.Equal(pendingReviewStatus.Id, savedPositionApplication.PositionApplicationStatusId);
            Assert.Equal(paidAt, savedPaymentTransaction.PaidAt);
            Assert.Equal(paidAt, savedInvoice.PaidAt);

            gateway.Verify(x => x.VerifyPayment(paymentReference), Times.Once);
        }

        [Fact]
        public async Task GetInvoice_ReturnsInvoice_WhenInvoiceExists()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            string invoiceId = Guid.NewGuid().ToString();

            InvoiceStatus invoiceStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid"
            };

            Invoice invoice = new()
            {
                Id = invoiceId,
                PositionApplicationId = Guid.NewGuid().ToString(),
                InvoiceNumber = "INV-001",
                StudentId = Guid.NewGuid(),
                UserId = "user-123",
                PayerFirstName = "Test",
                PayerLastName = "Student",
                PayerEmail = "student@example.com",
                RegistrationNumber = "REG001",
                Amount = 5000m,
                Currency = "NGN",
                InvoiceStatusId = invoiceStatus.Id,
                InvoiceStatus = invoiceStatus,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            factory.DbContextFactory.Context.InvoiceStatuses.Add(invoiceStatus);
            factory.DbContextFactory.Context.Invoices.Add(invoice);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(x => x.Map<InvoiceResponse>(It.IsAny<Invoice>()))
                .Returns((Invoice source) => new InvoiceResponse
                {
                    Id = source.Id,
                    InvoiceNumber = source.InvoiceNumber,
                    Amount = source.Amount,
                    Currency = source.Currency,
                    Status = source.InvoiceStatus.Name
                });

            Result<InvoiceResponse> result = await factory.Service.GetInvoice(invoiceId);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(invoice.Id, result.Value.Id);
            Assert.Equal(invoice.InvoiceNumber, result.Value.InvoiceNumber);
            Assert.Equal(invoice.Amount, result.Value.Amount);
            Assert.Equal(invoice.Currency, result.Value.Currency);
            Assert.Equal(invoiceStatus.Name, result.Value.Status);
        }

        [Fact]
        public async Task GetInvoice_ReturnsNotFound_WhenInvoiceDoesNotExist()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            string invoiceId = Guid.NewGuid().ToString();

            Result<InvoiceResponse> result = await factory.Service.GetInvoice(invoiceId);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Null(result.Value);
            Assert.Equal($"Invoice with id {invoiceId} was not found.", result.Error);

            factory.Mapper.Verify(x => x.Map<InvoiceResponse>(It.IsAny<Invoice>()), Times.Never);
        }

        [Fact]
        public async Task GetInvoices_ReturnsInvoices()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            InvoiceStatus invoiceStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid"
            };

            Invoice firstInvoice = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = Guid.NewGuid().ToString(),
                InvoiceNumber = "INV-001",
                StudentId = Guid.NewGuid(),
                UserId = "user-001",
                PayerFirstName = "John",
                PayerLastName = "Doe",
                PayerEmail = "john@example.com",
                RegistrationNumber = "REG001",
                Amount = 5000m,
                Currency = "NGN",
                InvoiceStatusId = invoiceStatus.Id,
                InvoiceStatus = invoiceStatus,
                CreatedAt = DateTime.UtcNow
            };

            Invoice secondInvoice = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = Guid.NewGuid().ToString(),
                InvoiceNumber = "INV-002",
                StudentId = Guid.NewGuid(),
                UserId = "user-002",
                PayerFirstName = "Jane",
                PayerLastName = "Doe",
                PayerEmail = "jane@example.com",
                RegistrationNumber = "REG002",
                Amount = 3000m,
                Currency = "NGN",
                InvoiceStatusId = invoiceStatus.Id,
                InvoiceStatus = invoiceStatus,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            };

            factory.DbContextFactory.Context.InvoiceStatuses.Add(invoiceStatus);
            factory.DbContextFactory.Context.Invoices.AddRange(firstInvoice, secondInvoice);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            SetupInvoicePagedMapping(factory);

            InvoiceRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<InvoiceResponse>> result = await factory.Service.GetInvoices(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Items.Count());
        }

        [Fact]
        public async Task GetInvoices_WithInvoiceStatusId_ShouldReturnFilteredInvoices()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            InvoiceStatus unpaidStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid"
            };

            InvoiceStatus paidStatus = new()
            {
                Id = 2,
                Code = ApplicationConstants.InvoiceStatuses.Paid,
                Name = "Paid"
            };

            Invoice unpaidInvoice = CreateInvoice(unpaidStatus, "INV-UNPAID-001");
            Invoice paidInvoice = CreateInvoice(paidStatus, "INV-PAID-001");

            factory.DbContextFactory.Context.InvoiceStatuses.AddRange(unpaidStatus, paidStatus);
            factory.DbContextFactory.Context.Invoices.AddRange(unpaidInvoice, paidInvoice);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            SetupInvoicePagedMapping(factory);

            InvoiceRequest request = new()
            {
                InvoiceStatusId = paidStatus.Id,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<InvoiceResponse>> result = await factory.Service.GetInvoices(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items);

            InvoiceResponse invoice = result.Value.Items.Single();

            Assert.Equal(paidInvoice.Id, invoice.Id);
            Assert.Equal("Paid", invoice.Status);
        }

        [Fact]
        public async Task GetInvoices_WithStudentId_ShouldReturnFilteredInvoices()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            InvoiceStatus invoiceStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid"
            };

            Guid matchingStudentId = Guid.NewGuid();

            Invoice matchingInvoice = CreateInvoice(invoiceStatus, "INV-STUDENT-001", matchingStudentId);
            Invoice nonMatchingInvoice = CreateInvoice(invoiceStatus, "INV-STUDENT-002", Guid.NewGuid());

            factory.DbContextFactory.Context.InvoiceStatuses.Add(invoiceStatus);
            factory.DbContextFactory.Context.Invoices.AddRange(matchingInvoice, nonMatchingInvoice);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            SetupInvoicePagedMapping(factory);

            InvoiceRequest request = new()
            {
                StudentId = matchingStudentId,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<InvoiceResponse>> result = await factory.Service.GetInvoices(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items);
            Assert.Equal(matchingInvoice.Id, result.Value.Items.Single().Id);
        }

        [Fact]
        public async Task GetInvoices_WithPositionApplicationId_ShouldReturnFilteredInvoices()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            InvoiceStatus invoiceStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid"
            };

            string positionApplicationId = Guid.NewGuid().ToString();

            Invoice matchingInvoice = CreateInvoice(invoiceStatus, "INV-APP-001");
            matchingInvoice.PositionApplicationId = positionApplicationId;

            Invoice nonMatchingInvoice = CreateInvoice(invoiceStatus, "INV-APP-002");

            factory.DbContextFactory.Context.InvoiceStatuses.Add(invoiceStatus);
            factory.DbContextFactory.Context.Invoices.AddRange(matchingInvoice, nonMatchingInvoice);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            SetupInvoicePagedMapping(factory);

            InvoiceRequest request = new()
            {
                PositionApplicationId = positionApplicationId,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<InvoiceResponse>> result = await factory.Service.GetInvoices(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items);
            Assert.Equal(matchingInvoice.Id, result.Value.Items.Single().Id);
        }

        [Fact]
        public async Task GetInvoices_WithSearchTerm_ShouldReturnMatchingInvoices()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            InvoiceStatus invoiceStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid"
            };

            Invoice matchingInvoice = CreateInvoice(invoiceStatus, "INV-SEARCH-001");
            matchingInvoice.PayerFirstName = "Obinna";
            matchingInvoice.PayerLastName = "Achara";
            matchingInvoice.RegistrationNumber = "REG-SEARCH-001";

            Invoice nonMatchingInvoice = CreateInvoice(invoiceStatus, "INV-OTHER-001");
            nonMatchingInvoice.PayerFirstName = "Another";
            nonMatchingInvoice.PayerLastName = "Student";
            nonMatchingInvoice.RegistrationNumber = "REG-OTHER-001";

            factory.DbContextFactory.Context.InvoiceStatuses.Add(invoiceStatus);
            factory.DbContextFactory.Context.Invoices.AddRange(matchingInvoice, nonMatchingInvoice);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            SetupInvoicePagedMapping(factory);

            InvoiceRequest request = new()
            {
                SearchTerm = "Obinna",
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<InvoiceResponse>> result = await factory.Service.GetInvoices(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items);
            Assert.Equal(matchingInvoice.Id, result.Value.Items.Single().Id);
        }

        [Fact]
        public async Task GetPaymentTransactions_ReturnsPaymentTransactions()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            (PaymentGateway paymentGateway, PaymentStatus paymentStatus) = await CreatePaymentTransactionLookupData(factory);

            PaymentTransaction firstPaymentTransaction = CreatePaymentTransaction(paymentGateway, paymentStatus, "PAY-001");
            PaymentTransaction secondPaymentTransaction = CreatePaymentTransaction(paymentGateway, paymentStatus, "PAY-002");

            factory.DbContextFactory.Context.PaymentTransactions.AddRange(firstPaymentTransaction, secondPaymentTransaction);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            SetupPaymentTransactionPagedMapping(factory);

            PaymentTransactionRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<PaymentTransactionResponse>> result = await factory.Service.GetPaymentTransactions(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Equal(2, result.Value.Items.Count());
        }

        [Fact]
        public async Task GetPaymentTransactions_WithPaymentGatewayId_ShouldReturnFilteredPaymentTransactions()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            PaymentGateway paystackGateway = new()
            {
                Id = 1,
                Code = ApplicationConstants.PaymentGateways.Paystack,
                Name = "Paystack",
                Active = true
            };

            PaymentGateway secondGateway = new()
            {
                Id = 2,
                Code = "SECOND_GATEWAY",
                Name = "Second Gateway",
                Active = true
            };

            PaymentStatus paymentStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.PaymentStatuses.Pending,
                Name = "Pending"
            };

            PaymentTransaction matchingPaymentTransaction = CreatePaymentTransaction(paystackGateway, paymentStatus, "PAY-GATEWAY-001");
            PaymentTransaction nonMatchingPaymentTransaction = CreatePaymentTransaction(secondGateway, paymentStatus, "PAY-GATEWAY-002");

            factory.DbContextFactory.Context.PaymentGateways.AddRange(paystackGateway, secondGateway);
            factory.DbContextFactory.Context.PaymentStatuses.Add(paymentStatus);
            factory.DbContextFactory.Context.PaymentTransactions.AddRange(matchingPaymentTransaction, nonMatchingPaymentTransaction);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            SetupPaymentTransactionPagedMapping(factory);

            PaymentTransactionRequest request = new()
            {
                PaymentGatewayId = paystackGateway.Id,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<PaymentTransactionResponse>> result = await factory.Service.GetPaymentTransactions(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);
            Assert.Equal(matchingPaymentTransaction.Id, result.Value.Items.Single().PaymentTransactionId);
        }

        [Fact]
        public async Task GetPaymentTransactions_WithPaymentStatusId_ShouldReturnFilteredPaymentTransactions()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            PaymentGateway paymentGateway = new()
            {
                Id = 1,
                Code = ApplicationConstants.PaymentGateways.Paystack,
                Name = "Paystack",
                Active = true
            };

            PaymentStatus pendingStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.PaymentStatuses.Pending,
                Name = "Pending"
            };

            PaymentStatus succeededStatus = new()
            {
                Id = 2,
                Code = ApplicationConstants.PaymentStatuses.Succeeded,
                Name = "Succeeded"
            };

            PaymentTransaction matchingPaymentTransaction = CreatePaymentTransaction(paymentGateway, succeededStatus, "PAY-STATUS-001");
            PaymentTransaction nonMatchingPaymentTransaction = CreatePaymentTransaction(paymentGateway, pendingStatus, "PAY-STATUS-002");

            factory.DbContextFactory.Context.PaymentGateways.Add(paymentGateway);
            factory.DbContextFactory.Context.PaymentStatuses.AddRange(pendingStatus, succeededStatus);
            factory.DbContextFactory.Context.PaymentTransactions.AddRange(matchingPaymentTransaction, nonMatchingPaymentTransaction);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            SetupPaymentTransactionPagedMapping(factory);

            PaymentTransactionRequest request = new()
            {
                PaymentStatusId = succeededStatus.Id,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<PaymentTransactionResponse>> result = await factory.Service.GetPaymentTransactions(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);
            Assert.Equal(matchingPaymentTransaction.Id, result.Value.Items.Single().PaymentTransactionId);
        }

        [Fact]
        public async Task GetPaymentTransactions_WithInvoiceId_ShouldReturnFilteredPaymentTransactions()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            (PaymentGateway paymentGateway, PaymentStatus paymentStatus) = await CreatePaymentTransactionLookupData(factory);

            string invoiceId = Guid.NewGuid().ToString();

            PaymentTransaction matchingPaymentTransaction = CreatePaymentTransaction(paymentGateway, paymentStatus, "PAY-INVOICE-001");
            matchingPaymentTransaction.InvoiceId = invoiceId;

            PaymentTransaction nonMatchingPaymentTransaction = CreatePaymentTransaction(paymentGateway, paymentStatus, "PAY-INVOICE-002");

            factory.DbContextFactory.Context.PaymentTransactions.AddRange(matchingPaymentTransaction, nonMatchingPaymentTransaction);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            SetupPaymentTransactionPagedMapping(factory);

            PaymentTransactionRequest request = new()
            {
                InvoiceId = invoiceId,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<PaymentTransactionResponse>> result = await factory.Service.GetPaymentTransactions(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);
            Assert.Equal(matchingPaymentTransaction.Id, result.Value.Items.Single().PaymentTransactionId);
        }

        [Fact]
        public async Task GetPaymentTransactions_WithStudentId_ShouldReturnFilteredPaymentTransactions()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            (PaymentGateway paymentGateway, PaymentStatus paymentStatus) = await CreatePaymentTransactionLookupData(factory);

            Guid studentId = Guid.NewGuid();

            PaymentTransaction matchingPaymentTransaction = CreatePaymentTransaction(paymentGateway, paymentStatus, "PAY-STUDENT-001");
            matchingPaymentTransaction.StudentId = studentId;

            PaymentTransaction nonMatchingPaymentTransaction = CreatePaymentTransaction(paymentGateway, paymentStatus, "PAY-STUDENT-002");

            factory.DbContextFactory.Context.PaymentTransactions.AddRange(matchingPaymentTransaction, nonMatchingPaymentTransaction);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            SetupPaymentTransactionPagedMapping(factory);

            PaymentTransactionRequest request = new()
            {
                StudentId = studentId,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<PaymentTransactionResponse>> result = await factory.Service.GetPaymentTransactions(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);
            Assert.Equal(matchingPaymentTransaction.Id, result.Value.Items.Single().PaymentTransactionId);
        }

        [Fact]
        public async Task GetPaymentTransactions_WithUserId_ShouldReturnFilteredPaymentTransactions()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            (PaymentGateway paymentGateway, PaymentStatus paymentStatus) = await CreatePaymentTransactionLookupData(factory);

            string userId = "user-123";

            PaymentTransaction matchingPaymentTransaction = CreatePaymentTransaction(paymentGateway, paymentStatus, "PAY-USER-001");
            matchingPaymentTransaction.UserId = userId;

            PaymentTransaction nonMatchingPaymentTransaction = CreatePaymentTransaction(paymentGateway, paymentStatus, "PAY-USER-002");

            factory.DbContextFactory.Context.PaymentTransactions.AddRange(matchingPaymentTransaction, nonMatchingPaymentTransaction);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            SetupPaymentTransactionPagedMapping(factory);

            PaymentTransactionRequest request = new()
            {
                UserId = userId,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<PaymentTransactionResponse>> result = await factory.Service.GetPaymentTransactions(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);
            Assert.Equal(matchingPaymentTransaction.Id, result.Value.Items.Single().PaymentTransactionId);
        }

        [Fact]
        public async Task GetPaymentTransactions_WithSearchTerm_ShouldReturnMatchingPaymentTransactions()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            (PaymentGateway paymentGateway, PaymentStatus paymentStatus) = await CreatePaymentTransactionLookupData(factory);

            PaymentTransaction matchingPaymentTransaction = CreatePaymentTransaction(paymentGateway, paymentStatus, "PAY-SEARCH-001");
            matchingPaymentTransaction.PayerFirstName = "Obinna";
            matchingPaymentTransaction.PayerLastName = "Achara";
            matchingPaymentTransaction.RegistrationNumber = "REG-SEARCH-001";

            PaymentTransaction nonMatchingPaymentTransaction = CreatePaymentTransaction(paymentGateway, paymentStatus, "PAY-OTHER-001");
            nonMatchingPaymentTransaction.PayerFirstName = "Another";
            nonMatchingPaymentTransaction.PayerLastName = "Student";
            nonMatchingPaymentTransaction.RegistrationNumber = "REG-OTHER-001";

            factory.DbContextFactory.Context.PaymentTransactions.AddRange(matchingPaymentTransaction, nonMatchingPaymentTransaction);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            SetupPaymentTransactionPagedMapping(factory);

            PaymentTransactionRequest request = new()
            {
                SearchTerm = "Obinna",
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<PaymentTransactionResponse>> result = await factory.Service.GetPaymentTransactions(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);
            Assert.Equal(matchingPaymentTransaction.Id, result.Value.Items.Single().PaymentTransactionId);
        }

        [Fact]
        public async Task GetPaymentTransaction_ReturnsPaymentTransaction_WhenPaymentTransactionExists()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            (PaymentGateway paymentGateway, PaymentStatus paymentStatus) = await CreatePaymentTransactionLookupData(factory);

            PaymentTransaction paymentTransaction = CreatePaymentTransaction(paymentGateway, paymentStatus, "PAY-SINGLE-001");

            factory.DbContextFactory.Context.PaymentTransactions.Add(paymentTransaction);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(x => x.Map<PaymentTransactionResponse>(It.IsAny<PaymentTransaction>()))
                .Returns((PaymentTransaction source) => new PaymentTransactionResponse
                {
                    PaymentTransactionId = source.Id,
                    InvoiceId = source.InvoiceId,
                    PaymentReference = source.PaymentReference,
                    ProviderReference = source.ProviderReference,
                    PaymentGateway = source.PaymentGateway.Name,
                    PaymentStatus = source.PaymentStatus.Name,
                    StudentId = source.StudentId,
                    UserId = source.UserId,
                    PayerFirstName = source.PayerFirstName,
                    PayerLastName = source.PayerLastName,
                    PayerEmail = source.PayerEmail,
                    RegistrationNumber = source.RegistrationNumber,
                    Amount = source.Amount,
                    Currency = source.Currency,
                    FailureReason = source.FailureReason,
                    PaidAt = source.PaidAt,
                    CreatedAt = source.CreatedAt
                });

            Result<PaymentTransactionResponse> result = await factory.Service.GetPaymentTransaction(paymentTransaction.PaymentReference);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(paymentTransaction.Id, result.Value.PaymentTransactionId);
            Assert.Equal(paymentTransaction.PaymentReference, result.Value.PaymentReference);
            Assert.Equal(paymentGateway.Name, result.Value.PaymentGateway);
            Assert.Equal(paymentStatus.Name, result.Value.PaymentStatus);
        }

        [Fact]
        public async Task GetPaymentTransaction_ReturnsNotFound_WhenPaymentTransactionDoesNotExist()
        {
            using PaymentServiceFactory factory = new PaymentServiceFactory();

            string paymentReference = "PAY-NOT-FOUND";

            Result<PaymentTransactionResponse> result = await factory.Service.GetPaymentTransaction(paymentReference);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Null(result.Value);
            Assert.Equal($"Payment transaction with reference {paymentReference} was not found.", result.Error);
        }

        private static Invoice CreateInvoice(InvoiceStatus invoiceStatus, string invoiceNumber, Guid? studentId = null)
        {
            return new Invoice
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = Guid.NewGuid().ToString(),
                InvoiceNumber = invoiceNumber,
                StudentId = studentId ?? Guid.NewGuid(),
                UserId = Guid.NewGuid().ToString(),
                PayerFirstName = "Test",
                PayerLastName = "Student",
                PayerEmail = "student@example.com",
                RegistrationNumber = $"REG-{Guid.NewGuid():N}",
                Amount = 5000m,
                Currency = "NGN",
                InvoiceStatusId = invoiceStatus.Id,
                InvoiceStatus = invoiceStatus,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private static void SetupInvoicePagedMapping(PaymentServiceFactory factory)
        {
            factory.Mapper.Setup(x => x.Map<PagedResponse<InvoiceResponse>>(It.IsAny<PagedList<Invoice>>()))
                .Returns((PagedList<Invoice> invoices) => new PagedResponse<InvoiceResponse>
                {
                    Items = invoices.Select(invoice => new InvoiceResponse
                    {
                        Id = invoice.Id,
                        InvoiceNumber = invoice.InvoiceNumber,
                        Amount = invoice.Amount,
                        Currency = invoice.Currency,
                        Status = invoice.InvoiceStatus.Name
                    }),
                    MetaData = invoices.MetaData
                });
        }

        private static string CreatePaymentRequestHash(string invoiceId, int paymentGatewayId)
        {
            string requestValue = $"{invoiceId}:{paymentGatewayId}";
            byte[] requestBytes = Encoding.UTF8.GetBytes(requestValue);
            byte[] hashBytes = SHA256.HashData(requestBytes);

            return Convert.ToHexString(hashBytes);
        }

        private static Invoice CreateInvoice(string invoiceId, string userId, InvoiceStatus invoiceStatus)
        {
            return new Invoice
            {
                Id = invoiceId,
                PositionApplicationId = Guid.NewGuid().ToString(),
                InvoiceNumber = $"INV-{Guid.NewGuid():N}",
                StudentId = Guid.NewGuid(),
                UserId = userId,
                PayerFirstName = "Test",
                PayerLastName = "Student",
                PayerEmail = "student@example.com",
                RegistrationNumber = "REG001",
                Amount = 5000m,
                Currency = "NGN",
                InvoiceStatusId = invoiceStatus.Id,
                InvoiceStatus = invoiceStatus
            };
        }

        private static PaymentTransaction CreatePaymentTransaction(Invoice invoice, PaymentGateway paymentGateway,
            PaymentStatus paymentStatus, string paymentReference)
        {
            return new PaymentTransaction
            {
                InvoiceId = invoice.Id,
                PaymentGatewayId = paymentGateway.Id,
                PaymentStatusId = paymentStatus.Id,
                PaymentReference = paymentReference,
                StudentId = invoice.StudentId,
                UserId = invoice.UserId,
                PayerFirstName = invoice.PayerFirstName,
                PayerLastName = invoice.PayerLastName,
                PayerEmail = invoice.PayerEmail,
                RegistrationNumber = invoice.RegistrationNumber,
                Amount = invoice.Amount,
                Currency = invoice.Currency
            };
        }

        private static async Task<(PaymentGateway PaymentGateway, PaymentStatus PaymentStatus)> CreatePaymentTransactionLookupData(PaymentServiceFactory factory)
        {
            PaymentGateway paymentGateway = new()
            {
                Id = 1,
                Code = ApplicationConstants.PaymentGateways.Paystack,
                Name = "Paystack",
                Active = true
            };

            PaymentStatus paymentStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.PaymentStatuses.Pending,
                Name = "Pending"
            };

            factory.DbContextFactory.Context.PaymentGateways.Add(paymentGateway);
            factory.DbContextFactory.Context.PaymentStatuses.Add(paymentStatus);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            return (paymentGateway, paymentStatus);
        }

        private static PaymentTransaction CreatePaymentTransaction(PaymentGateway paymentGateway, PaymentStatus paymentStatus, string paymentReference)
        {
            return new PaymentTransaction
            {
                Id = Guid.NewGuid().ToString(),
                InvoiceId = Guid.NewGuid().ToString(),
                PaymentGatewayId = paymentGateway.Id,
                PaymentStatusId = paymentStatus.Id,
                PaymentReference = paymentReference,
                ProviderReference = $"PROVIDER-{paymentReference}",
                StudentId = Guid.NewGuid(),
                UserId = Guid.NewGuid().ToString(),
                PayerFirstName = "Test",
                PayerLastName = "Student",
                PayerEmail = "student@example.com",
                RegistrationNumber = $"REG-{Guid.NewGuid():N}",
                Amount = 5000m,
                Currency = "NGN",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PaymentGateway = paymentGateway,
                PaymentStatus = paymentStatus
            };
        }

        private static void SetupPaymentTransactionPagedMapping(PaymentServiceFactory factory)
        {
            factory.Mapper.Setup(x => x.Map<PagedResponse<PaymentTransactionResponse>>(It.IsAny<PagedList<PaymentTransaction>>()))
                .Returns((PagedList<PaymentTransaction> paymentTransactions) => new PagedResponse<PaymentTransactionResponse>
                {
                    Items = paymentTransactions.Select(paymentTransaction => new PaymentTransactionResponse
                    {
                        PaymentTransactionId = paymentTransaction.Id,
                        InvoiceId = paymentTransaction.InvoiceId,
                        PaymentReference = paymentTransaction.PaymentReference,
                        ProviderReference = paymentTransaction.ProviderReference,
                        PaymentGateway = paymentTransaction.PaymentGateway.Name,
                        PaymentStatus = paymentTransaction.PaymentStatus.Name,
                        StudentId = paymentTransaction.StudentId,
                        UserId = paymentTransaction.UserId,
                        PayerFirstName = paymentTransaction.PayerFirstName,
                        PayerLastName = paymentTransaction.PayerLastName,
                        PayerEmail = paymentTransaction.PayerEmail,
                        RegistrationNumber = paymentTransaction.RegistrationNumber,
                        Amount = paymentTransaction.Amount,
                        Currency = paymentTransaction.Currency,
                        FailureReason = paymentTransaction.FailureReason,
                        PaidAt = paymentTransaction.PaidAt,
                        CreatedAt = paymentTransaction.CreatedAt
                    }),
                    MetaData = paymentTransactions.MetaData
                });
        }

        private static Mock<IPaymentGateway> CreateGateway()
        {
            Mock<IPaymentGateway> gateway = new Mock<IPaymentGateway>();

            gateway.SetupGet(x => x.Code)
                .Returns(ApplicationConstants.PaymentGateways.Paystack);

            return gateway;
        }

        private static void SetupInitiatePaymentMappings(PaymentServiceFactory factory)
        {
            factory.Mapper.Setup(x => x.Map<PaymentTransaction>(It.IsAny<Invoice>()))
                .Returns((Invoice source) => new PaymentTransaction
                {
                    InvoiceId = source.Id,
                    StudentId = source.StudentId,
                    UserId = source.UserId,
                    PayerFirstName = source.PayerFirstName,
                    PayerLastName = source.PayerLastName,
                    PayerEmail = source.PayerEmail,
                    RegistrationNumber = source.RegistrationNumber,
                    Amount = source.Amount,
                    Currency = source.Currency
                });

            factory.Mapper.Setup(x => x.Map<GatewayPaymentInitializationRequest>(It.IsAny<PaymentTransaction>()))
                .Returns((PaymentTransaction source) => new GatewayPaymentInitializationRequest
                {
                    PaymentReference = source.PaymentReference,
                    Amount = source.Amount,
                    Currency = source.Currency,
                    Email = source.PayerEmail,
                    PayerFirstName = source.PayerFirstName,
                    PayerLastName = source.PayerLastName
                });
        }

        private static void SetupVerificationMappings(PaymentServiceFactory factory)
        {
            factory.Mapper.Setup(x => x.Map<PaymentVerificationResponse>(It.IsAny<PaymentTransaction>()))
                .Returns((PaymentTransaction source) => new PaymentVerificationResponse
                {
                    PaymentTransactionId = source.Id,
                    PaymentReference = source.PaymentReference,
                    ProviderReference = source.ProviderReference,
                    PaymentStatus = source.PaymentStatus.Name,
                    InvoiceStatus = source.Invoice.InvoiceStatus.Name,
                    Amount = source.Amount,
                    Currency = source.Currency,
                    PaidAt = source.PaidAt
                });

            factory.Mapper.Setup(x => x.Map(
                It.IsAny<GatewayPaymentVerificationResponse>(),
                It.IsAny<PaymentVerificationResponse>()))
                .Callback<GatewayPaymentVerificationResponse, PaymentVerificationResponse>((source, destination) =>
                {
                    destination.ProviderReference = source.ProviderReference;
                    destination.PaymentStatus = source.Status;
                    destination.PaidAt = source.PaidAt;
                })
                .Returns((GatewayPaymentVerificationResponse source, PaymentVerificationResponse destination) => destination);
        }
    }
}