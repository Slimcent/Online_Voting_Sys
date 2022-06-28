using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Enums;
using OnlineVoting.Models.GlobalMessage;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        [HttpGet("all-staff", Name = "All-Staff")]
        public async Task<IActionResult> GetAllStaff()
        {
            IEnumerable<StaffResponseDto> allStaff = await _staffService.GetAllStaff();

            if (allStaff.Any())
                return Ok(allStaff);

            return BadRequest(new ErrorResponse { Status = ResponseStatus.NOT_FOUND, Message = $"No User found" });
        }

        [HttpGet("all-active-staff", Name = "All-Active-Staff")]
        public async Task<IActionResult> GetAllActiveStaff()
        {
            IEnumerable<StaffResponseDto> allStaff = await _staffService.GetAllActiveStaff();

            if (allStaff.Any())
                return Ok(allStaff);

            return BadRequest(new ErrorResponse { Status = ResponseStatus.NOT_FOUND, Message = $"No User found" });
        }

        [HttpGet("all-deleted-staff", Name = "All-Deleted-Staff")]
        public async Task<IActionResult> GetAllDeletedStaff()
        {
            IEnumerable<StaffResponseDto> allStaff = await _staffService.GetAllDeletedStaff();

            if (allStaff.Any())
                return Ok(allStaff);

            return BadRequest(new ErrorResponse { Status = ResponseStatus.NOT_FOUND, Message = $"No User found" });
        }

        [HttpGet("all-paged-staff", Name = "All-Paged-Staff")]
        public async Task<IActionResult> AllPagedStaff(StaffRequestDto request)
        {
            PagedResponse<StaffResponseDto> allStaff = await _staffService.AllStaff(request);

            return Ok(allStaff);
        }

        [HttpGet("all-paged-active-staff", Name = "All-Paged-Active-Staff")]
        public async Task<IActionResult> AllPagedActiveStaff(StaffRequestDto request)
        {
            PagedResponse<StaffResponseDto> allStaff = await _staffService.AllActiveStaff(request);

            return Ok(allStaff);
        }

        [HttpGet("all-paged-deleted-staff", Name = "All-Paged-Deleted-Staff")]
        public async Task<IActionResult> AllPagedDeletedStaff(StaffRequestDto request)
        {
            PagedResponse<StaffResponseDto> allStaff = await _staffService.AllDeletedStaff(request);

            return Ok(allStaff);
        }


        [HttpGet("staff-by-id", Name = "Staff-By-Id")]
        public async Task<IActionResult> GetStaffById(Guid id)
        {
            StaffResponseDto staff = await _staffService.GetStaff(id);

            return Ok(staff);
        }

        [HttpGet("staff-by-email", Name = "Staff-By-Email")]
        public async Task<IActionResult> GetStaffByEmail(string email)
        {
            StaffResponseDto staff = await _staffService.GetStaffByEmail(email);

            return Ok(staff);
        }

        [HttpPost("create-staff", Name = "Create-Staff")]
        public async Task<IActionResult> CreateStaff([FromQuery] CreateStaffRequestDto model)
        {
            string staff = await _staffService.CreateStaff(model);

            return Ok(staff);
        }
                
        [HttpPatch("update-staff", Name = "Update-Staff")]
        public async Task<IActionResult> UpdateStaff(Guid Id, JsonPatchDocument<UpdateStaffDto> model)
        {
            string staff = await _staffService.UpdateStaff(Id, model);

            return Ok(staff);
        }

        [HttpPut("edit-staff", Name = "Edit-Staff")]
        public async Task<IActionResult> EditStaff([FromQuery] Guid staffId, UpdateStaffDto model)
        {
            string staff = await _staffService.EditStaff(staffId, model);

            return Ok(staff);
        }

        [HttpPatch("patch-staff-address", Name = "Patch-Staff-Address")]
        public async Task<IActionResult> PatchStaffAddress(Guid Id, JsonPatchDocument<UpdateAddressDto> model)
        {
            string staff = await _staffService.PatchStaffAddress(Id, model);

            return Ok(staff);
        }

        [HttpPut("toggle-staff-status", Name = "Toggle-Staff-Status")]
        public async Task<IActionResult> ToggleStaffStatus([FromQuery] Guid staffId)
        {
            string staff = await _staffService.ToggleStaffStatus(staffId);

            return Ok(staff);
        }

        [HttpPut("update-staff-address", Name = "Update-Staff-Address")]
        public async Task<IActionResult> UpdateStaffAddress([FromQuery] Guid staffId, UpdateAddressDto model)
        {
            string staff = await _staffService.UpdateStaffAddress(staffId, model);

            return Ok(staff);
        }

        [HttpGet("total-number-of-staff", Name = "Total-Number-Of-Staff")]
        public IActionResult GetTotalNumberOfStaff()
        {
            int staff = _staffService.GetTotalNumberOfStaff().Count();

            if (staff <= 0)
                return BadRequest(new ErrorResponse { Status = ResponseStatus.NOT_FOUND, Message = $"0 Staff found" });

            return Ok($"{staff} Staff");
        }

        [HttpDelete("delete-staff-by-id", Name = "Delete-Staff-By-Id")]
        public async Task<IActionResult> DeleteStaff([FromQuery] Guid id)
        {
            string staff = await _staffService.DeleteStaffById(id);

            return Ok(staff);
        }
    }
}
