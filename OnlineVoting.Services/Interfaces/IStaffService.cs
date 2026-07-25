using Microsoft.AspNetCore.JsonPatch;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Services.Interfaces
{
    public interface IStaffService
    {
        Task<string> CreateStaff(CreateStaffRequest request);
        Task<string> UpdateStaffAddress(Guid staffId, UpdateAddressRequest request);
        //Task<IEnumerable<StaffResponseDto>> GetAllStaff();
        Task<StaffResponse> GetStaff(Guid id);
        IEnumerable<Staff> GetTotalNumberOfStaff();
        Task<string> DeleteStaffById(Guid id);
        Task<StaffResponse> GetStaffByEmail(string email);
        Task<String> UpdateStaff(Guid id, JsonPatchDocument<UpdateStaffRequest> model);
        Task<String> PatchStaffAddress(Guid staffId, JsonPatchDocument<UpdateAddressRequest> model);
        Task<string> EditStaff(Guid staffId, UpdateStaffRequest request);
        Task<IEnumerable<StaffResponse>> GetAllDeletedStaff();
        Task<IEnumerable<StaffResponse>> GetAllActiveStaff();
        Task<PagedResponse<StaffResponse>> AllStaff(StaffRequest request);
        Task<PagedResponse<StaffResponse>> AllActiveStaff(StaffRequest request);
        Task<PagedResponse<StaffResponse>> AllDeletedStaff(StaffRequest request);
        Task<string> ToggleStaffStatus(Guid id);
    }
}