using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Caching.Interfaces;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Request.Payments;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Dtos.Response.Payments;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Interfaces;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Caching.Keys;
using OnlineVoting.Services.Caching.Policies;
using OnlineVoting.Services.Caching.Tags;
using OnlineVoting.Services.Interfaces;
using OnlineVoting.Services.Interfaces.Payments;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VotingSystem.Data.Extensions;
using VotingSystem.Logger;

namespace OnlineVoting.Services.Implementation
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Invoice> _invoiceRepo;
        private readonly IRepository<InvoiceStatus> _invoiceStatusRepo;
        private readonly IRepository<PaymentTransaction> _paymentTransactionRepo;
        private readonly IRepository<PaymentGateway> _paymentGatewayRepo;
        private readonly IRepository<PaymentStatus> _paymentStatusRepo;
        private readonly IRepository<IdempotencyRecord> _idempotencyRecordRepo;
        private readonly IRepository<PositionApplication> _positionApplicationRepo;
        private readonly IRepository<PositionApplicationStatus> _positionApplicationStatusRepo;
        private readonly ICurrentUserContext _currentUserContext;
        private readonly IPaymentGatewayResolver _paymentGatewayResolver;
        private readonly IMapper _mapper;
        private readonly ILoggerMessage _loggerMessage;
        private readonly ICacheService _cacheService;

        public PaymentService(IServiceFactory serviceFactory)
        {
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _invoiceRepo = _unitOfWork.GetRepository<Invoice>();
            _invoiceStatusRepo = _unitOfWork.GetRepository<InvoiceStatus>();
            _paymentTransactionRepo = _unitOfWork.GetRepository<PaymentTransaction>();
            _paymentGatewayRepo = _unitOfWork.GetRepository<PaymentGateway>();
            _paymentStatusRepo = _unitOfWork.GetRepository<PaymentStatus>();
            _idempotencyRecordRepo = _unitOfWork.GetRepository<IdempotencyRecord>();
            _positionApplicationRepo = _unitOfWork.GetRepository<PositionApplication>();
            _positionApplicationStatusRepo = _unitOfWork.GetRepository<PositionApplicationStatus>();
            _currentUserContext = serviceFactory.GetService<ICurrentUserContext>();
            _paymentGatewayResolver = serviceFactory.GetService<IPaymentGatewayResolver>();
            _mapper = serviceFactory.GetService<IMapper>();
            _loggerMessage = serviceFactory.GetService<ILoggerMessage>();
            _cacheService = serviceFactory.GetService<ICacheService>();
        }

        public async Task<Result<InvoiceResponse>> GetInvoice(string invoiceId)
        {
            string normalizedInvoiceId = invoiceId.Trim();
            string cacheKey = InvoiceCacheKeys.GetInvoice(normalizedInvoiceId);

            Func<CancellationToken, ValueTask<InvoiceResponse?>> cacheFactory = async _ =>
            {
                Invoice? invoice = await _invoiceRepo.GetSingleByAsync(x => x.Id == normalizedInvoiceId,
                    include: query => query.Include(x => x.InvoiceStatus), tracking: false);

                if (invoice is null)
                    return null;

                return _mapper.Map<InvoiceResponse>(invoice);
            };

            InvoiceResponse? response = await _cacheService.GetOrCreate(cacheKey, cacheFactory, CachePolicies.Invoice);

            if (response is null)
            {
                _loggerMessage.LogWarn($"Invoice with id {normalizedInvoiceId} was not found.");

                return Result<InvoiceResponse>.NotFound($"Invoice with id {normalizedInvoiceId} was not found.");
            }

            _loggerMessage.LogInfo($"Invoice with id {normalizedInvoiceId} found.");

            return Result<InvoiceResponse>.Success(response);
        }

        public async Task<Result<PagedResponse<InvoiceResponse>>> GetInvoices(InvoiceRequest request)
        {
            string cacheKey = InvoiceCacheKeys.GetInvoices(request);

            Func<CancellationToken, ValueTask<PagedResponse<InvoiceResponse>>> cacheFactory = async _ =>
            {
                IQueryable<Invoice> query = _invoiceRepo.GetQueryable(include: query => query.Include(x => x.InvoiceStatus)).AsNoTracking();

                if (request.InvoiceStatusId.HasValue)
                    query = query.Where(x => x.InvoiceStatusId == request.InvoiceStatusId.Value);

                if (request.StudentId.HasValue)
                    query = query.Where(x => x.StudentId == request.StudentId.Value);

                if (!string.IsNullOrWhiteSpace(request.PositionApplicationId))
                    query = query.Where(x => x.PositionApplicationId == request.PositionApplicationId);

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    string searchTerm = request.SearchTerm.Trim();

                    query = query.Where(x => x.InvoiceNumber.Contains(searchTerm)
                        || x.PayerFirstName.Contains(searchTerm)
                        || x.PayerLastName.Contains(searchTerm)
                        || (x.PayerEmail != null && x.PayerEmail.Contains(searchTerm))
                        || (x.RegistrationNumber != null && x.RegistrationNumber.Contains(searchTerm)));
                }

                query = query.OrderByDescending(x => x.CreatedAt);

                PagedList<Invoice> invoices = await query.GetPagedItems(request);

                PagedResponse<InvoiceResponse> response = _mapper.Map<PagedResponse<InvoiceResponse>>(invoices);

                _loggerMessage.LogInfo($"{invoices.MetaData.TotalCount} invoices found.");

                return response;
            };

            PagedResponse<InvoiceResponse> response = await _cacheService.GetOrCreate(cacheKey, cacheFactory, CachePolicies.Invoice);

            return Result<PagedResponse<InvoiceResponse>>.Success(response);
        }

        public async Task<Result<string>> ProcessPaymentWebhook(string gatewayCode, string payload, string signature)
        {
            IPaymentGateway? gateway = _paymentGatewayResolver.Resolve(gatewayCode);
            if (gateway is null)
            {
                _loggerMessage.LogWarn($"Payment webhook processing failed because no payment gateway implementation was configured for code {gatewayCode}.");
                return Result<string>.ValidationError("The payment gateway is not configured.");
            }

            Result<GatewayPaymentWebhookResponse> webhookResult = gateway.ProcessWebhook(payload, signature);

            if (!webhookResult.IsSuccess || webhookResult.Value is null)
            {
                return Result<string>.FromFailure(webhookResult);
            }

            GatewayPaymentWebhookResponse webhookResponse = webhookResult.Value;

            if (webhookResponse.Event != "charge.success")
            {
                _loggerMessage.LogInfo($"Payment webhook event {webhookResponse.Event} was received for payment reference {webhookResponse.PaymentReference} and requires no processing.");

                return Result<string>.Success("Webhook received.");
            }

            PaymentTransaction? paymentTransaction = await _paymentTransactionRepo.GetSingleByAsync(x => x.PaymentReference == webhookResponse.PaymentReference,
                include: query => query.Include(x => x.PaymentGateway).Include(x => x.PaymentStatus).Include(x => x.Invoice).ThenInclude(x => x.InvoiceStatus));

            if (paymentTransaction is null)
            {
                _loggerMessage.LogWarn($"Payment webhook was received for payment reference {webhookResponse.PaymentReference}, but no local payment transaction was found.");

                return Result<string>.Success("Webhook received.");
            }

            if (!string.Equals(paymentTransaction.PaymentGateway.Code, gatewayCode, StringComparison.OrdinalIgnoreCase))
            {
                _loggerMessage.LogWarn($"Payment webhook for payment transaction id {paymentTransaction.Id} was ignored because gateway code {gatewayCode} does not match stored gateway code {paymentTransaction.PaymentGateway.Code}.");

                return Result<string>.Success("Webhook received.");
            }

            if (paymentTransaction.PaymentStatus.Code == ApplicationConstants.PaymentStatuses.Succeeded)
            {
                await UpdateRecoveredPaymentIdempotency(paymentTransaction);

                _loggerMessage.LogInfo($"Payment webhook for payment transaction id {paymentTransaction.Id} and payment reference {paymentTransaction.PaymentReference} was already processed successfully.");

                return Result<string>.Success("Webhook already processed.");
            }

            Result<PaymentVerificationResponse> verificationResult = await VerifyAndReconcilePayment(paymentTransaction, gateway);

            if (!verificationResult.IsSuccess || verificationResult.Value is null)
            {
                _loggerMessage.LogWarn($"Payment webhook verification could not complete for payment transaction id {paymentTransaction.Id} and payment reference {paymentTransaction.PaymentReference}.");

                return Result<string>.Success("Webhook received.");
            }

            if (paymentTransaction.PaymentStatus.Code != ApplicationConstants.PaymentStatuses.Succeeded)
            {
                _loggerMessage.LogWarn($"Payment webhook verification for payment reference {paymentTransaction.PaymentReference} did not confirm a successful payment.");

                return Result<string>.Success("Webhook received.");
            }

            await UpdateRecoveredPaymentIdempotency(paymentTransaction);

            _loggerMessage.LogInfo($"Payment webhook processed successfully for payment transaction id {paymentTransaction.Id} and payment reference {paymentTransaction.PaymentReference}.");

            return Result<string>.Success("Webhook processed successfully.");
        }

        private async Task UpdateRecoveredPaymentIdempotency(PaymentTransaction paymentTransaction)
        {
            IdempotencyRecord? idempotencyRecord = await _idempotencyRecordRepo.GetSingleByAsync(x => x.Operation == ApplicationConstants.IdempotencyOperations.InitiatePayment
                && x.ResourceId == paymentTransaction.Id);

            if (idempotencyRecord is null)
            {
                return;
            }

            if (idempotencyRecord.Status != ApplicationConstants.IdempotencyStatuses.Processing)
            {
                return;
            }

            InitiatePaymentResponse response = _mapper.Map<InitiatePaymentResponse>(paymentTransaction);
            response.PaymentGatewayCode = paymentTransaction.PaymentGateway.Code;
            response.PaymentStatus = ApplicationConstants.PaymentStatuses.Succeeded;

            idempotencyRecord.Status = ApplicationConstants.IdempotencyStatuses.Completed;
            idempotencyRecord.StatusCode = StatusCodes.Status200OK;
            idempotencyRecord.Response = JsonSerializer.Serialize(response);

            _idempotencyRecordRepo.Update(idempotencyRecord);

            await _unitOfWork.SaveChangesAsync();

            _loggerMessage.LogInfo($"Recovered payment initiation idempotency record with key {idempotencyRecord.Key} for payment transaction id {paymentTransaction.Id}.");
        }

        public async Task<Result<InvoiceResponse>> CreateInvoice(CreateInvoiceRequest request)
        {
            string payerName = $"{request.PayerFirstName} {request.PayerLastName}".Trim();

            _loggerMessage.LogInfo($"Invoice creation request received for {payerName}, registration number {request.RegistrationNumber}, student id {request.StudentId}, user id {request.UserId}, position application id {request.PositionApplicationId}.");

            Invoice? existingInvoice = await _invoiceRepo.GetSingleByAsync(x => x.PositionApplicationId == request.PositionApplicationId, include: query => query.Include(x => x.InvoiceStatus));
            if (existingInvoice is not null)
            {
                if (existingInvoice.StudentId != request.StudentId || existingInvoice.UserId != request.UserId)
                {
                    _loggerMessage.LogWarn($"Invoice ownership mismatch detected for position application id {request.PositionApplicationId}. Requested student id {request.StudentId}, requested user id {request.UserId}, stored student id {existingInvoice.StudentId}, stored user id {existingInvoice.UserId}.");
                    return Result<InvoiceResponse>.Conflict("The invoice ownership information does not match the position application.");
                }

                InvoiceResponse existingResponse = _mapper.Map<InvoiceResponse>(existingInvoice);
                _loggerMessage.LogInfo($"Invoice with id {existingInvoice.Id} and invoice number {existingInvoice.InvoiceNumber} already exists for {payerName}, " +
                    $"registration number {request.RegistrationNumber}, student id {request.StudentId}, user id {request.UserId}, position application id {request.PositionApplicationId}.");

                return Result<InvoiceResponse>.Success(existingResponse);
            }

            InvoiceStatus? unpaidStatus = await _invoiceStatusRepo.GetSingleByAsync(x => x.Code == ApplicationConstants.InvoiceStatuses.Unpaid);
            if (unpaidStatus is null)
            {
                _loggerMessage.LogWarn($"Invoice creation failed for {payerName}, registration number {request.RegistrationNumber}, student id {request.StudentId}, " +
                    $"user id {request.UserId}, position application id {request.PositionApplicationId}, because the Unpaid invoice status was not found.");
                return Result<InvoiceResponse>.NotFound("Unpaid invoice status was not found.");
            }

            Invoice invoice = _mapper.Map<Invoice>(request);
            invoice.InvoiceNumber = GenerateInvoiceNumber();
            invoice.InvoiceStatusId = unpaidStatus.Id;
            invoice.InvoiceStatus = unpaidStatus;

            await _invoiceRepo.AddAsync(invoice);

            await _cacheService.RemoveByTag(CacheTags.Invoice);

            InvoiceResponse response = _mapper.Map<InvoiceResponse>(invoice);

            _loggerMessage.LogInfo($"Invoice with id {invoice.Id} and invoice number {invoice.InvoiceNumber} created successfully for {payerName}, " +
                $"registration number {request.RegistrationNumber}, student id {request.StudentId}, user id {request.UserId}, position application id {request.PositionApplicationId}.");

            return Result<InvoiceResponse>.Created(response);
        }

        public async Task<Result<InitiatePaymentResponse>> InitiatePayment(InitiatePaymentRequest request)
        {
            string? userId = _currentUserContext.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                _loggerMessage.LogWarn("Payment initiation failed because the current user id was not available.");
                return Result<InitiatePaymentResponse>.ValidationError("The current user could not be identified.");
            }

            string invoiceId = request.InvoiceId.Trim();
            string idempotencyKey = request.IdempotencyKey.Trim();

            _loggerMessage.LogInfo($"Payment initiation request received from user id {userId} for invoice id {invoiceId} using payment gateway id {request.PaymentGatewayId}.");

            Invoice? invoice = await _invoiceRepo.GetSingleByAsync(x => x.Id == invoiceId, include: query => query.Include(x => x.InvoiceStatus));
            if (invoice is null)
            {
                _loggerMessage.LogWarn($"Payment initiation failed for user id {userId} because invoice with id {invoiceId} was not found.");
                return Result<InitiatePaymentResponse>.NotFound($"Invoice with id {invoiceId} was not found.");
            }

            if (invoice.UserId != userId)
            {
                _loggerMessage.LogWarn($"Payment initiation denied for user id {userId} because invoice with id {invoiceId} belongs to another user.");
                return Result<InitiatePaymentResponse>.Forbidden("You are not allowed to initiate payment for this invoice.");
            }

            string requestHash = CreatePaymentRequestHash(invoiceId, request.PaymentGatewayId);

            IdempotencyRecord? existingIdempotencyRecord = await _idempotencyRecordRepo.GetSingleByAsync(x => x.UserId == userId
                && x.Operation == ApplicationConstants.IdempotencyOperations.InitiatePayment && x.Key == idempotencyKey);

            if (existingIdempotencyRecord is not null)
            {
                if (existingIdempotencyRecord.RequestHash != requestHash)
                {
                    _loggerMessage.LogWarn($"Payment initiation failed for user id {userId} because idempotency key {idempotencyKey} was reused with a different request.");
                    return Result<InitiatePaymentResponse>.Conflict("The idempotency key has already been used for a different request.");
                }

                if (existingIdempotencyRecord.Status == ApplicationConstants.IdempotencyStatuses.Completed)
                {
                    if (string.IsNullOrWhiteSpace(existingIdempotencyRecord.Response))
                    {
                        _loggerMessage.LogWarn($"Completed payment initiation idempotency record with key {idempotencyKey} has no stored response for user id {userId}.");
                        return Result<InitiatePaymentResponse>.ValidationError("The previous payment initiation response could not be restored.");
                    }

                    InitiatePaymentResponse? storedResponse;

                    try
                    {
                        storedResponse = JsonSerializer.Deserialize<InitiatePaymentResponse>(existingIdempotencyRecord.Response);
                    }
                    catch (JsonException exception)
                    {
                        _loggerMessage.LogError($"Completed payment initiation idempotency record with key {idempotencyKey} contains an invalid stored response for user id {userId}. Error: {exception.Message}");
                        return Result<InitiatePaymentResponse>.ValidationError("The previous payment initiation response could not be restored.");
                    }

                    if (storedResponse is null)
                    {
                        _loggerMessage.LogWarn($"Completed payment initiation idempotency record with key {idempotencyKey} contains an invalid stored response for user id {userId}.");
                        return Result<InitiatePaymentResponse>.ValidationError("The previous payment initiation response could not be restored.");
                    }

                    _loggerMessage.LogInfo($"Completed payment initiation request with idempotency key {idempotencyKey} returned successfully for user id {userId}.");

                    return Result<InitiatePaymentResponse>.Success(storedResponse);
                }

                if (existingIdempotencyRecord.Status == ApplicationConstants.IdempotencyStatuses.Processing)
                {
                    Result<InitiatePaymentResponse>? processingResult = await HandleProcessingPayment(existingIdempotencyRecord, userId, invoiceId);

                    if (processingResult is not null)
                    {
                        return processingResult;
                    }
                }

                if (existingIdempotencyRecord.Status != ApplicationConstants.IdempotencyStatuses.Failed)
                {
                    _loggerMessage.LogWarn($"Payment initiation failed for user id {userId} because idempotency record with key {idempotencyKey} has unsupported status {existingIdempotencyRecord.Status}.");
                    return Result<InitiatePaymentResponse>.ValidationError("The payment initiation request has an invalid idempotency status.");
                }
            }

            if (invoice.InvoiceStatus.Code == ApplicationConstants.InvoiceStatuses.Paid)
            {
                _loggerMessage.LogWarn($"Payment initiation failed for user id {userId} because invoice with id {invoiceId} has already been paid.");
                return Result<InitiatePaymentResponse>.Conflict("The invoice has already been paid.");
            }

            if (invoice.InvoiceStatus.Code == ApplicationConstants.InvoiceStatuses.Cancelled)
            {
                _loggerMessage.LogWarn($"Payment initiation failed for user id {userId} because invoice with id {invoiceId} has been cancelled.");
                return Result<InitiatePaymentResponse>.ValidationError("Payment cannot be initiated for a cancelled invoice.");
            }

            if (invoice.InvoiceStatus.Code != ApplicationConstants.InvoiceStatuses.Unpaid)
            {
                _loggerMessage.LogWarn($"Payment initiation failed for user id {userId} because invoice with id {invoiceId} has an unsupported status {invoice.InvoiceStatus.Code}.");
                return Result<InitiatePaymentResponse>.ValidationError("The invoice is not available for payment.");
            }

            PaymentGateway? paymentGateway = await _paymentGatewayRepo.GetSingleByAsync(x => x.Id == request.PaymentGatewayId);
            if (paymentGateway is null)
            {
                _loggerMessage.LogWarn($"Payment initiation failed for user id {userId}, invoice id {invoiceId}, because payment gateway with id {request.PaymentGatewayId} was not found.");
                return Result<InitiatePaymentResponse>.NotFound($"Payment gateway with id {request.PaymentGatewayId} was not found.");
            }

            if (!paymentGateway.Active)
            {
                _loggerMessage.LogWarn($"Payment initiation failed for user id {userId}, invoice id {invoiceId}, because payment gateway with id {paymentGateway.Id} is inactive.");
                return Result<InitiatePaymentResponse>.ValidationError("The selected payment gateway is inactive.");
            }

            IPaymentGateway? gateway = _paymentGatewayResolver.Resolve(paymentGateway.Code);
            if (gateway is null)
            {
                _loggerMessage.LogWarn($"Payment initiation failed for user id {userId}, invoice id {invoiceId}, because no payment gateway implementation was configured for code {paymentGateway.Code}.");
                return Result<InitiatePaymentResponse>.ValidationError("The selected payment gateway is not configured.");
            }

            PaymentStatus? pendingStatus = await _paymentStatusRepo.GetSingleByAsync(x => x.Code == ApplicationConstants.PaymentStatuses.Pending);
            if (pendingStatus is null)
            {
                _loggerMessage.LogWarn($"Payment initiation failed for user id {userId}, invoice id {invoiceId}, because the Pending payment status was not found.");
                return Result<InitiatePaymentResponse>.NotFound("Pending payment status was not found.");
            }

            PaymentStatus? failedStatus = await _paymentStatusRepo.GetSingleByAsync(x => x.Code == ApplicationConstants.PaymentStatuses.Failed);
            if (failedStatus is null)
            {
                _loggerMessage.LogWarn($"Payment initiation failed for user id {userId}, invoice id {invoiceId}, because the Failed payment status was not found.");
                return Result<InitiatePaymentResponse>.NotFound("Failed payment status was not found.");
            }

            PaymentTransaction? existingPendingTransaction = await _paymentTransactionRepo.GetSingleByAsync(x => x.InvoiceId == invoice.Id
                && x.PaymentStatusId == pendingStatus.Id,
                include: query => query.Include(x => x.PaymentGateway).Include(x => x.PaymentStatus).Include(x => x.Invoice).ThenInclude(x => x.InvoiceStatus));

            if (existingPendingTransaction is not null)
            {
                IPaymentGateway? existingPaymentGateway = _paymentGatewayResolver.Resolve(existingPendingTransaction.PaymentGateway.Code);
                if (existingPaymentGateway is null)
                {
                    _loggerMessage.LogWarn($"Payment initiation failed for payment transaction id {existingPendingTransaction.Id} because no payment gateway implementation was configured for code {existingPendingTransaction.PaymentGateway.Code}.");
                    return Result<InitiatePaymentResponse>.ValidationError("The existing payment gateway is not configured.");
                }

                Result<InitiatePaymentResponse>? existingPaymentResult = await HandleExistingPendingPayment(existingPendingTransaction, existingPaymentGateway, null, userId, invoiceId);

                if (existingPaymentResult is not null)
                {
                    return existingPaymentResult;
                }
            }

            PaymentTransaction paymentTransaction = _mapper.Map<PaymentTransaction>(invoice);
            paymentTransaction.PaymentGatewayId = paymentGateway.Id;
            paymentTransaction.PaymentStatusId = pendingStatus.Id;
            paymentTransaction.PaymentReference = GeneratePaymentReference();

            IdempotencyRecord idempotencyRecord;
            bool isNewIdempotencyRecord;

            if (existingIdempotencyRecord is null)
            {
                isNewIdempotencyRecord = true;

                idempotencyRecord = new IdempotencyRecord
                {
                    Key = idempotencyKey,
                    UserId = userId,
                    Operation = ApplicationConstants.IdempotencyOperations.InitiatePayment,
                    RequestHash = requestHash,
                    Status = ApplicationConstants.IdempotencyStatuses.Processing,
                    ResourceId = paymentTransaction.Id
                };
            }
            else
            {
                isNewIdempotencyRecord = false;

                idempotencyRecord = existingIdempotencyRecord;
                idempotencyRecord.Status = ApplicationConstants.IdempotencyStatuses.Processing;
                idempotencyRecord.ResourceId = paymentTransaction.Id;
                idempotencyRecord.StatusCode = null;
                idempotencyRecord.Response = null;
            }

            try
            {
                _paymentTransactionRepo.Add(paymentTransaction);

                if (isNewIdempotencyRecord)
                {
                    _idempotencyRecordRepo.Add(idempotencyRecord);
                }
                else
                {
                    _idempotencyRecordRepo.Update(idempotencyRecord);
                }

                await _unitOfWork.SaveChangesAsync();
                await _cacheService.RemoveByTag(CacheTags.PaymentTransaction);

                GatewayPaymentInitializationRequest gatewayRequest = _mapper.Map<GatewayPaymentInitializationRequest>(paymentTransaction);

                Result<GatewayPaymentInitializationResponse> gatewayResult = await gateway.InitiatePayment(gatewayRequest);

                if (!gatewayResult.IsSuccess || gatewayResult.Value is null)
                {
                    paymentTransaction.PaymentStatusId = failedStatus.Id;
                    paymentTransaction.PaymentStatus = failedStatus;
                    paymentTransaction.FailureReason = gatewayResult.Error;

                    _paymentTransactionRepo.Update(paymentTransaction);

                    int gatewayStatusCode = GetStatusCode(gatewayResult.Status);

                    idempotencyRecord.Status = ApplicationConstants.IdempotencyStatuses.Failed;
                    idempotencyRecord.ResourceId = paymentTransaction.Id;
                    idempotencyRecord.StatusCode = gatewayStatusCode;
                    idempotencyRecord.Response = gatewayResult.Error;

                    _idempotencyRecordRepo.Update(idempotencyRecord);

                    await _unitOfWork.SaveChangesAsync();
                    await _cacheService.RemoveByTag(CacheTags.PaymentTransaction);

                    _loggerMessage.LogWarn($"Payment initiation failed for user id {userId}, invoice id {invoiceId}, payment transaction id {paymentTransaction.Id}, payment reference {paymentTransaction.PaymentReference}, using payment gateway {paymentGateway.Code}.");

                    return Result<InitiatePaymentResponse>.FromFailure(gatewayResult);
                }

                GatewayPaymentInitializationResponse gatewayResponse = gatewayResult.Value;

                paymentTransaction.ProviderReference = gatewayResponse.ProviderReference;
                paymentTransaction.CheckoutUrl = gatewayResponse.CheckoutUrl;
                paymentTransaction.FailureReason = null;

                _paymentTransactionRepo.Update(paymentTransaction);

                InitiatePaymentResponse response = _mapper.Map<InitiatePaymentResponse>(paymentTransaction);
                response.PaymentGatewayCode = paymentGateway.Code;
                response.PaymentStatus = pendingStatus.Name;

                idempotencyRecord.Status = ApplicationConstants.IdempotencyStatuses.Completed;
                idempotencyRecord.ResourceId = paymentTransaction.Id;
                idempotencyRecord.StatusCode = StatusCodes.Status201Created;
                idempotencyRecord.Response = JsonSerializer.Serialize(response);

                _idempotencyRecordRepo.Update(idempotencyRecord);

                await _unitOfWork.SaveChangesAsync();
                await _cacheService.RemoveByTag(CacheTags.PaymentTransaction);

                _loggerMessage.LogInfo($"Payment transaction with id {paymentTransaction.Id} and payment reference {paymentTransaction.PaymentReference} initialized successfully for user id {userId}, invoice id {invoiceId}, using payment gateway {paymentGateway.Code}.");

                return Result<InitiatePaymentResponse>.Created(response);
            }
            catch (Exception exception)
            {
                _loggerMessage.LogError($"Payment initiation encountered an unexpected error for user id {userId}, invoice id {invoiceId}, payment transaction id {paymentTransaction.Id}, payment reference {paymentTransaction.PaymentReference}, using payment gateway {paymentGateway.Code}. Error: {exception.Message}");

                throw;
            }
        }

        public async Task<Result<PaymentVerificationResponse>> VerifyPayment(string paymentReference)
        {
            string? userId = _currentUserContext.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                _loggerMessage.LogWarn("Payment verification failed because the current user id was not available.");
                return Result<PaymentVerificationResponse>.ValidationError("The current user could not be identified.");
            }

            if (string.IsNullOrWhiteSpace(paymentReference))
            {
                _loggerMessage.LogWarn($"Payment verification failed for user id {userId} because the payment reference was not provided.");
                return Result<PaymentVerificationResponse>.ValidationError("Payment reference is required.");
            }

            string normalizedPaymentReference = paymentReference.Trim();

            PaymentTransaction? paymentTransaction = await _paymentTransactionRepo.GetSingleByAsync(x => x.PaymentReference == normalizedPaymentReference,
                include: query => query.Include(x => x.PaymentGateway).Include(x => x.PaymentStatus).Include(x => x.Invoice).ThenInclude(x => x.InvoiceStatus));

            if (paymentTransaction is null)
            {
                _loggerMessage.LogWarn($"Payment verification failed for user id {userId} because payment transaction with reference {normalizedPaymentReference} was not found.");
                return Result<PaymentVerificationResponse>.NotFound($"Payment transaction with reference {normalizedPaymentReference} was not found.");
            }

            if (paymentTransaction.UserId != userId)
            {
                _loggerMessage.LogWarn($"Payment verification denied for user id {userId} because payment transaction with reference {normalizedPaymentReference} belongs to another user.");
                return Result<PaymentVerificationResponse>.Forbidden("You are not allowed to verify this payment.");
            }

            IPaymentGateway? gateway = _paymentGatewayResolver.Resolve(paymentTransaction.PaymentGateway.Code);
            if (gateway is null)
            {
                _loggerMessage.LogWarn($"Payment verification failed for payment transaction id {paymentTransaction.Id} because no payment gateway implementation was configured for code {paymentTransaction.PaymentGateway.Code}.");
                return Result<PaymentVerificationResponse>.ValidationError("The payment gateway is not configured.");
            }

            return await VerifyAndReconcilePayment(paymentTransaction, gateway);
        }

        public async Task<Result<PaymentTransactionResponse>> GetPaymentTransaction(string paymentReference)
        {
            string normalizedPaymentReference = paymentReference.Trim();
            string cacheKey = PaymentTransactionCacheKeys.GetPaymentTransaction(normalizedPaymentReference);

            Func<CancellationToken, ValueTask<PaymentTransactionResponse?>> cacheFactory = async _ =>
            {
                PaymentTransaction? paymentTransaction = await _paymentTransactionRepo.GetSingleByAsync(x => x.PaymentReference == normalizedPaymentReference,
                    include: query => query.Include(x => x.PaymentGateway).Include(x => x.PaymentStatus), tracking: false);

                if (paymentTransaction is null)
                    return null;

                return _mapper.Map<PaymentTransactionResponse>(paymentTransaction);
            };

            PaymentTransactionResponse? response = await _cacheService.GetOrCreate(cacheKey, cacheFactory, CachePolicies.PaymentTransaction);
            if (response is null)
            {
                _loggerMessage.LogWarn($"Payment transaction with reference {normalizedPaymentReference} was not found.");

                return Result<PaymentTransactionResponse>.NotFound($"Payment transaction with reference {normalizedPaymentReference} was not found.");
            }

            _loggerMessage.LogInfo($"Payment transaction with reference {normalizedPaymentReference} found.");

            return Result<PaymentTransactionResponse>.Success(response);
        }

        public async Task<Result<PagedResponse<PaymentTransactionResponse>>> GetPaymentTransactions(PaymentTransactionRequest request)
        {
            string cacheKey = PaymentTransactionCacheKeys.GetPaymentTransactions(request);

            Func<CancellationToken, ValueTask<PagedResponse<PaymentTransactionResponse>>> cacheFactory = async _ =>
            {
                IQueryable<PaymentTransaction> query = _paymentTransactionRepo.GetQueryable(include: query => query.Include(x => x.PaymentGateway)
                    .Include(x => x.PaymentStatus)).AsNoTracking();

                if (request.PaymentGatewayId.HasValue)
                    query = query.Where(x => x.PaymentGatewayId == request.PaymentGatewayId.Value);

                if (request.PaymentStatusId.HasValue)
                    query = query.Where(x => x.PaymentStatusId == request.PaymentStatusId.Value);

                if (!string.IsNullOrWhiteSpace(request.InvoiceId))
                    query = query.Where(x => x.InvoiceId == request.InvoiceId);

                if (request.StudentId.HasValue)
                    query = query.Where(x => x.StudentId == request.StudentId.Value);

                if (!string.IsNullOrWhiteSpace(request.UserId))
                    query = query.Where(x => x.UserId == request.UserId);

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    string searchTerm = request.SearchTerm.Trim();

                    query = query.Where(x => x.PaymentReference.Contains(searchTerm)
                        || (x.ProviderReference != null && x.ProviderReference.Contains(searchTerm))
                        || x.PayerFirstName.Contains(searchTerm)
                        || x.PayerLastName.Contains(searchTerm)
                        || (x.PayerEmail != null && x.PayerEmail.Contains(searchTerm))
                        || (x.RegistrationNumber != null && x.RegistrationNumber.Contains(searchTerm)));
                }

                query = query.OrderByDescending(x => x.CreatedAt);

                PagedList<PaymentTransaction> paymentTransactions = await query.GetPagedItems(request);

                PagedResponse<PaymentTransactionResponse> response = _mapper.Map<PagedResponse<PaymentTransactionResponse>>(paymentTransactions);

                _loggerMessage.LogInfo($"{paymentTransactions.MetaData.TotalCount} payment transactions found.");

                return response;
            };

            PagedResponse<PaymentTransactionResponse> response = await _cacheService.GetOrCreate(
                cacheKey, cacheFactory, CachePolicies.PaymentTransaction);

            return Result<PagedResponse<PaymentTransactionResponse>>.Success(response);
        }

        private async Task<Result<InitiatePaymentResponse>?> HandleProcessingPayment(IdempotencyRecord idempotencyRecord, string userId, string invoiceId)
        {
            if (string.IsNullOrWhiteSpace(idempotencyRecord.ResourceId))
            {
                _loggerMessage.LogWarn($"Payment initiation request with idempotency key {idempotencyRecord.Key} is already being processed for user id {userId}.");
                return Result<InitiatePaymentResponse>.Conflict("The payment initiation request is already being processed.");
            }

            PaymentTransaction? paymentTransaction = await _paymentTransactionRepo.GetSingleByAsync(x => x.Id == idempotencyRecord.ResourceId,
                include: query => query.Include(x => x.PaymentGateway).Include(x => x.PaymentStatus).Include(x => x.Invoice).ThenInclude(x => x.InvoiceStatus));

            if (paymentTransaction is null)
            {
                _loggerMessage.LogWarn($"Payment initiation recovery failed for user id {userId}, invoice id {invoiceId}, " +
                    $"because payment transaction with id {idempotencyRecord.ResourceId} was not found.");
                return Result<InitiatePaymentResponse>.NotFound("The payment transaction linked to the payment request was not found.");
            }

            IPaymentGateway? gateway = _paymentGatewayResolver.Resolve(paymentTransaction.PaymentGateway.Code);
            if (gateway is null)
            {
                _loggerMessage.LogWarn($"Payment initiation recovery failed for payment transaction id {paymentTransaction.Id} because no payment gateway implementation was configured for code {paymentTransaction.PaymentGateway.Code}.");
                return Result<InitiatePaymentResponse>.ValidationError("The payment gateway is not configured.");
            }

            return await HandleExistingPendingPayment(paymentTransaction, gateway, idempotencyRecord, userId, invoiceId);
        }

        private async Task<Result<InitiatePaymentResponse>?> HandleExistingPendingPayment(PaymentTransaction paymentTransaction, IPaymentGateway gateway,
            IdempotencyRecord? idempotencyRecord, string userId, string invoiceId)
        {
            Result<PaymentVerificationResponse> verificationResult = await VerifyAndReconcilePayment(paymentTransaction, gateway);

            if (!verificationResult.IsSuccess || verificationResult.Value is null)
            {
                return Result<InitiatePaymentResponse>.FromFailure(verificationResult);
            }

            PaymentVerificationResponse verificationResponse = verificationResult.Value;
            if (verificationResponse.PaymentStatus == ApplicationConstants.GatewayPaymentStatuses.Pending)
            {
                _loggerMessage.LogInfo($"Payment initiation stopped for user id {userId}, invoice id {invoiceId}, because existing payment transaction with reference " +
                    $"{paymentTransaction.PaymentReference} is still pending.");
                return Result<InitiatePaymentResponse>.Conflict("A payment for this invoice is still being processed.");
            }

            if (verificationResponse.PaymentStatus == ApplicationConstants.GatewayPaymentStatuses.Succeeded)
            {
                InitiatePaymentResponse response = _mapper.Map<InitiatePaymentResponse>(paymentTransaction);
                response.PaymentGatewayCode = paymentTransaction.PaymentGateway.Code;
                response.PaymentStatus = ApplicationConstants.PaymentStatuses.Succeeded;

                if (idempotencyRecord is not null)
                {
                    idempotencyRecord.Status = ApplicationConstants.IdempotencyStatuses.Completed;
                    idempotencyRecord.ResourceId = paymentTransaction.Id;
                    idempotencyRecord.StatusCode = StatusCodes.Status200OK;
                    idempotencyRecord.Response = JsonSerializer.Serialize(response);

                    _idempotencyRecordRepo.Update(idempotencyRecord);

                    await _unitOfWork.SaveChangesAsync();
                }

                _loggerMessage.LogInfo($"Existing payment transaction with reference {paymentTransaction.PaymentReference} was recovered successfully for user id {userId}, invoice id {invoiceId}.");

                return Result<InitiatePaymentResponse>.Success(response);
            }

            if (verificationResponse.PaymentStatus == ApplicationConstants.GatewayPaymentStatuses.Failed
                || verificationResponse.PaymentStatus == ApplicationConstants.GatewayPaymentStatuses.Cancelled)
            {
                if (idempotencyRecord is not null)
                {
                    idempotencyRecord.Status = ApplicationConstants.IdempotencyStatuses.Failed;
                    idempotencyRecord.ResourceId = paymentTransaction.Id;
                    idempotencyRecord.StatusCode = null;
                    idempotencyRecord.Response = null;

                    _idempotencyRecordRepo.Update(idempotencyRecord);

                    await _unitOfWork.SaveChangesAsync();
                }

                return null;
            }

            _loggerMessage.LogWarn($"Payment initiation could not continue for user id {userId}, invoice id {invoiceId}, because payment transaction with reference {paymentTransaction.PaymentReference} returned unsupported verification status {verificationResponse.PaymentStatus}.");

            return Result<InitiatePaymentResponse>.Conflict("The payment status could not be resolved.");
        }

        private async Task<Result<PaymentVerificationResponse>> VerifyAndReconcilePayment(PaymentTransaction paymentTransaction, IPaymentGateway gateway)
        {
            Result<GatewayPaymentVerificationResponse> gatewayResult;

            try
            {
                gatewayResult = await gateway.VerifyPayment(paymentTransaction.PaymentReference);
            }
            catch (Exception exception)
            {
                _loggerMessage.LogError($"Payment verification encountered an unexpected error for payment transaction id {paymentTransaction.Id}, payment reference {paymentTransaction.PaymentReference}, using payment gateway {paymentTransaction.PaymentGateway.Code}. Error: {exception.Message}");

                throw;
            }

            if (!gatewayResult.IsSuccess || gatewayResult.Value is null)
            {
                _loggerMessage.LogWarn($"Payment verification could not be completed for payment transaction id {paymentTransaction.Id}, payment reference {paymentTransaction.PaymentReference}, using payment gateway {paymentTransaction.PaymentGateway.Code}.");

                return Result<PaymentVerificationResponse>.FromFailure(gatewayResult);
            }

            GatewayPaymentVerificationResponse gatewayResponse = gatewayResult.Value;

            if (!string.Equals(gatewayResponse.PaymentReference, paymentTransaction.PaymentReference, StringComparison.OrdinalIgnoreCase))
            {
                _loggerMessage.LogWarn($"Payment verification failed for payment transaction id {paymentTransaction.Id} because the provider payment reference {gatewayResponse.PaymentReference} does not match the stored payment reference {paymentTransaction.PaymentReference}.");

                return Result<PaymentVerificationResponse>.Conflict("The verified payment reference does not match the stored payment reference.");
            }

            if (gatewayResponse.Amount != paymentTransaction.Amount)
            {
                _loggerMessage.LogWarn($"Payment verification failed for payment transaction id {paymentTransaction.Id} because the provider amount {gatewayResponse.Amount} does not match the stored amount {paymentTransaction.Amount}.");

                return Result<PaymentVerificationResponse>.Conflict("The verified payment amount does not match the expected amount.");
            }

            if (!string.Equals(gatewayResponse.Currency, paymentTransaction.Currency, StringComparison.OrdinalIgnoreCase))
            {
                _loggerMessage.LogWarn($"Payment verification failed for payment transaction id {paymentTransaction.Id} because the provider currency {gatewayResponse.Currency} does not match the stored currency {paymentTransaction.Currency}.");

                return Result<PaymentVerificationResponse>.Conflict("The verified payment currency does not match the expected currency.");
            }

            if (gatewayResponse.Status == ApplicationConstants.GatewayPaymentStatuses.Succeeded)
            {
                return await ReconcileSuccessfulPayment(paymentTransaction, gatewayResponse);
            }

            if (gatewayResponse.Status == ApplicationConstants.GatewayPaymentStatuses.Failed)
            {
                return await ReconcileFailedPayment(paymentTransaction, gatewayResponse);
            }

            if (gatewayResponse.Status == ApplicationConstants.GatewayPaymentStatuses.Cancelled)
            {
                return await ReconcileCancelledPayment(paymentTransaction, gatewayResponse);
            }

            if (gatewayResponse.Status == ApplicationConstants.GatewayPaymentStatuses.Pending)
            {
                return CreatePendingVerificationResponse(paymentTransaction, gatewayResponse);
            }

            _loggerMessage.LogWarn($"Payment verification failed for payment transaction id {paymentTransaction.Id} because gateway returned unsupported payment status {gatewayResponse.Status}.");

            return Result<PaymentVerificationResponse>.ValidationError("The payment gateway returned an unsupported payment status.");
        }

        private async Task<Result<PaymentVerificationResponse>> ReconcileSuccessfulPayment(PaymentTransaction paymentTransaction, GatewayPaymentVerificationResponse gatewayResponse)
        {
            PaymentStatus? succeededStatus = await _paymentStatusRepo.GetSingleByAsync(x => x.Code == ApplicationConstants.PaymentStatuses.Succeeded);
            if (succeededStatus is null)
            {
                _loggerMessage.LogWarn($"Payment reconciliation failed for payment transaction id {paymentTransaction.Id} because the Succeeded payment status was not found.");
                return Result<PaymentVerificationResponse>.NotFound("Succeeded payment status was not found.");
            }

            InvoiceStatus? paidInvoiceStatus = await _invoiceStatusRepo.GetSingleByAsync(x => x.Code == ApplicationConstants.InvoiceStatuses.Paid);
            if (paidInvoiceStatus is null)
            {
                _loggerMessage.LogWarn($"Payment reconciliation failed for payment transaction id {paymentTransaction.Id} because the Paid invoice status was not found.");
                return Result<PaymentVerificationResponse>.NotFound("Paid invoice status was not found.");
            }

            PositionApplication? positionApplication = await _positionApplicationRepo.GetSingleByAsync(x => x.Id == paymentTransaction.Invoice.PositionApplicationId,
                include: query => query.Include(x => x.PositionApplicationStatus));

            if (positionApplication is null)
            {
                _loggerMessage.LogWarn($"Payment reconciliation failed for payment transaction id {paymentTransaction.Id} because position application with id {paymentTransaction.Invoice.PositionApplicationId} was not found.");
                return Result<PaymentVerificationResponse>.NotFound("Position application was not found.");
            }

            PositionApplicationStatus? pendingReviewStatus = null;

            if (positionApplication.PositionApplicationStatus.Code == ApplicationConstants.PositionApplicationStatuses.PendingPayment)
            {
                pendingReviewStatus = await _positionApplicationStatusRepo.GetSingleByAsync(x => x.Code == ApplicationConstants.PositionApplicationStatuses.PendingReview);

                if (pendingReviewStatus is null)
                {
                    _loggerMessage.LogWarn($"Payment reconciliation failed for payment transaction id {paymentTransaction.Id} because the Pending Review position application status was not found.");
                    return Result<PaymentVerificationResponse>.NotFound("Pending Review position application status was not found.");
                }
            }

            DateTime paidAt = gatewayResponse.PaidAt ?? paymentTransaction.PaidAt ?? DateTime.UtcNow;

            paymentTransaction.PaymentStatusId = succeededStatus.Id;
            paymentTransaction.PaymentStatus = succeededStatus;
            paymentTransaction.ProviderReference = gatewayResponse.ProviderReference ?? paymentTransaction.ProviderReference;
            paymentTransaction.PaidAt = paidAt;
            paymentTransaction.FailureReason = null;

            paymentTransaction.Invoice.InvoiceStatusId = paidInvoiceStatus.Id;
            paymentTransaction.Invoice.InvoiceStatus = paidInvoiceStatus;
            paymentTransaction.Invoice.PaidAt = paidAt;

            _paymentTransactionRepo.Update(paymentTransaction);
            _invoiceRepo.Update(paymentTransaction.Invoice);

            if (pendingReviewStatus is not null)
            {
                positionApplication.PositionApplicationStatusId = pendingReviewStatus.Id;
                positionApplication.PositionApplicationStatus = pendingReviewStatus;

                _positionApplicationRepo.Update(positionApplication);
            }

            await _unitOfWork.SaveChangesAsync();
            await _cacheService.RemoveByTag(CacheTags.PaymentTransaction);
            await _cacheService.RemoveByTag(CacheTags.Invoice);
            await _cacheService.RemoveByTag(CacheTags.PositionApplication);

            PaymentVerificationResponse response = CreateVerificationResponse(paymentTransaction, gatewayResponse);

            _loggerMessage.LogInfo($"Payment transaction with id {paymentTransaction.Id} and payment reference {paymentTransaction.PaymentReference} was verified successfully and reconciled.");

            return Result<PaymentVerificationResponse>.Success(response);
        }

        private async Task<Result<PaymentVerificationResponse>> ReconcileFailedPayment(PaymentTransaction paymentTransaction, GatewayPaymentVerificationResponse gatewayResponse)
        {
            PaymentStatus? failedStatus = await _paymentStatusRepo.GetSingleByAsync(x => x.Code == ApplicationConstants.PaymentStatuses.Failed);
            if (failedStatus is null)
            {
                _loggerMessage.LogWarn($"Payment reconciliation failed for payment transaction id {paymentTransaction.Id} because the Failed payment status was not found.");
                return Result<PaymentVerificationResponse>.NotFound("Failed payment status was not found.");
            }

            paymentTransaction.PaymentStatusId = failedStatus.Id;
            paymentTransaction.PaymentStatus = failedStatus;
            paymentTransaction.ProviderReference = gatewayResponse.ProviderReference ?? paymentTransaction.ProviderReference;

            _paymentTransactionRepo.Update(paymentTransaction);

            await _unitOfWork.SaveChangesAsync();
            await _cacheService.RemoveByTag(CacheTags.PaymentTransaction);

            PaymentVerificationResponse response = CreateVerificationResponse(paymentTransaction, gatewayResponse);

            _loggerMessage.LogInfo($"Payment transaction with id {paymentTransaction.Id} and payment reference {paymentTransaction.PaymentReference} was verified as failed.");

            return Result<PaymentVerificationResponse>.Success(response);
        }

        private async Task<Result<PaymentVerificationResponse>> ReconcileCancelledPayment(PaymentTransaction paymentTransaction, GatewayPaymentVerificationResponse gatewayResponse)
        {
            PaymentStatus? cancelledStatus = await _paymentStatusRepo.GetSingleByAsync(x => x.Code == ApplicationConstants.PaymentStatuses.Cancelled);
            if (cancelledStatus is null)
            {
                _loggerMessage.LogWarn($"Payment reconciliation failed for payment transaction id {paymentTransaction.Id} because the Cancelled payment status was not found.");
                return Result<PaymentVerificationResponse>.NotFound("Cancelled payment status was not found.");
            }

            paymentTransaction.PaymentStatusId = cancelledStatus.Id;
            paymentTransaction.PaymentStatus = cancelledStatus;
            paymentTransaction.ProviderReference = gatewayResponse.ProviderReference ?? paymentTransaction.ProviderReference;

            _paymentTransactionRepo.Update(paymentTransaction);

            await _unitOfWork.SaveChangesAsync();
            await _cacheService.RemoveByTag(CacheTags.PaymentTransaction);

            PaymentVerificationResponse response = CreateVerificationResponse(paymentTransaction, gatewayResponse);

            _loggerMessage.LogInfo($"Payment transaction with id {paymentTransaction.Id} and payment reference {paymentTransaction.PaymentReference} was verified as cancelled.");

            return Result<PaymentVerificationResponse>.Success(response);
        }

        private Result<PaymentVerificationResponse> CreatePendingVerificationResponse(PaymentTransaction paymentTransaction, GatewayPaymentVerificationResponse gatewayResponse)
        {
            PaymentVerificationResponse response = CreateVerificationResponse(paymentTransaction, gatewayResponse);

            _loggerMessage.LogInfo($"Payment transaction with id {paymentTransaction.Id} and payment reference {paymentTransaction.PaymentReference} remains pending after verification.");

            return Result<PaymentVerificationResponse>.Success(response);
        }

        private PaymentVerificationResponse CreateVerificationResponse(PaymentTransaction paymentTransaction, GatewayPaymentVerificationResponse gatewayResponse)
        {
            PaymentVerificationResponse response = _mapper.Map<PaymentVerificationResponse>(paymentTransaction);

            _mapper.Map(gatewayResponse, response);

            return response;
        }

        private static string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}";
        }

        private static string CreatePaymentRequestHash(string invoiceId, int paymentGatewayId)
        {
            string requestValue = $"{invoiceId.Trim().ToLowerInvariant()}:{paymentGatewayId}";
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(requestValue));

            return Convert.ToHexString(hash);
        }

        private static string GeneratePaymentReference()
        {
            return $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}";
        }

        private static int GetStatusCode(ResultStatus status)
        {
            return status switch
            {
                ResultStatus.Success => StatusCodes.Status200OK,
                ResultStatus.Created => StatusCodes.Status201Created,
                ResultStatus.NoContent => StatusCodes.Status204NoContent,
                ResultStatus.ValidationError => StatusCodes.Status400BadRequest,
                ResultStatus.Unauthorized => StatusCodes.Status401Unauthorized,
                ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
                ResultStatus.NotFound => StatusCodes.Status404NotFound,
                ResultStatus.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };
        }
    }
}