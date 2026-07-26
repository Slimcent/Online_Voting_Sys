using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces
{
    public interface IUserService
    {
        Task<Result<string>> CreateUser(CreateUserRequest request);

        Task<Result<LoggedInUserResponse>> UserLogin(LoginRequest request);

        Task<Result<string>> VerifyUser(VerifyAccountRequest request);

        Task<Result<string>> ResetPassword(ResetPasswordRequest request);

        Task<Result<string>> ChangePassword(string userId, ChangePasswordRequest request);

        Task<Result<string>> UpdateRecoveryEmail(string userId, string email);

        Task<Result<string>> ChangeEmail(string userId, ChangeEmailRequestDto request);
    }
}