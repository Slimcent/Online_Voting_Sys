using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;

namespace OnlineVoting.Services.Interfaces
{
    public interface IClaimsService
    {
        Task<List<string>> GetRouteNames(string baseUrl);
        Task<UserClaimsResponse> CreateUserClaims(string email, string claimType, string claimValue);
        Task<string> DeleteClaims(UserClaimsRequest request);
        Task<EditUserClaimsRequest> EditUserClaims(EditUserClaimsRequest userClaimsDto);
        Task<IEnumerable<UserClaimsResponse>> GetUserClaims(string email);
    }
}