using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;

namespace OnlineVoting.Services.Interfaces
{
    public interface IUserService
    {
        Task<string> CreateUser(UserCreateRequestDto model);
        Task<LoggedInUserDto> UserLogin(LoginDto request);
        Task<UserClaimsResponseDto> CreateUserClaims(string email, string claimType, string claimValue);
        Task<string> DeleteClaims(UserClaimsRequestDto request);
        Task<EditUserClaimsDto> EditUserClaims(EditUserClaimsDto userClaimsDto);
        Task<IEnumerable<UserClaimsResponseDto>> GetUserClaims(string email);
    }
}
