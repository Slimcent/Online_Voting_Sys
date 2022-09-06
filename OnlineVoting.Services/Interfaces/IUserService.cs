using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Dtos.Response;

namespace OnlineVoting.Services.Interfaces
{
    public interface IUserService
    {
        Task<string> CreateUser(UserCreateRequestDto model);
        Task<LoggedInUserDto> UserLogin(LoginDto request);
        Task<string> VerifyUser(VerifyAccountRequestDto request);
        Task<string> ResetPassword(ResetPasswordRequestDto request);
        Task<string> ChangePassword(string userId, ChangePasswordRequestDto request);
        Task<string> UpdateRecoveryEmail(string userId, string email);
        Task<string> ChangeEmail(string userId, ChangeEmailRequestDto request);
    }
}
