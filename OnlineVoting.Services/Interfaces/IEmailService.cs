using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendVoterEmail(VoterEmailDto request);
        Task SendCreateUserEmail(UserMailDto request);
        Task<Result<string>> SendResetPasswordEmail(string email);
        Task<Result<string>> SendChangeEmail(ChangeEmailRequest request);
    }
}