using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Caching.Interfaces;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Interfaces;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VotingSystem.Data.Extensions;
using VotingSystem.Logger;
using OnlineVoting.Services.Caching.Keys;
using OnlineVoting.Services.Caching.Policies;
using OnlineVoting.Services.Caching.Tags;

namespace OnlineVoting.Services.Implementation
{
    public class PositionApplicationService : IPositionApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<PositionApplication> _positionApplicationRepo;
        private readonly IRepository<PositionApplicationStatus> _positionApplicationStatusRepo;
        private readonly IRepository<ElectionPosition> _electionPositionRepo;
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<IdempotencyRecord> _idempotencyRecordRepo;
        private readonly IRepository<Invoice> _invoiceRepo;
        private readonly IRepository<InvoiceStatus> _invoiceStatusRepo;
        private readonly ICurrentUserContext _currentUserContext;
        private readonly IPaymentService _paymentService;
        private readonly IMapper _mapper;
        private readonly ILoggerMessage _loggerMessage;
        private readonly ICacheService _cacheService;

        public PositionApplicationService(IServiceFactory serviceFactory)
        {
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _positionApplicationRepo = _unitOfWork.GetRepository<PositionApplication>();
            _positionApplicationStatusRepo = _unitOfWork.GetRepository<PositionApplicationStatus>();
            _electionPositionRepo = _unitOfWork.GetRepository<ElectionPosition>();
            _studentRepo = _unitOfWork.GetRepository<Student>();
            _idempotencyRecordRepo = _unitOfWork.GetRepository<IdempotencyRecord>();
            _invoiceRepo = _unitOfWork.GetRepository<Invoice>();
            _invoiceStatusRepo = _unitOfWork.GetRepository<InvoiceStatus>();
            _currentUserContext = serviceFactory.GetService<ICurrentUserContext>();
            _paymentService = serviceFactory.GetService<IPaymentService>();
            _mapper = serviceFactory.GetService<IMapper>();
            _loggerMessage = serviceFactory.GetService<ILoggerMessage>();
            _cacheService = serviceFactory.GetService<ICacheService>();
        }

        public async Task<Result<CreatePositionApplicationResponse>> CreatePositionApplication(CreatePositionApplicationRequest request)
        {
            string? userId = _currentUserContext.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                _loggerMessage.LogWarn("Position application creation failed because the current user id was not available.");
                return Result<CreatePositionApplicationResponse>.ValidationError("The current user could not be identified.");
            }

            string electionPositionId = request.ElectionPositionId.Trim();
            string idempotencyKey = request.IdempotencyKey.Trim();
            string requestHash = CreateRequestHash(electionPositionId);

            _loggerMessage.LogInfo($"Position application creation request received from user id {userId} for election position id {electionPositionId}.");

            IdempotencyRecord? idempotencyRecord = await _idempotencyRecordRepo.GetSingleByAsync(x => x.UserId == userId
                && x.Operation == ApplicationConstants.IdempotencyOperations.CreatePositionApplication
                && x.Key == idempotencyKey);

            if (idempotencyRecord is not null)
            {
                if (idempotencyRecord.RequestHash != requestHash)
                {
                    _loggerMessage.LogWarn($"Position application creation failed for user id {userId} because idempotency key {idempotencyKey} was reused with a different request.");
                    return Result<CreatePositionApplicationResponse>.Conflict("The idempotency key has already been used for a different request.");
                }

                if (idempotencyRecord.Status == ApplicationConstants.IdempotencyStatuses.Processing)
                {
                    _loggerMessage.LogWarn($"Position application creation request from user id {userId} with idempotency key {idempotencyKey} is already being processed.");
                    return Result<CreatePositionApplicationResponse>.Conflict("The request is already being processed.");
                }

                if (idempotencyRecord.Status == ApplicationConstants.IdempotencyStatuses.Completed)
                {
                    if (string.IsNullOrWhiteSpace(idempotencyRecord.Response))
                    {
                        _loggerMessage.LogWarn($"Completed position application request with idempotency key {idempotencyKey} has no stored response.");
                        return Result<CreatePositionApplicationResponse>.Conflict("The completed request could not be restored.");
                    }

                    CreatePositionApplicationResponse? completedResponse = JsonSerializer.Deserialize<CreatePositionApplicationResponse>(idempotencyRecord.Response);
                    if (completedResponse is null)
                    {
                        _loggerMessage.LogWarn($"Completed position application request with idempotency key {idempotencyKey} could not be restored.");
                        return Result<CreatePositionApplicationResponse>.Conflict("The completed request could not be restored.");
                    }

                    _loggerMessage.LogInfo($"Position application creation request from user id {userId} with idempotency key {idempotencyKey} has already been completed.");

                    if (idempotencyRecord.StatusCode == StatusCodes.Status200OK)
                        return Result<CreatePositionApplicationResponse>.Success(completedResponse);

                    if (idempotencyRecord.StatusCode == StatusCodes.Status201Created)
                        return Result<CreatePositionApplicationResponse>.Created(completedResponse);

                    _loggerMessage.LogWarn($"Completed position application request with idempotency key {idempotencyKey} has an invalid stored status code {idempotencyRecord.StatusCode}.");
                    return Result<CreatePositionApplicationResponse>.Conflict("The completed request could not be restored.");
                }

                if (idempotencyRecord.Status != ApplicationConstants.IdempotencyStatuses.Failed)
                {
                    _loggerMessage.LogWarn($"Position application creation request from user id {userId} has unsupported idempotency status {idempotencyRecord.Status}.");
                    return Result<CreatePositionApplicationResponse>.Conflict("The request has an invalid idempotency state.");
                }

                _loggerMessage.LogInfo($"Retrying failed position application request from user id {userId} with idempotency key {idempotencyKey}.");
            }

