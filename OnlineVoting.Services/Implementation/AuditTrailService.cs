using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Data.Interfaces;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
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
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerMessage _loggerMessage;

        public AuditTrailService(IServiceFactory serviceFactory)
        {
            _unitOfWork = serviceFactory.GetService<IUnitOfWork>();
            _auditTrailRepo = _unitOfWork.GetRepository<AuditTrail>();
            _mapper = serviceFactory.GetService<IMapper>();
            _loggerMessage = serviceFactory.GetService<ILoggerMessage>();
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
    }
}