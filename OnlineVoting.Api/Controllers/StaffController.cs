using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Api.Documentation.Attributes;
using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Extensions;
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
    public class StaffController : BaseController
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        //[HttpGet("all-staff", Name = "All-Staff")]
        //public async Task<IActionResult> GetAllStaff()
        //{
        //    IEnumerable<StaffResponseDto> allStaff = await _staffService.GetAllStaff();

        //    if (allStaff.Any())
        //        return Ok(allStaff);

        //    return BadRequest(new ResponseError { Status = ResponseStatus.NOT_FOUND, Message = $"No User found" });
        //}

        [HttpGet("all-active-staff", Name = "All-Active-Staff")]
        [ApiDocumentation(StaffDocumentationKeys.GetAllActiveStaff)]
        public async Task<IActionResult> GetAllActiveStaff()
        {
            Result<IEnumerable<StaffResponse>> result = await _staffService.GetAllActiveStaff();

            return result.ToActionResult(this);
        }

        [HttpGet("all-deleted-staff", Name = "All-Deleted-Staff")]
        [ApiDocumentation(StaffDocumentationKeys.GetAllDeletedStaff)]
        public async Task<IActionResult> GetAllDeletedStaff()
        {
            Result<IEnumerable<StaffResponse>> result = await _staffService.GetAllDeletedStaff();

            return result.ToActionResult(this);
        }

        [HttpGet("all-paged-staff", Name = "All-Paged-Staff")]
        [ApiDocumentation(StaffDocumentationKeys.GetAllPagedStaff)]
        public async Task<IActionResult> AllPagedStaff([FromQuery] StaffRequest request)
        {
            Result<PagedResponse<StaffResponse>> result = await _staffService.AllStaff(request);

            return result.ToActionResult(this);
        }

        [HttpGet("all-paged-active-staff", Name = "All-Paged-Active-Staff")]
        [ApiDocumentation(StaffDocumentationKeys.GetAllPagedActiveStaff)]
        public async Task<IActionResult> AllPagedActiveStaff([FromQuery] StaffRequest request)
        {
            Result<PagedResponse<StaffResponse>> result = await _staffService.AllActiveStaff(request);

            return result.ToActionResult(this);
        }

        [HttpGet("all-paged-deleted-staff", Name = "All-Paged-Deleted-Staff")]
        [ApiDocumentation(StaffDocumentationKeys.GetAllPagedDeletedStaff)]
        public async Task<IActionResult> AllPagedDeletedStaff([FromQuery] StaffRequest request)
        {
            Result<PagedResponse<StaffResponse>> result = await _staffService.AllDeletedStaff(request);

            return result.ToActionResult(this);
        }

        [HttpGet("staff-by-id", Name = "Staff-By-Id")]
        [ApiDocumentation(StaffDocumentationKeys.GetStaffById)]
        public async Task<IActionResult> GetStaffById([FromQuery] Guid id)
        {
            Result<StaffResponse> result = await _staffService.GetStaff(id);

            return result.ToActionResult(this);
        }

        [HttpGet("staff-by-email", Name = "Staff-By-Email")]
        [ApiDocumentation(StaffDocumentationKeys.GetStaffByEmail)]
        public async Task<IActionResult> GetStaffByEmail([FromQuery] string email)
        {
            Result<StaffResponse> result = await _staffService.GetStaffByEmail(email);

            return result.ToActionResult(this);
        }

        [HttpPost("create-staff", Name = "Create-Staff")]
        [ApiDocumentation(StaffDocumentationKeys.CreateStaff)]
        public async Task<IActionResult> CreateStaff([FromQuery] CreateStaffRequest model)
        {
            Result<string> result = await _staffService.CreateStaff(model);

            return result.ToActionResult(this);
        }

        [HttpPatch("update-staff", Name = "Update-Staff")]
        [ApiDocumentation(StaffDocumentationKeys.UpdateStaff)]
        public async Task<IActionResult> UpdateStaff([FromQuery] Guid id, [FromBody] JsonPatchDocument<UpdateStaffRequest> model)
        {
            Result<string> result = await _staffService.UpdateStaff(id, model);

            return result.ToActionResult(this);
        }

        [HttpPut("edit-staff", Name = "Edit-Staff")]
        [ApiDocumentation(StaffDocumentationKeys.EditStaff)]
        public async Task<IActionResult> EditStaff([FromQuery] Guid staffId, [FromBody] UpdateStaffRequest model)
        {
            Result<string> result = await _staffService.EditStaff(staffId, model);

            return result.ToActionResult(this);
        }

        [HttpPatch("patch-staff-address", Name = "Patch-Staff-Address")]
        [ApiDocumentation(StaffDocumentationKeys.PatchStaffAddress)]
        public async Task<IActionResult> PatchStaffAddress([FromQuery] Guid id, [FromBody] JsonPatchDocument<UpdateAddressRequest> model)
        {
            Result<string> result = await _staffService.PatchStaffAddress(id, model);

            return result.ToActionResult(this);
        }

        [HttpPut("toggle-staff-status", Name = "Toggle-Staff-Status")]
        [ApiDocumentation(StaffDocumentationKeys.ToggleStaffStatus)]
        public async Task<IActionResult> ToggleStaffStatus([FromQuery] Guid staffId)
        {
            Result<string> result = await _staffService.ToggleStaffStatus(staffId);

            return result.ToActionResult(this);
        }

        [HttpPut("update-staff-address", Name = "Update-Staff-Address")]
        [ApiDocumentation(StaffDocumentationKeys.UpdateStaffAddress)]
        public async Task<IActionResult> UpdateStaffAddress([FromQuery] Guid staffId, [FromBody] UpdateAddressRequest model)
        {
            Result<string> result = await _staffService.UpdateStaffAddress(staffId, model);

            return result.ToActionResult(this);
        }

        [HttpGet("total-number-of-staff", Name = "Total-Number-Of-Staff")]
        [ApiDocumentation(StaffDocumentationKeys.GetTotalNumberOfStaff)]
        public IActionResult GetTotalNumberOfStaff()
        {
            Result<int> result = _staffService.GetTotalNumberOfStaff();

            return result.ToActionResult(this);
        }

        [HttpDelete("delete-staff-by-id", Name = "Delete-Staff-By-Id")]
        [ApiDocumentation(StaffDocumentationKeys.DeleteStaff)]
        public async Task<IActionResult> DeleteStaff([FromQuery] Guid id)
        {
            Result<string> result = await _staffService.DeleteStaffById(id);

            return result.ToActionResult(this);
        }
    }
}