            Student? student = await _studentRepo.GetSingleByAsync(x => x.UserId == userId, include: query => query.Include(x => x.User));
            if (student is null)
            {
                _loggerMessage.LogWarn($"Position application creation failed because no student profile was found for user id {userId}.");
                return Result<CreatePositionApplicationResponse>.NotFound("Student profile was not found.");
            }

            if (student.User is null)
            {
                _loggerMessage.LogWarn($"Position application creation failed because student with id {student.Id} and user id {userId} has no linked user.");
                return Result<CreatePositionApplicationResponse>.ValidationError("The student profile does not have a valid user account.");
            }

            string applicantName = $"{student.User.FirstName} {student.User.LastName}".Trim();

            _loggerMessage.LogInfo($"Position application request is being processed for {applicantName}, registration number {student.RegNumber}, student id {student.Id}, user id {userId}, for election position id {electionPositionId}.");

            if (!student.Active)
            {
                _loggerMessage.LogWarn($"Position application creation failed for {applicantName}, registration number {student.RegNumber}, because student with id {student.Id} is inactive.");
                return Result<CreatePositionApplicationResponse>.ValidationError("The student profile is inactive.");
            }

            if (!student.User.Active)
            {
                _loggerMessage.LogWarn($"Position application creation failed for {applicantName}, registration number {student.RegNumber}, because user with id {userId} is inactive.");
                return Result<CreatePositionApplicationResponse>.ValidationError("The user account is inactive.");
            }

            ElectionPosition? electionPosition = await _electionPositionRepo.GetSingleByAsync(x => x.Id == electionPositionId, include: query => query.Include(x => x.Election));
            if (electionPosition is null)
            {
                _loggerMessage.LogWarn($"Position application creation failed for {applicantName}, registration number {student.RegNumber}, because election position with id {electionPositionId} was not found.");
                return Result<CreatePositionApplicationResponse>.NotFound($"Election position with id {electionPositionId} was not found.");
            }

            if (!electionPosition.Active)
            {
                _loggerMessage.LogWarn($"Position application creation failed for {applicantName}, registration number {student.RegNumber}, because election position with id {electionPositionId} is inactive.");
                return Result<CreatePositionApplicationResponse>.ValidationError("The election position is inactive.");
            }

            Election election = electionPosition.Election;

            if (!election.Active)
            {
                _loggerMessage.LogWarn($"Position application creation failed for {applicantName}, registration number {student.RegNumber}, because election with id {election.Id} is inactive.");
                return Result<CreatePositionApplicationResponse>.ValidationError("The election is inactive.");
            }

            if (!election.ApplicationStartAt.HasValue || !election.ApplicationEndAt.HasValue)
            {
                _loggerMessage.LogWarn($"Position application creation failed for {applicantName}, registration number {student.RegNumber}, because the application period for election with id {election.Id} has not been configured.");
                return Result<CreatePositionApplicationResponse>.ValidationError("The application period has not been configured.");
            }

            DateTime currentDate = DateTime.UtcNow;

            if (currentDate < election.ApplicationStartAt.Value)
            {
                _loggerMessage.LogWarn($"Position application creation failed for {applicantName}, registration number {student.RegNumber}, because the application period for election with id {election.Id} has not started.");
                return Result<CreatePositionApplicationResponse>.ValidationError("The application period has not started.");
            }

            if (currentDate > election.ApplicationEndAt.Value)
            {
                _loggerMessage.LogWarn($"Position application creation failed for {applicantName}, registration number {student.RegNumber}, because the application period for election with id {election.Id} has ended.");
                return Result<CreatePositionApplicationResponse>.ValidationError("The application period has ended.");
            }

