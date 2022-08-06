using OnlineVoting.Models.Dtos.Request.Email;

namespace OnlineVoting.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendVoterEmail(VoterEmailDto request);
        Task SendCreateUserEmail(UserMailDto request);
    }
}
