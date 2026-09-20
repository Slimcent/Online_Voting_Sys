using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces
{
    public interface IAuditTrailService
    {
        Task<Result<PagedResponse<AuditTrailResponse>>> GetAuditTrails(AuditTrailRequest request);
        Task RecordEvent(AuditEventRequest request);
        Task RecordAuthenticationEvent(string eventName, string outcome, string description, User? user = null, string? attemptedUsername = null);
    }
}