            if (idempotencyRecord is null)
            {
                idempotencyRecord = new IdempotencyRecord
                {
                    Key = idempotencyKey,
                    UserId = userId,
                    Operation = ApplicationConstants.IdempotencyOperations.CreatePositionApplication,
                    RequestHash = requestHash,
                    Status = ApplicationConstants.IdempotencyStatuses.Processing
                };

                await _idempotencyRecordRepo.AddAsync(idempotencyRecord);
            }
            else if (idempotencyRecord.Status == ApplicationConstants.IdempotencyStatuses.Failed)
            {
                idempotencyRecord.Status = ApplicationConstants.IdempotencyStatuses.Processing;
                idempotencyRecord.ResourceId = null;
                idempotencyRecord.StatusCode = null;
                idempotencyRecord.Response = null;

                await _idempotencyRecordRepo.UpdateAsync(idempotencyRecord);
            }

            CreateInvoiceRequest invoiceRequest = _mapper.Map<CreateInvoiceRequest>(student);
            invoiceRequest.Amount = electionPosition.ApplicationFee;
            invoiceRequest.Currency = electionPosition.Currency;

            bool transactionStarted = false;
            bool existingApplication = false;
            PositionApplication? positionApplication = null;

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                transactionStarted = true;

                positionApplication = await _positionApplicationRepo.GetSingleByAsync(x => x.StudentId == student.Id
                    && x.ElectionPositionId == electionPositionId, include: query => query.Include(x => x.PositionApplicationStatus));

                existingApplication = positionApplication is not null;

                if (positionApplication is null)
                {
                    PositionApplicationStatus? pendingPaymentStatus = await _positionApplicationStatusRepo.GetSingleByAsync(x => x.Code == ApplicationConstants.PositionApplicationStatuses.PendingPayment);
                    if (pendingPaymentStatus is null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        transactionStarted = false;

                        idempotencyRecord.Status = ApplicationConstants.IdempotencyStatuses.Failed;
                        idempotencyRecord.StatusCode = StatusCodes.Status404NotFound;
                        idempotencyRecord.Response = "Pending Payment position application status was not found.";

                        await _idempotencyRecordRepo.UpdateAsync(idempotencyRecord);
                                                
                        _loggerMessage.LogWarn($"Position application creation failed for {applicantName}, registration number {student.RegNumber}, because the Pending Payment status was not found.");

                        return Result<CreatePositionApplicationResponse>.NotFound("Pending Payment position application status was not found.");
                    }

                    positionApplication = _mapper.Map<PositionApplication>(request);
                    positionApplication.StudentId = student.Id;
                    positionApplication.ElectionPositionId = electionPositionId;
                    positionApplication.PositionApplicationStatusId = pendingPaymentStatus.Id;
                    positionApplication.PositionApplicationStatus = pendingPaymentStatus;

                    await _positionApplicationRepo.AddAsync(positionApplication);
                }

                invoiceRequest.PositionApplicationId = positionApplication.Id;

                Result<InvoiceResponse> invoiceResult = await _paymentService.CreateInvoice(invoiceRequest);

                if (!invoiceResult.IsSuccess || invoiceResult.Value is null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    transactionStarted = false;

                    int invoiceStatusCode = GetStatusCode(invoiceResult.Status);

                    idempotencyRecord.Status = ApplicationConstants.IdempotencyStatuses.Failed;
                    idempotencyRecord.ResourceId = existingApplication ? positionApplication.Id : null;
                    idempotencyRecord.StatusCode = invoiceStatusCode;
                    idempotencyRecord.Response = invoiceResult.Error;

                    await _idempotencyRecordRepo.UpdateAsync(idempotencyRecord);

                    _loggerMessage.LogWarn($"Invoice processing failed for position application id {positionApplication.Id}, applicant {applicantName}, registration number {student.RegNumber}, student id {student.Id}, user id {userId}. Error: {invoiceResult.Error}");

                    return Result<CreatePositionApplicationResponse>.FromFailure(invoiceResult);
                }

                InvoiceResponse invoiceResponse = invoiceResult.Value;

                CreatePositionApplicationResponse response = _mapper.Map<CreatePositionApplicationResponse>(positionApplication);
                response.InvoiceId = invoiceResponse.Id;
                response.InvoiceNumber = invoiceResponse.InvoiceNumber;
                response.Amount = invoiceResponse.Amount;
                response.Currency = invoiceResponse.Currency;
                response.InvoiceStatus = invoiceResponse.Status;

