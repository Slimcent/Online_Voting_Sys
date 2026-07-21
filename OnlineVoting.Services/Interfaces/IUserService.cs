using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Dtos.Response;

namespace OnlineVoting.Services.Interfaces
{
    public interface IUserService
    {
        Task<string> CreateUser(CreateUserRequest model);
        Task<LoggedInUserResponse> UserLogin(LoginRequest request);
        Task<string> VerifyUser(VerifyAccountRequest request);
        Task<string> ResetPassword(ResetPasswordRequestDto request);
        Task<string> ChangePassword(string userId, ChangePasswordRequest request);
        Task<string> UpdateRecoveryEmail(string userId, string email);
        Task<string> ChangeEmail(string userId, ChangeEmailRequestDto request);
    }
}
