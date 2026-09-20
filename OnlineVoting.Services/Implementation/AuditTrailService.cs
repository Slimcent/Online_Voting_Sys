using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Interfaces;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;
using VotingSystem.Data.Extensions;
using VotingSystem.Logger;

namespace OnlineVoting.Services.Implementation
{
    public class AuditTrailService : IAuditTrailService
    {
        private readonly IRepository<AuditTrail> _auditTrailRepo;
        private readonly IRepository<AuditOutcome> _auditOutcomeRepo;
        private readonly IAuditMetadataProvider _auditMetadataProvider;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerMessage _loggerMessage;

        public AuditTrailService(IServiceFactory serviceFactory)
        {
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _auditTrailRepo = _unitOfWork.GetRepository<AuditTrail>();
            _auditOutcomeRepo = _unitOfWork.GetRepository<AuditOutcome>();
            _mapper = serviceFactory.GetService<IMapper>();
            _loggerMessage = serviceFactory.GetService<ILoggerMessage>();
            _auditMetadataProvider = serviceFactory.GetService<IAuditMetadataProvider>();
        }

        public async Task<Result<PagedResponse<AuditTrailResponse>>> GetAuditTrails(AuditTrailRequest request)
        {
            _loggerMessage.LogInfo($"Audit trail list request received for page {request.PageNumber}.");

            IQueryable<AuditTrail> query = _auditTrailRepo.GetQueryable(include: query => query.Include(auditTrail => auditTrail.Outcome)
                .Include(auditTrail => auditTrail.Location)).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.ActorUserId))
                query = query.Where(auditTrail => auditTrail.ActorUserId == request.ActorUserId);

            if (!string.IsNullOrWhiteSpace(request.ActorUsername))
                query = query.Where(auditTrail => auditTrail.ActorUsername == request.ActorUsername);

            if (!string.IsNullOrWhiteSpace(request.EndpointName))
                query = query.Where(auditTrail => auditTrail.EndpointName == request.EndpointName);

            if (!string.IsNullOrWhiteSpace(request.EventName))
                query = query.Where(auditTrail => auditTrail.EventName == request.EventName);

            if (!string.IsNullOrWhiteSpace(request.EntityType))
                query = query.Where(auditTrail => auditTrail.EntityType == request.EntityType);

            if (!string.IsNullOrWhiteSpace(request.EntityId))
                query = query.Where(auditTrail => auditTrail.EntityId == request.EntityId);

            if (!string.IsNullOrWhiteSpace(request.Outcome))
                query = query.Where(auditTrail => auditTrail.Outcome.Name == request.Outcome);

            if (!string.IsNullOrWhiteSpace(request.CorrelationId))
                query = query.Where(auditTrail => auditTrail.CorrelationId == request.CorrelationId);

            if (!string.IsNullOrWhiteSpace(request.IpAddress))
                query = query.Where(auditTrail => auditTrail.IpAddress == request.IpAddress);

            if (request.From.HasValue)
                query = query.Where(auditTrail => auditTrail.CreatedAt >= request.From.Value);

            if (request.To.HasValue)
                query = query.Where(auditTrail => auditTrail.CreatedAt <= request.To.Value);
                        
            PagedList<AuditTrail> pagedAuditTrails = await query.GetPagedItems(request);

            PagedResponse<AuditTrailResponse> response = _mapper.Map<PagedResponse<AuditTrailResponse>>(pagedAuditTrails);

            _loggerMessage.LogInfo($"{pagedAuditTrails.MetaData.TotalCount} audit trail records found.");

            return Result<PagedResponse<AuditTrailResponse>>.Success(response);
        }

        public async Task RecordEvent(AuditEventRequest request)
        {
            AuditOutcome? auditOutcome = await _auditOutcomeRepo.GetSingleByAsync(
                auditOutcome => auditOutcome.Name == request.Outcome);

            if (auditOutcome is null)
                throw new InvalidOperationException($"Audit outcome '{request.Outcome}' was not found.");

            AuditTrail auditTrail = _mapper.Map<AuditTrail>(request);

            auditTrail.ActorUserId ??= _auditMetadataProvider.GetActorUserId();
            auditTrail.ActorUsername ??= _auditMetadataProvider.GetActorUsername();
            auditTrail.OutcomeId = auditOutcome.Id;
            auditTrail.EndpointName = _auditMetadataProvider.GetEndpointName();
            auditTrail.HttpMethod = _auditMetadataProvider.GetHttpMethod();
            auditTrail.IpAddress = _auditMetadataProvider.GetIpAddress();
            auditTrail.UserAgent = _auditMetadataProvider.GetUserAgent();
            auditTrail.CorrelationId = _auditMetadataProvider.GetCorrelationId();

            AuditLocation auditLocation = new()
            {
                AuditTrailId = auditTrail.Id,
                IpCountry = _auditMetadataProvider.GetIpCountry(),
                IpRegion = _auditMetadataProvider.GetIpRegion(),
                IpCity = _auditMetadataProvider.GetIpCity(),
                IpLatitude = _auditMetadataProvider.GetIpLatitude(),
                IpLongitude = _auditMetadataProvider.GetIpLongitude(),
                DeviceLatitude = _auditMetadataProvider.GetDeviceLatitude(),
                DeviceLongitude = _auditMetadataProvider.GetDeviceLongitude(),
                DeviceAccuracyMeters = _auditMetadataProvider.GetDeviceAccuracyMeters(),
                DeviceLocationCapturedAt = _auditMetadataProvider.GetDeviceLocationCapturedAt()
            };

            auditTrail.Location = auditLocation;

           await _auditTrailRepo.AddAsync(auditTrail);
        }

        public async Task RecordAuthenticationEvent(string eventName, string outcome, string description, User? user = null, string? attemptedUsername = null)
        {
            AuditEventRequest request = new()
            {
                EventName = eventName,
                Outcome = outcome,
                Description = description,
                ActorUserId = user?.Id,
                ActorUsername = user?.Email ?? attemptedUsername,
                EntityType = ApplicationConstants.Audit.EntityTypes.User,
                EntityId = user?.Id
            };

            await RecordEvent(request);
        }
    }
}