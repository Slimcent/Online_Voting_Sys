using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces
{
    public interface IClaimsService
    {
        Task<List<string>> GetRouteNames(string baseUrl);
        Task<Result<UserClaimsResponse>> CreateUserClaims(string email, string claimType, string claimValue);
        Task<Result<string>> DeleteClaims(UserClaimsRequest request);
        Task<Result<EditUserClaimsRequest>> EditUserClaims(EditUserClaimsRequest userClaimsDto);
        Task<Result<IEnumerable<UserClaimsResponse>>> GetUserClaims(string email);
    }
}