                idempotencyRecord.Status = ApplicationConstants.IdempotencyStatuses.Completed;
                idempotencyRecord.ResourceId = positionApplication.Id;
                idempotencyRecord.StatusCode = existingApplication ? StatusCodes.Status200OK : StatusCodes.Status201Created;
                idempotencyRecord.Response = JsonSerializer.Serialize(response);

                await _idempotencyRecordRepo.UpdateAsync(idempotencyRecord);

                await _unitOfWork.CommitTransactionAsync();
                transactionStarted = false;

                await _cacheService.RemoveByTag(CacheTags.PositionApplication);

                if (existingApplication)
                {
                    _loggerMessage.LogInfo($"Existing position application with id {positionApplication.Id} returned successfully for {applicantName}, registration number {student.RegNumber}, student id {student.Id}, user id {userId}.");
                    return Result<CreatePositionApplicationResponse>.Success(response);
                }

                _loggerMessage.LogInfo($"Position application with id {positionApplication.Id} created successfully for {applicantName}, registration number {student.RegNumber}, student id {student.Id}, user id {userId}.");

                return Result<CreatePositionApplicationResponse>.Created(response);
            }
            catch
            {
                if (transactionStarted)
                    await _unitOfWork.RollbackTransactionAsync();

                try
                {
                    idempotencyRecord.Status = ApplicationConstants.IdempotencyStatuses.Failed;
                    idempotencyRecord.ResourceId = existingApplication ? positionApplication?.Id : null;
                    idempotencyRecord.StatusCode = StatusCodes.Status500InternalServerError;
                    idempotencyRecord.Response = "Position application creation failed.";

                    await _idempotencyRecordRepo.UpdateAsync(idempotencyRecord);
                }
                catch
                {
                    _loggerMessage.LogError($"Failed to update idempotency record {idempotencyRecord.Id} after position application creation failed for user id {userId}.");
                }

                _loggerMessage.LogError($"Position application creation failed for {applicantName}, registration number {student.RegNumber}, student id {student.Id}, user id {userId}, for election position id {electionPositionId}.");

                throw;
            }
        }

        public async Task<Result<string>> CancelPositionApplication(string positionApplicationId)
        {
            string? userId = _currentUserContext.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                _loggerMessage.LogWarn("Position application cancellation failed because the current user id was not available.");
                return Result<string>.ValidationError("The current user could not be identified.");
            }

            string normalizedPositionApplicationId = positionApplicationId.Trim();

            _loggerMessage.LogInfo($"Position application cancellation request received from user id {userId} for position application id {normalizedPositionApplicationId}.");

            PositionApplication? positionApplication = await _positionApplicationRepo.GetSingleByAsync(x => x.Id == normalizedPositionApplicationId,
                include: query => query.Include(x => x.Student).Include(x => x.PositionApplicationStatus).Include(x => x.Invoice)
                    .ThenInclude(x => x.InvoiceStatus), tracking: true);

            if (positionApplication is null)
            {
                _loggerMessage.LogWarn($"Position application cancellation failed for user id {userId} because position application with id {normalizedPositionApplicationId} was not found.");
                return Result<string>.NotFound($"Position application with id {normalizedPositionApplicationId} was not found.");
            }

            if (positionApplication.Student.UserId != userId)
            {
                _loggerMessage.LogWarn($"Position application cancellation denied for user id {userId} because position application with id {normalizedPositionApplicationId} belongs to another student.");
                return Result<string>.Forbidden("You are not allowed to cancel this position application.");
            }

            if (positionApplication.PositionApplicationStatus.Code == ApplicationConstants.PositionApplicationStatuses.Withdrawn)
            {
                _loggerMessage.LogInfo($"Position application with id {normalizedPositionApplicationId} has already been withdrawn by user id {userId}.");
                return Result<string>.Success("Position application has already been cancelled.");
            }

            if (positionApplication.PositionApplicationStatus.Code != ApplicationConstants.PositionApplicationStatuses.PendingPayment)
            {
                _loggerMessage.LogWarn($"Position application cancellation failed for user id {userId} because position application with id {normalizedPositionApplicationId} " +
                    $"has status {positionApplication.PositionApplicationStatus.Code}.");
                return Result<string>.Conflict("Only a position application awaiting payment can be cancelled.");
            }

            if (positionApplication.Invoice is null)
            {
                _loggerMessage.LogWarn($"Position application cancellation failed for user id {userId} because position application with id {normalizedPositionApplicationId} does not have an invoice.");
                return Result<string>.NotFound("The invoice for the position application was not found.");
            }

            if (positionApplication.Invoice.InvoiceStatus.Code == ApplicationConstants.InvoiceStatuses.Paid)
            {
                _loggerMessage.LogWarn($"Position application cancellation failed for user id {userId} because invoice {positionApplication.Invoice.Id} has already been paid.");
                return Result<string>.Conflict("A paid position application cannot be cancelled.");
            }

            if (positionApplication.Invoice.InvoiceStatus.Code != ApplicationConstants.InvoiceStatuses.Unpaid)
            {
                _loggerMessage.LogWarn($"Position application cancellation failed for user id {userId} because invoice {positionApplication.Invoice.Id} " +
                    $"has status {positionApplication.Invoice.InvoiceStatus.Code}.");
                return Result<string>.Conflict("The invoice is not available for cancellation.");
            }

            PositionApplicationStatus? withdrawnStatus = await _positionApplicationStatusRepo.GetSingleByAsync(x => x.Code == ApplicationConstants.PositionApplicationStatuses.Withdrawn);
            if (withdrawnStatus is null)
            {
                _loggerMessage.LogWarn($"Position application cancellation failed for user id {userId} because the Withdrawn position application status was not found.");
                return Result<string>.NotFound("Withdrawn position application status was not found.");
            }

            InvoiceStatus? cancelledInvoiceStatus = await _invoiceStatusRepo.GetSingleByAsync(x => x.Code == ApplicationConstants.InvoiceStatuses.Cancelled);
            if (cancelledInvoiceStatus is null)
            {
                _loggerMessage.LogWarn($"Position application cancellation failed for user id {userId} because the Cancelled invoice status was not found.");
                return Result<string>.NotFound("Cancelled invoice status was not found.");
            }

            positionApplication.PositionApplicationStatusId = withdrawnStatus.Id;
            positionApplication.Invoice.InvoiceStatusId = cancelledInvoiceStatus.Id;

            _positionApplicationRepo.Update(positionApplication);
            _invoiceRepo.Update(positionApplication.Invoice);

            await _unitOfWork.SaveChangesAsync();

            await _cacheService.RemoveByTag(CacheTags.PositionApplication);

            _loggerMessage.LogInfo($"Position application with id {normalizedPositionApplicationId} was cancelled successfully by user id {userId}. Invoice with id {positionApplication.Invoice.Id} was also cancelled.");

            return Result<string>.Success("Position application cancelled successfully.");
        }

        public async Task<Result<PagedResponse<PositionApplicationResponse>>> GetMyPositionApplications(PositionApplicationRequest request)
        {
            string? userId = _currentUserContext.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                _loggerMessage.LogWarn("Position application list request failed because the current user id was not available.");
                return Result<PagedResponse<PositionApplicationResponse>>.ValidationError("The current user could not be identified.");
            }

            _loggerMessage.LogInfo($"Position application list request received from user id {userId} for page {request.PageNumber}.");

            string cacheKey = PositionApplicationCacheKeys.GetMyPositionApplications(userId, request);

            Func<CancellationToken, ValueTask<PagedResponse<PositionApplicationResponse>>> cacheFactory = async _ =>
            {
                IQueryable<PositionApplication> query = _positionApplicationRepo.GetQueryable(x => x.Student.UserId == userId,
                    include: query => query.Include(x => x.Student).Include(x => x.PositionApplicationStatus)
                        .Include(x => x.Invoice).ThenInclude(x => x.InvoiceStatus).Include(x => x.ElectionPosition).ThenInclude(x => x.Election)
                        .Include(x => x.ElectionPosition).ThenInclude(x => x.Position)).AsNoTracking();

                if (request.PositionApplicationStatusId.HasValue)
                    query = query.Where(x => x.PositionApplicationStatusId == request.PositionApplicationStatusId.Value);

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    string searchTerm = request.SearchTerm.Trim();

                    query = query.Where(x => x.ElectionPosition.Election.Name.Contains(searchTerm) || x.ElectionPosition.Position.Name.Contains(searchTerm)
                        || x.PositionApplicationStatus.Name.Contains(searchTerm) || (x.Invoice != null && x.Invoice.InvoiceNumber.Contains(searchTerm)));
                }

                query = query.OrderByDescending(x => x.CreatedAt);

                PagedList<PositionApplication> positionApplications = await query.GetPagedItems(request);

                PagedResponse<PositionApplicationResponse> response = _mapper.Map<PagedResponse<PositionApplicationResponse>>(positionApplications);

                _loggerMessage.LogInfo($"{positionApplications.MetaData.TotalCount} position applications found for user id {userId}.");

                return response;
            };

            PagedResponse<PositionApplicationResponse> response = await _cacheService.GetOrCreate(cacheKey, cacheFactory, CachePolicies.PositionApplication);

            return Result<PagedResponse<PositionApplicationResponse>>.Success(response);
        }

        public async Task<Result<PositionApplicationResponse>> GetPositionApplication(string positionApplicationId)
        {
            string? userId = _currentUserContext.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                _loggerMessage.LogWarn("Position application request failed because the current user id was not available.");
                return Result<PositionApplicationResponse>.ValidationError("The current user could not be identified.");
            }

            ApplicationUserRole? applicationUserRole = await _unitOfWork.GetRepository<ApplicationUserRole>().GetSingleByAsync(x => x.UserId == userId && x.Active,
                include: query => query.Include(x => x.Role), tracking: false);

            if (applicationUserRole is null)
            {
                _loggerMessage.LogWarn($"Position application request failed because no active role was found for user id {userId}.");
                return Result<PositionApplicationResponse>.Forbidden("You are not allowed to view this position application.");
            }

            string normalizedPositionApplicationId = positionApplicationId.Trim();
            string roleName = applicationUserRole.Role.Name ?? string.Empty;

            PositionApplication? positionApplication;

            if (roleName == ApplicationConstants.RoleNames.Student)
            {
                positionApplication = await _positionApplicationRepo.GetSingleByAsync(x => x.Id == normalizedPositionApplicationId && x.Student.UserId == userId,
                    include: query => query.Include(x => x.Student).ThenInclude(x => x.User)
                        .Include(x => x.PositionApplicationStatus)
                        .Include(x => x.Invoice).ThenInclude(x => x.InvoiceStatus)
                        .Include(x => x.ElectionPosition).ThenInclude(x => x.Election)
                        .Include(x => x.ElectionPosition).ThenInclude(x => x.Position),
                    tracking: false);
            }
            else if (roleName == ApplicationConstants.RoleNames.Admin || roleName == ApplicationConstants.RoleNames.SuperAdmin)
            {
                positionApplication = await _positionApplicationRepo.GetSingleByAsync(x => x.Id == normalizedPositionApplicationId,
                    include: query => query.Include(x => x.Student).ThenInclude(x => x.User)
                        .Include(x => x.PositionApplicationStatus)
                        .Include(x => x.Invoice).ThenInclude(x => x.InvoiceStatus)
                        .Include(x => x.ElectionPosition).ThenInclude(x => x.Election)
                        .Include(x => x.ElectionPosition).ThenInclude(x => x.Position),
                    tracking: false);
            }
            else
            {
                _loggerMessage.LogWarn($"User with id {userId} and role {roleName} attempted to access position application with id {normalizedPositionApplicationId}.");
                return Result<PositionApplicationResponse>.Forbidden("You are not allowed to view this position application.");
            }

            if (positionApplication is null)
            {
                _loggerMessage.LogWarn($"Position application with id {normalizedPositionApplicationId} was not found for user id {userId}.");
                return Result<PositionApplicationResponse>.NotFound($"Position application with id {normalizedPositionApplicationId} was not found.");
            }

            PositionApplicationResponse response = _mapper.Map<PositionApplicationResponse>(positionApplication);

            _loggerMessage.LogInfo($"Position application with id {normalizedPositionApplicationId} found for user id {userId}.");

            return Result<PositionApplicationResponse>.Success(response);
        }

        public async Task<Result<PagedResponse<PositionApplicationResponse>>> GetPositionApplications(PositionApplicationRequest request)
        {
            _loggerMessage.LogInfo($"Position application list request received for page {request.PageNumber}.");

            IQueryable<PositionApplication> query = _positionApplicationRepo.GetQueryable(include: query => query
                .Include(x => x.Student).ThenInclude(x => x.User)
                .Include(x => x.PositionApplicationStatus)
                .Include(x => x.Invoice).ThenInclude(x => x.InvoiceStatus)
                .Include(x => x.ElectionPosition).ThenInclude(x => x.Election)
                .Include(x => x.ElectionPosition).ThenInclude(x => x.Position)).AsNoTracking();

            if (request.PositionApplicationStatusId.HasValue)
                query = query.Where(x => x.PositionApplicationStatusId == request.PositionApplicationStatusId.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                string searchTerm = request.SearchTerm.Trim();

                query = query.Where(x => x.ElectionPosition.Election.Name.Contains(searchTerm)
                    || x.ElectionPosition.Position.Name.Contains(searchTerm)
                    || x.PositionApplicationStatus.Name.Contains(searchTerm)
                    || (x.Invoice != null && x.Invoice.InvoiceNumber.Contains(searchTerm))
                    || (x.Student.RegNumber != null && x.Student.RegNumber.Contains(searchTerm))
                    || (x.Student.User != null && ((x.Student.User.FirstName != null && x.Student.User.FirstName.Contains(searchTerm))
                        || (x.Student.User.LastName != null && x.Student.User.LastName.Contains(searchTerm))
                        || (x.Student.User.Email != null && x.Student.User.Email.Contains(searchTerm)))));
            }

            query = query.OrderByDescending(x => x.CreatedAt);

            PagedList<PositionApplication> positionApplications = await query.GetPagedItems(request);

            PagedResponse<PositionApplicationResponse> response = _mapper.Map<PagedResponse<PositionApplicationResponse>>(positionApplications);

            _loggerMessage.LogInfo($"{positionApplications.MetaData.TotalCount} position applications found.");

            return Result<PagedResponse<PositionApplicationResponse>>.Success(response);
        }

        public async Task<Result<string>> ApproveOrRejectPositionApplication(ApproveOrRejectPositionApplicationRequest request)
        {
            string? userId = _currentUserContext.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                _loggerMessage.LogWarn("Position application approval or rejection failed because the current user id was not available.");
                return Result<string>.ValidationError("The current user could not be identified.");
            }

            ApplicationUserRole? applicationUserRole = await _unitOfWork.GetRepository<ApplicationUserRole>().GetSingleByAsync(x => x.UserId == userId && x.Active,
                include: query => query.Include(x => x.Role), tracking: false);

            if (applicationUserRole is null)
            {
                _loggerMessage.LogWarn($"Position application approval or rejection failed because no active role was found for user id {userId}.");
                return Result<string>.Forbidden("You are not allowed to approve or reject position applications.");
            }

            string roleName = applicationUserRole.Role.Name ?? string.Empty;

            if (roleName != ApplicationConstants.RoleNames.Admin
                && roleName != ApplicationConstants.RoleNames.SuperAdmin)
            {
                _loggerMessage.LogWarn($"User with id {userId} and role {roleName} attempted to approve or reject a position application.");
                return Result<string>.Forbidden("You are not allowed to approve or reject position applications.");
            }

            string normalizedPositionApplicationId = request.PositionApplicationId.Trim();

            bool transactionStarted = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                transactionStarted = true;

                PositionApplicationStatus? requestedStatus = await _positionApplicationStatusRepo.GetSingleByAsync(x => x.Id == request.PositionApplicationStatusId && x.Active);
                if (requestedStatus is null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    transactionStarted = false;

                    _loggerMessage.LogWarn($"Position application status with id {request.PositionApplicationStatusId} was not found.");
                    return Result<string>.NotFound($"Position application status with id {request.PositionApplicationStatusId} was not found.");
                }

                if (requestedStatus.Code != ApplicationConstants.PositionApplicationStatuses.Approved
                    && requestedStatus.Code != ApplicationConstants.PositionApplicationStatuses.Rejected)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    transactionStarted = false;

                    _loggerMessage.LogWarn($"Position application status with id {requestedStatus.Id} and code {requestedStatus.Code} cannot be used to approve or reject an application.");
                    return Result<string>.ValidationError("The position application can only be approved or rejected.");
                }

                PositionApplication? positionApplication = await _positionApplicationRepo.GetSingleByAsync(x => x.Id == normalizedPositionApplicationId,
                    include: query => query.Include(x => x.PositionApplicationStatus).Include(x => x.Invoice).ThenInclude(x => x.InvoiceStatus)
                        .Include(x => x.Contestant), tracking: true);

                if (positionApplication is null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    transactionStarted = false;

                    _loggerMessage.LogWarn($"Position application with id {normalizedPositionApplicationId} was not found.");
                    return Result<string>.NotFound($"Position application with id {normalizedPositionApplicationId} was not found.");
                }

                if (positionApplication.PositionApplicationStatus.Code != ApplicationConstants.PositionApplicationStatuses.PendingReview)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    transactionStarted = false;

                    _loggerMessage.LogWarn($"Position application with id {normalizedPositionApplicationId} cannot be approved or rejected because its current status is {positionApplication.PositionApplicationStatus.Code}.");
                    return Result<string>.Conflict("Only position applications pending review can be approved or rejected.");
                }

                if (positionApplication.Invoice is null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    transactionStarted = false;

                    _loggerMessage.LogWarn($"Position application with id {normalizedPositionApplicationId} cannot be approved or rejected because it does not have an invoice.");
                    return Result<string>.Conflict("The position application does not have an invoice.");
                }

                if (positionApplication.Invoice.InvoiceStatus.Code != ApplicationConstants.InvoiceStatuses.Paid)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    transactionStarted = false;

                    _loggerMessage.LogWarn($"Position application with id {normalizedPositionApplicationId} cannot be approved or rejected because " +
                        $"invoice {positionApplication.Invoice.Id} has not been paid.");
                    return Result<string>.Conflict("The position application invoice must be paid before the application can be approved or rejected.");
                }

                DateTime currentDateTime = DateTime.UtcNow;

                if (requestedStatus.Code == ApplicationConstants.PositionApplicationStatuses.Approved)
                {
                    if (positionApplication.Contestant is not null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        transactionStarted = false;

                        _loggerMessage.LogWarn($"Position application with id {normalizedPositionApplicationId} already has contestant {positionApplication.Contestant.Id}.");

                        return Result<string>.Conflict("A contestant has already been created for this position application.");
                    }

                    Contestant contestant = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        PositionApplicationId = positionApplication.Id,
                        PositionApplication = positionApplication,
                        Active = true,
                        CreatedAt = currentDateTime,
                        UpdatedAt = currentDateTime,
                        CreatedBy = userId,
                        UpdatedBy = userId
                    };

                    _unitOfWork.GetRepository<Contestant>().Add(contestant);
                }

                positionApplication.PositionApplicationStatusId = requestedStatus.Id;
                positionApplication.PositionApplicationStatus = requestedStatus;
                positionApplication.UpdatedAt = currentDateTime;
                positionApplication.UpdatedBy = userId;

                _unitOfWork.GetRepository<PositionApplication>().Update(positionApplication);

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();
                transactionStarted = false;

                await _cacheService.RemoveByTag(CacheTags.PositionApplication);

                if (requestedStatus.Code == ApplicationConstants.PositionApplicationStatuses.Approved)
                {
                    _loggerMessage.LogInfo($"Position application with id {normalizedPositionApplicationId} was approved successfully by user id {userId}.");

                    return Result<string>.Success("Position application approved successfully.");
                }

                _loggerMessage.LogInfo($"Position application with id {normalizedPositionApplicationId} was rejected successfully by user id {userId}.");

                return Result<string>.Success("Position application rejected successfully.");
            }
            catch (Exception exception)
            {
                if (transactionStarted)
                    await _unitOfWork.RollbackTransactionAsync();

                _loggerMessage.LogError(exception,
                    $"An error occurred while approving or rejecting position application with id {normalizedPositionApplicationId}.");

                throw new InvalidOperationException(
                    $"An error occurred while approving or rejecting position application with id {normalizedPositionApplicationId}.",
                    exception);
            }
        }

        public async Task<Result<PagedResponse<PositionApplicationResponse>>> GetPositionApplicationsWithContestants(PositionApplicationWithContestantRequest request)
        {
            IQueryable<PositionApplication> query = _positionApplicationRepo.GetQueryable(x => x.Contestant != null,
                include: query => query.Include(x => x.Student).ThenInclude(x => x.User)
                    .Include(x => x.PositionApplicationStatus).Include(x => x.Invoice).ThenInclude(x => x.InvoiceStatus)
                    .Include(x => x.ElectionPosition).ThenInclude(x => x.Election).Include(x => x.ElectionPosition).ThenInclude(x => x.Position)
                    .Include(x => x.Contestant)).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.ElectionId))
                query = query.Where(x => x.ElectionPosition.ElectionId == request.ElectionId);

            if (!string.IsNullOrWhiteSpace(request.PositionId))
                query = query.Where(x => x.ElectionPosition.PositionId == request.PositionId);

            if (request.ContestantActive.HasValue)
                query = query.Where(x => x.Contestant != null && x.Contestant.Active == request.ContestantActive.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                string searchTerm = request.SearchTerm.Trim();

                query = query.Where(x => x.ElectionPosition.Election.Name.Contains(searchTerm)
                    || x.ElectionPosition.Position.Name.Contains(searchTerm)
                    || x.PositionApplicationStatus.Name.Contains(searchTerm)
                    || (x.Student.RegNumber != null && x.Student.RegNumber.Contains(searchTerm))
                    || (x.Student.User != null && ((x.Student.User.FirstName != null && x.Student.User.FirstName.Contains(searchTerm))
                        || (x.Student.User.LastName != null && x.Student.User.LastName.Contains(searchTerm))
                        || (x.Student.User.Email != null && x.Student.User.Email.Contains(searchTerm)))));
            }

            query = query.OrderByDescending(x => x.CreatedAt);

            PagedList<PositionApplication> positionApplications = await query.GetPagedItems(request);

            PagedResponse<PositionApplicationResponse> response = _mapper.Map<PagedResponse<PositionApplicationResponse>>(positionApplications);

            _loggerMessage.LogInfo($"{positionApplications.MetaData.TotalCount} position applications with contestants found.");

            return Result<PagedResponse<PositionApplicationResponse>>.Success(response);
        }

        private static string CreateRequestHash(string electionPositionId)
        {
            string requestValue = electionPositionId.Trim().ToLowerInvariant();
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(requestValue));

            return Convert.ToHexString(hash);
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