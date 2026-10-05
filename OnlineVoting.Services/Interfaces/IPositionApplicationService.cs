using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces
{
    public interface IPositionApplicationService
    {
        Task<Result<CreatePositionApplicationResponse>> CreatePositionApplication(CreatePositionApplicationRequest request);
        Task<Result<string>> CancelPositionApplication(string positionApplicationId);
        Task<Result<PagedResponse<PositionApplicationResponse>>> GetMyPositionApplications(PositionApplicationRequest request);
        Task<Result<PositionApplicationResponse>> GetPositionApplication(string positionApplicationId);
        Task<Result<PagedResponse<PositionApplicationResponse>>> GetPositionApplications(PositionApplicationRequest request);
        Task<Result<string>> ApproveOrRejectPositionApplication(ApproveOrRejectPositionApplicationRequest request);
        Task<Result<PagedResponse<PositionApplicationResponse>>> GetPositionApplicationsWithContestants(PositionApplicationWithContestantRequest request);
    }
}
