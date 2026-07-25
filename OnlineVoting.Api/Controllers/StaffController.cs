using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Api.Documentation.Attributes;
using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Enums;
using OnlineVoting.Models.GlobalMessage;
using OnlineVoting.Models.Pagination;
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
            IEnumerable<StaffResponse> allStaff = await _staffService.GetAllActiveStaff();

            if (allStaff.Any())
                return Ok(allStaff);

            return BadRequest(new ResponseError { Status = ResponseStatus.NOT_FOUND, Message = $"No User found" });
        }

        [HttpGet("all-deleted-staff", Name = "All-Deleted-Staff")]
        [ApiDocumentation(StaffDocumentationKeys.GetAllDeletedStaff)]
        public async Task<IActionResult> GetAllDeletedStaff()
        {
            IEnumerable<StaffResponse> allStaff = await _staffService.GetAllDeletedStaff();

            if (allStaff.Any())
                return Ok(allStaff);

            return BadRequest(new ResponseError { Status = ResponseStatus.NOT_FOUND, Message = $"No User found" });
        }

        [HttpGet("all-paged-staff", Name = "All-Paged-Staff")]
        [ApiDocumentation(StaffDocumentationKeys.GetAllPagedStaff)]
        public async Task<IActionResult> AllPagedStaff(StaffRequest request)
        {
            PagedResponse<StaffResponse> allStaff = await _staffService.AllStaff(request);

            return Ok(allStaff);
        }

        [HttpGet("all-paged-active-staff", Name = "All-Paged-Active-Staff")]
        [ApiDocumentation(StaffDocumentationKeys.GetAllPagedActiveStaff)]
        public async Task<IActionResult> AllPagedActiveStaff(StaffRequest request)
        {
            PagedResponse<StaffResponse> allStaff = await _staffService.AllActiveStaff(request);

            return Ok(allStaff);
        }

        [HttpGet("all-paged-deleted-staff", Name = "All-Paged-Deleted-Staff")]
        [ApiDocumentation(StaffDocumentationKeys.GetAllPagedDeletedStaff)]
        public async Task<IActionResult> AllPagedDeletedStaff(StaffRequest request)
        {
            PagedResponse<StaffResponse> allStaff = await _staffService.AllDeletedStaff(request);

            return Ok(allStaff);
        }


        [HttpGet("staff-by-id", Name = "Staff-By-Id")]
        [ApiDocumentation(StaffDocumentationKeys.GetStaffById)]
        public async Task<IActionResult> GetStaffById(Guid id)
        {
            StaffResponse staff = await _staffService.GetStaff(id);

            return Ok(staff);
        }

        [HttpGet("staff-by-email", Name = "Staff-By-Email")]
        [ApiDocumentation(StaffDocumentationKeys.GetStaffByEmail)]
        public async Task<IActionResult> GetStaffByEmail(string email)
        {
            StaffResponse staff = await _staffService.GetStaffByEmail(email);

            return Ok(staff);
        }

        [HttpPost("create-staff", Name = "Create-Staff")]
        [ApiDocumentation(StaffDocumentationKeys.CreateStaff)]
        public async Task<IActionResult> CreateStaff([FromQuery] CreateStaffRequest model)
        {
            string staff = await _staffService.CreateStaff(model);

            return Ok(staff);
        }

        [HttpPatch("update-staff", Name = "Update-Staff")]
        [ApiDocumentation(StaffDocumentationKeys.UpdateStaff)]
        public async Task<IActionResult> UpdateStaff([FromQuery] Guid id, [FromBody] JsonPatchDocument<UpdateStaffRequest> model)
        {
            string staff = await _staffService.UpdateStaff(id, model);

            return Ok(staff);
        }

        [HttpPut("edit-staff", Name = "Edit-Staff")]
        [ApiDocumentation(StaffDocumentationKeys.EditStaff)]
        public async Task<IActionResult> EditStaff([FromQuery] Guid staffId, [FromBody] UpdateStaffRequest model)
        {
            string staff =  await _staffService.EditStaff(staffId, model);

            return Ok(staff);
        }

        [HttpPatch("patch-staff-address", Name = "Patch-Staff-Address")]
        [ApiDocumentation(StaffDocumentationKeys.PatchStaffAddress)]
        public async Task<IActionResult> PatchStaffAddress([FromQuery] Guid id, [FromBody] JsonPatchDocument<UpdateAddressRequest> model)
        {
            string staff =  await _staffService.PatchStaffAddress(id, model);

            return Ok(staff);
        }

        [HttpPut("toggle-staff-status", Name = "Toggle-Staff-Status")]
        [ApiDocumentation(StaffDocumentationKeys.ToggleStaffStatus)]
        public async Task<IActionResult> ToggleStaffStatus([FromQuery] Guid staffId)
        {
            string staff = await _staffService.ToggleStaffStatus(staffId);

            return Ok(staff);
        }

        [HttpPut("update-staff-address", Name = "Update-Staff-Address")]
        [ApiDocumentation(StaffDocumentationKeys.UpdateStaffAddress)]
        public async Task<IActionResult> UpdateStaffAddress([FromQuery] Guid staffId, [FromBody] UpdateAddressRequest model)
        {
            string staff = await _staffService.UpdateStaffAddress(staffId, model);

            return Ok(staff);
        }

        [HttpGet("total-number-of-staff", Name = "Total-Number-Of-Staff")]
        [ApiDocumentation(StaffDocumentationKeys.GetTotalNumberOfStaff)]
        public IActionResult GetTotalNumberOfStaff()
        {
            int staff = _staffService.GetTotalNumberOfStaff().Count();

            if (staff <= 0)
                return BadRequest(new ResponseError { Status = ResponseStatus.NOT_FOUND, Message = $"0 Staff found" });

            return Ok($"{staff} Staff");
        }

        [HttpDelete("delete-staff-by-id", Name = "Delete-Staff-By-Id")]
        [ApiDocumentation(StaffDocumentationKeys.DeleteStaff)]
        public async Task<IActionResult> DeleteStaff([FromQuery] Guid id)
        {
            string staff = await _staffService.DeleteStaffById(id);

            return Ok(staff);
        }
    }
}