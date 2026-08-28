using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Api.Documentation.Attributes;
using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Extensions;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/audit-trails")]
    [ApiController]
    [Authorize(Policy = "Authorization")]
    public class AuditTrailController : BaseController
    {
        private readonly IAuditTrailService _auditTrailService;

        public AuditTrailController(IAuditTrailService auditTrailService)
        {
            _auditTrailService = auditTrailService;
        }

        [HttpGet("audit-trails", Name = "Get-Audit-Trails")]
        [ApiDocumentation(AuditTrailDocumentationKeys.GetAuditTrails)]
        public async Task<IActionResult> GetAuditTrails([FromQuery] AuditTrailRequest request)
        {
            Result<PagedResponse<AuditTrailResponse>> result = await _auditTrailService.GetAuditTrails(request);

            return result.ToActionResult(this);
        }
    }
}