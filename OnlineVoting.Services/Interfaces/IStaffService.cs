using Microsoft.AspNetCore.JsonPatch;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces
{
    public interface IStaffService
    {
        Task<Result<string>> CreateStaff(CreateStaffRequest request);
        Task<Result<string>> UpdateStaffAddress(Guid staffId, UpdateAddressRequest request);
        //Task<IEnumerable<StaffResponseDto>> GetAllStaff();
        Task<Result<StaffResponse>> GetStaff(Guid id);
        Result<int> GetTotalNumberOfStaff();
        Task<Result<string>> DeleteStaffById(Guid id);
        Task<Result<StaffResponse>> GetStaffByEmail(string email);
        Task<Result<string>> UpdateStaff(Guid id, JsonPatchDocument<UpdateStaffRequest> model);
        Task<Result<string>> PatchStaffAddress(Guid staffId, JsonPatchDocument<UpdateAddressRequest> model);
        Task<Result<string>> EditStaff(Guid staffId, UpdateStaffRequest request);
        Task<Result<IEnumerable<StaffResponse>>> GetAllDeletedStaff();
        Task<Result<IEnumerable<StaffResponse>>> GetAllActiveStaff();
        Task<Result<PagedResponse<StaffResponse>>> AllStaff(StaffRequest request);
        Task<Result<PagedResponse<StaffResponse>>> AllActiveStaff(StaffRequest request);
        Task<Result<PagedResponse<StaffResponse>>> AllDeletedStaff(StaffRequest request);
        Task<Result<string>> ToggleStaffStatus(Guid id);
    }
}